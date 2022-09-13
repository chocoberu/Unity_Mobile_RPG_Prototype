using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Rifle : MonoBehaviourPun, IWeaponable
{
    [SerializeField]
    private float attackDamage = 10.0f;
    public float fireDistance = 30.0f;

    public float AttackDamage { get { return attackDamage; } }

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

    // �ѱ� ��ƼŬ
    public ParticleSystem muzzleFlashEffect;
    public ParticleSystem shellEjectEffect;

    // �ѱ� ȿ����
    public AudioClip shotClip;
    public AudioClip reloadClip;

    public int magAmmo; // ���� źâ�� ź�� �� 
    public int magCapacity = 10; // źâ �뷮

    [SerializeField]
    private float timeBetFire = 0.2f; // ���� �ӵ�
    [SerializeField]
    private float reloadTime = 2.0f; // ������ �ӵ�
    private float lastFireTime;

    private bool isPressed;
    private bool isReloading;

    private void Awake()
    {
        bulletLineRenderer = GetComponent<LineRenderer>();
        audioPlayer = GetComponent<AudioSource>();
        
        meshRendererList = GetComponentsInChildren<MeshRenderer>().ToList();

        fireTransform = transform.Find("FireTransform");
        
        // ����� ���� 2���� ����
        bulletLineRenderer.positionCount = 2;
        // ���� ������ ��Ȱ��ȭ
        bulletLineRenderer.enabled = false;
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

        magAmmo = magCapacity;
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
        playerMovement.RotationFix = true;

        // ���� ���� ó���� ȣ��Ʈ�� ����
        photonView.RPC("Server_StartAttack", RpcTarget.MasterClient);
    }

    public void StopAttack()
    {
        Debug.Log("StopAttack() called");
        photonView.RPC("Server_StopAttack", RpcTarget.MasterClient);
    }

    [PunRPC]
    private void Server_StartAttack()
    {
        if(false == PhotonNetwork.IsMasterClient)
        {
            return;
        }

        isPressed = true;
        StartCoroutine(Shot());
    }

    [PunRPC]
    private void Server_StopAttack()
    {
        if (false == PhotonNetwork.IsMasterClient)
        {
            return;
        }

        isPressed = false;
        StopCoroutine(Shot());
    }

    [PunRPC]
    private void SetRotationFix(bool value)
    {
        playerMovement.RotationFix = value;
    }

    private IEnumerator Shot()
    {
        while(true == isPressed)
        {
            Attack();
            yield return new WaitForSeconds(timeBetFire);
            
            if(false == isPressed)
            {
                photonView.RPC("SetRotationFix", RpcTarget.All, false);
            }
        }
    }

    private void Attack()
    {
        if(false == PhotonNetwork.IsMasterClient)
        {
            return;
        }

        // ������, ������ �������� üũ
        if(true == isReloading || PlayerMoveState.Roll == playerMovement.MoveState)
        {
            StopAttack();
            return;
        }

        RaycastHit hit;
        Vector3 hitPosition = Vector3.zero;

        HealthComponent target = null;
        if (true == Physics.Raycast(fireTransform.position, fireTransform.forward, out hit, fireDistance))
        {
            target = hit.collider.GetComponent<HealthComponent>();

            if (null != target)
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

        lastFireTime = Time.time;
        // �߻� ���� ó��
        photonView.RPC("OnAttackProcessClient", RpcTarget.All, hitPosition);
    }

    [PunRPC]
    private void OnAttackProcessClient(Vector3 hitPosition)
    {
        magAmmo--;
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

        // ź�� ���� ó��
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

        magAmmo += magCapacity;
        isReloading = false;
    }
}
