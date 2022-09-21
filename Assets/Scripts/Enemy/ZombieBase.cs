using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum ZombieState
{
    Idle = 0,
    Patrol,
    Chasing,
    Attack,
    Dead
}

public class ZombieBase : MonoBehaviourPun, IPunObservable
{
    protected LayerMask targetLayer;

    // 컴포넌트 관련
    protected NavMeshAgent pathFinder;
    protected AudioSource audioSource;
    protected Animator animator;
    protected Renderer zombieRenderer;
    protected Rigidbody zombieRigidbody;
    protected ZombieHealth zombieHealth;

    protected FSM<ZombieState> fsm;

    // 공격 관련
    protected HealthComponent target;
    protected float damage = 30.0f;
    protected float timeBetAttack = 5.0f;
    protected float attackRange = 2.0f;
    protected float attackRadius = 1.0f;
    protected float detectRange = 20.0f;
    protected ParticleSystem hitEffect;

    // 이동 관련
    protected float moveSpeed = 3.5f;
    protected float rotationSpeed = 30.0f;

    // 동기화 관련
    protected Vector3 serializedPosition;
    protected Quaternion serializedRotation;

    protected virtual void Awake()
    {
        targetLayer = LayerMask.NameToLayer("Character");

        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        pathFinder = GetComponent<NavMeshAgent>();
        zombieRenderer = GetComponentInChildren<Renderer>();
        zombieRigidbody = GetComponent<Rigidbody>();
        zombieHealth = GetComponent<ZombieHealth>();

        pathFinder.enabled = false;
        pathFinder.enabled = true;
        pathFinder.isStopped = true;

        hitEffect = transform.Find("SoftRadialPunchMedium").GetComponent<ParticleSystem>();
        
        fsm = new FSM<ZombieState>(this);
    }

    public virtual void Idle_Enter()
    {
        pathFinder.isStopped = true;
    }

    public virtual void Idle_Update()
    {

    }

    public virtual void Idle_Exit()
    {

    }

    public virtual void Patrol_Enter()
    {
        photonView.RPC("SetMove", RpcTarget.All, true);
    }

    public virtual void Patrol_Update()
    {

    }

    public virtual void Patrol_Exit()
    {
        photonView.RPC("SetMove", RpcTarget.All, false);
    }

    public virtual void Chasing_Enter()
    {
        pathFinder.isStopped = false;
        photonView.RPC("SetMove", RpcTarget.All, true);
    }

    public virtual void Chasing_Update()
    {

    }

    public virtual void Chasing_Exit()
    {
        photonView.RPC("SetMove", RpcTarget.All, false);
        pathFinder.isStopped = true;
    }

    public virtual void Attack_Enter()
    {

    }

    public virtual void Dead_Enter()
    {
        pathFinder.isStopped = true;
    }

    protected void UpdateTransform()
    {
        if (false == PhotonNetwork.IsMasterClient)
        {
            transform.position = Vector3.Lerp(transform.position, serializedPosition, Time.deltaTime * pathFinder.speed);
            transform.rotation = Quaternion.Slerp(transform.rotation, serializedRotation, Time.deltaTime * rotationSpeed);
        }
    }

    public void StartFSM()
    {
        fsm.StartFSM(ZombieState.Idle);
    }

    [PunRPC]
    protected void SetMove(bool value)
    {
        animator.SetBool("Move", value);
    }

    [PunRPC]
    protected void PlayNormalAttack()
    {
        animator.SetTrigger("NormalAttack");
    }

    protected Vector3 GetRandomPointOnNavMesh(Vector3 position, float distance)
    {
        // position 중심으로 반지름이 distance 구 안에서의 랜덤한 위치 하나를 저장
        Vector3 randomPos = Random.insideUnitSphere * distance + position;

        // 내비메시 샘플링의 결과 정보를 저장하는 변수
        NavMeshHit hit;
        // maxDistance 반경 안에서, randomPos에 가장 가까운 내비메시 위의 한 점을 찾음
        NavMesh.SamplePosition(randomPos, out hit, distance, NavMesh.AllAreas);

        // 찾은 점 반환
        return hit.position;
    }

    protected void NormalAttackHitCheck()
    {
        hitEffect?.Play();       
        Debug.DrawLine(transform.position, transform.position + transform.forward * attackRange, Color.red, 2.0f);

        if (false == PhotonNetwork.IsMasterClient || true == zombieHealth.Dead)
        {
            return;
        }

        // Overlap을 통해 충돌 검사
        Collider[] colliders = Physics.OverlapCapsule(transform.position, transform.position + transform.forward * (attackRange - attackRadius), attackRadius, targetLayer);
        Debug.DrawLine(transform.position, transform.position + transform.forward * attackRange, Color.red, 2.0f);

        for (int i = 0; i < colliders.Length; i++)
        {
            HealthComponent entity = colliders[i].GetComponent<HealthComponent>();
            if (null != entity && false == entity.Dead && this != entity && false == zombieHealth.Dead)
            {
                Debug.Log($"Length : {Vector3.Distance(entity.transform.position, transform.position)}");
                entity.OnDamage(damage, colliders[i].transform.position, colliders[i].transform.up, zombieHealth.GetTeamNumber());
                break;
            }
        }
        
        fsm.Transition(ZombieState.Chasing);
    }

    protected virtual void OnDead()
    {
        Debug.Log("OnDead");
        fsm.Transition(ZombieState.Dead);
    }

    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (true == stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            serializedPosition = (Vector3)stream.ReceiveNext();
            serializedRotation = (Quaternion)stream.ReceiveNext();

            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            serializedPosition += lag * zombieRigidbody.velocity;
        }
    }
}
