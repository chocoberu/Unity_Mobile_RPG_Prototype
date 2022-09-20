using Photon.Pun;
using System;
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

    // ȿ����
    public AudioClip deathSound;
    public AudioClip hitSound;

    // ����Ʈ
    private ParticleSystem hitEffect;

    public event Action OnUpdate;

    protected override void Awake()
    {
        base.Awake();

        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        pathFinder = GetComponent<NavMeshAgent>();
        enemyRenderer = GetComponent<Renderer>();
        enemyRigidbody = GetComponent<Rigidbody>();
        zombieComponent = GetComponent<ZombieBase>();

        hitEffect = transform.Find("BloodSplatDirectional").GetComponent<ParticleSystem>();
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

        if(null != OnUpdate)
        {
            OnUpdate.Invoke();
        }    
    }

    public override void Die()
    {
        base.Die();

        enemyRigidbody.isKinematic = true;

        // ���� ���� Collider�� ������� �ʵ��� ����
        Collider[] enemyColliders = GetComponents<Collider>();
        for (int i = 0; i < enemyColliders.Length; i++)
        {
            enemyColliders[i].enabled = false;
        }

        // NavMeshAgent ������� �ʵ��� ����
        pathFinder.isStopped = true;
        pathFinder.enabled = false;

        animator.SetTrigger("Die");
        audioSource.PlayOneShot(deathSound);
    }
}
