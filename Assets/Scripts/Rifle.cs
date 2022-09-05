using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rifle : MonoBehaviourPun, IWeaponable
{
    [SerializeField]
    private float attackDamage = 10.0f;
    public float fireDistance = 30.0f;

    public float AttackDamage { get { return attackDamage; } }

    private PlayerAttack playerAttack;
    private Animator playerAnimator;
    private PlayerHealth playerHealth;

    private LineRenderer bulletLineRenderer;
    private AudioSource audioPlayer;
    
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

    public void StartAttack()
    {
        //Debug.Log($"{player.gameObject.name} start attack");

        if(true == isReloading)
        {
            return;
        }

        isPressed = true;
        StartCoroutine(Shot());
    }

    public void StopAttack()
    {
        //Debug.Log($"{player.gameObject.name} stop attack");

        isPressed = false;
        StopCoroutine(Shot());
    }

    private IEnumerator Shot()
    {
        while(true == isPressed)
        {
            // ���� ���� ó���� ȣ��Ʈ�� ����
            photonView.RPC("Attack", RpcTarget.MasterClient);

            yield return new WaitForSeconds(timeBetFire);
        }
    }

    [PunRPC]
    private void Attack()
    {
        if(false == PhotonNetwork.IsMasterClient)
        {
            return;
        }

        // ������, ���� ������
        if(true == isReloading || Time.time < lastFireTime + timeBetFire)
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

            if (null != target && playerHealth.GetTeamNumber() != target.GetTeamNumber())
            {
                target.OnDamage(AttackDamage, hit.point, hit.normal);
            }
            hitPosition = hit.point;
        }
        else
        {
            hitPosition = fireTransform.position + fireTransform.forward * fireDistance;
        }

        // �߻� ���� ó��
        photonView.RPC("OnAttackProcessClient", RpcTarget.All, hitPosition);
    }

    [PunRPC]
    private void OnAttackProcessClient(Vector3 hitPosition)
    {
        magAmmo--;
        if(magAmmo <= 0)
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
