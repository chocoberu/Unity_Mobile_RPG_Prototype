using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Rifle : MonoBehaviourPun, IWeaponable
{
    [SerializeField]
    private float attackDamage = 10.0f;
    public float fireDistance = 20.0f;

    public float AttackDamage { get { return attackDamage; } }

    public WeaponType weaponType { get { return _weaponType; } }
    private WeaponType _weaponType = WeaponType.Gun;

    private PlayerAttack playerAttack;
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;
    private PlayerState playerState;

    private Animator playerAnimator;
    private LineRenderer bulletLineRenderer;
    private AudioSource audioPlayer;
    private List<MeshRenderer> meshRendererList = new List<MeshRenderer>();

    private Transform fireTransform;
    public LayerMask targetLayer;
    //private PlayerState playerState;

    // 총기 파티클
    public ParticleSystem muzzleFlashEffect;
    public ParticleSystem shellEjectEffect;

    // 총기 효과음
    public AudioClip shotClip;
    public AudioClip reloadClip;

    // 현재 탄창의 탄알 수 
    private int magAmmo;
    public int MagAmmo { get { return magAmmo; } private set { magAmmo = value; OnUpdateAmmo?.Invoke(magAmmo); } }
    
    [SerializeField]
    private int magCapacity = 10; // 탄창 용량

    [SerializeField]
    private float timeBetFire = 0.2f; // 공격 속도
    [SerializeField]
    private float reloadTime = 2.0f; // 재장전 속도
    private float lastFireTime;

    private bool isPressed;
    private bool isReloading;

    public Action<int> OnUpdateAmmo;

    private void Awake()
    {
        bulletLineRenderer = GetComponent<LineRenderer>();
        audioPlayer = GetComponent<AudioSource>();
        
        meshRendererList = GetComponentsInChildren<MeshRenderer>().ToList();

        fireTransform = transform.Find("FireTransform");
        
        // 사용할 점을 2개로 설정
        bulletLineRenderer.positionCount = 2;
        // 라인 렌더러 비활성화
        bulletLineRenderer.enabled = false;

        GameObject virtualPad = GameObject.Find("Virtual Pad");
        RightButtons rightButtons = Utils.FindChild<RightButtons>(virtualPad, null, true);
        if(null != rightButtons && true == photonView.IsMine)
        {
            OnUpdateAmmo += rightButtons.OnUpdateAttackCount;
        }
    }

    private void OnEnable()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            if (photonView.CreatorActorNr != player.GetComponent<PhotonView>().CreatorActorNr)
            {
                continue;
            }

            playerAttack = player.GetComponent<PlayerAttack>();
            playerHealth = player.GetComponent<PlayerHealth>();
            playerMovement = player.GetComponent<PlayerMovement>();
            playerState = player.GetComponent<PlayerState>();
            playerAnimator = player.GetComponent<Animator>();
            break;
        }
        if (false == photonView.IsMine)
        {
            playerAttack.SetupWeapon(gameObject);
        }

        MagAmmo = magCapacity;        
        lastFireTime = 0.0f;
        
        isPressed = false;
        isReloading = false;
    }

    public void SetWeaponVisible(bool value)
    {
        for (int i = 0; i < meshRendererList.Count; i++)
        {
            meshRendererList[i].enabled = value;
        }
    }

    public void StartAttack()
    {
        //Debug.Log($"{player.gameObject.name} start attack");
        if(true == isReloading || PlayerMoveState.Roll == playerMovement.MoveState)
        {
            return;
        }

        isPressed = true;
        playerMovement.RotationFix = true;
        StartCoroutine(Shot());
    }

    public void StopAttack()
    {
        //Debug.Log("StopAttack() called");
        
        isPressed = false;
        StopCoroutine(Shot());
    }

    private IEnumerator Shot()
    {
        while(true == isPressed)
        {
            // 실제 공격 처리는 호스트에 위임
            photonView.RPC("Attack", RpcTarget.MasterClient, transform.forward);
            yield return new WaitForSeconds(timeBetFire);
            
            if(false == isPressed)
            {
                playerMovement.RotationFix = false;
            }
        }
    }

    [PunRPC]
    private void Attack(Vector3 fireDireciton, PhotonMessageInfo info)
    {
        if(false == PhotonNetwork.IsMasterClient)
        {
            return;
        }

        float lag = Mathf.Max(0.0f, (float)(PhotonNetwork.Time - info.SentServerTime));
        
        // Reload, Roll, 연사 서버 검증
        if(true == isReloading || PlayerMoveState.Roll == playerMovement.MoveState || (float)Time.time < lastFireTime + timeBetFire - lag)
        {
            Debug.Log($"Time.time : {Time.time}, lastFireTime : {(double)lastFireTime}, timeBetFire : {(double)timeBetFire} lag : {(double)lag}");
            StopAttack();
            return;
        }

        RaycastHit hit;
        Vector3 hitPosition = Vector3.zero;

        HealthComponent target = null;
        if (true == Physics.Raycast(fireTransform.position, fireDireciton, out hit, fireDistance))
        {
            target = hit.collider.GetComponent<HealthComponent>();

            if (null != target && false == target.Dead)
            {
                target.OnDamage(AttackDamage, hit.point, hit.normal, playerHealth.GetTeamNumber());
                if(true == target.Dead)
                {
                    playerState.KillScore++;
                }
            }
            hitPosition = hit.point;
        }
        else
        {
            hitPosition = fireTransform.position + fireTransform.forward * fireDistance;
        }

        lastFireTime = Time.time - lag;
        // 발사 이후 처리
        photonView.RPC("OnAttackProcessClient", RpcTarget.All, hitPosition);
    }

    [PunRPC]
    private void OnAttackProcessClient(Vector3 hitPosition)
    {
        MagAmmo--;
        
        if (magAmmo <= 0)
        {
            StopAttack();
            StartCoroutine(Reload());
        }

        StartCoroutine(ShotEffect(hitPosition));
    }

    private IEnumerator ShotEffect(Vector3 hitPosition)
    {
        muzzleFlashEffect.Play();
        shellEjectEffect.Play();

        audioPlayer.PlayOneShot(shotClip);

        // 탄알 궤적 처리
        bulletLineRenderer.SetPosition(0, fireTransform.position);
        bulletLineRenderer.SetPosition(1, hitPosition);
        bulletLineRenderer.enabled = true;

        yield return new WaitForSeconds(0.03f);

        bulletLineRenderer.enabled = false;

    }

    private IEnumerator Reload()
    {
        isReloading = true;
        audioPlayer.PlayOneShot(reloadClip);
        playerAnimator.SetTrigger("Reload");
        yield return new WaitForSeconds(reloadTime);

        MagAmmo += magCapacity;
        isReloading = false;
    }
}
