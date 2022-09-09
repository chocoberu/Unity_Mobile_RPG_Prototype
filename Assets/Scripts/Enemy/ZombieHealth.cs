using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieHealth : HealthComponent
{
    private Animator animator;
    private AudioSource audioSource;
    private NavMeshAgent pathFinder;
    private Renderer enemyRenderer;
    private Rigidbody enemyRigidbody;
    private ZombieBase zombieComponent;

    // 효과음
    public AudioClip deathSound;
    public AudioClip hitSound;

    // 이펙트
    private ParticleSystem hitEffect;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        pathFinder = GetComponent<NavMeshAgent>();
        enemyRenderer = GetComponent<Renderer>();
        enemyRigidbody = GetComponent<Rigidbody>();
        zombieComponent = GetComponent<ZombieBase>();

        hitEffect = transform.Find("BloodSplatDirectional").GetComponent<ParticleSystem>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [PunRPC]
    public override void OnDamage(float damage, Vector3 hitPosition, Vector3 hitNormal, int AttackerTeamNumber)
    {
        if(false == Dead)
        {
            hitEffect.transform.rotation = Quaternion.LookRotation(hitNormal);
            hitEffect.Play();

            audioSource.PlayOneShot(hitSound);
        }
        
        base.OnDamage(damage, hitPosition, hitNormal, AttackerTeamNumber);
    }

    public override void Die()
    {
        base.Die();

        enemyRigidbody.isKinematic = true;

        // 좀비 관련 Collider를 사용하지 않도록 변경
        Collider[] enemyColliders = GetComponents<Collider>();
        for (int i = 0; i < enemyColliders.Length; i++)
        {
            enemyColliders[i].enabled = false;
        }

        // NavMeshAgent 사용하지 않도록 변경
        pathFinder.isStopped = true;
        pathFinder.enabled = false;

        animator.SetTrigger("Die");
        audioSource.PlayOneShot(deathSound);
    }
}
