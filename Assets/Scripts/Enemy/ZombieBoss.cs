using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum ZombieBossState
{
    Idle = 0,
    Patrol,
    Chasing,
    Attack,
    Dead
}

public class ZombieBoss : MonoBehaviourPun, IPunObservable
{
    private LayerMask targetLayer;

    // 컴포넌트 관련
    private NavMeshAgent pathFinder;
    private AudioSource audioSource;
    private Animator animator;
    private Renderer zombieRenderer;
    private Rigidbody zombieRigidbody;
    private ZombieHealth zombieHealth;

    private ZombieBossState state;
    public ZombieBossState State { get; }

    private FSM<ZombieBossState> fsm;

    private HealthComponent target;

    // 공격 관련
    private float damage = 30.0f;
    private float timeBetAttack = 5.0f;
    private float attackRange = 2.0f;
    private float detectRange = 20.0f;

    private float rotSpeed = 30.0f;

    // 동기화 관련
    private Vector3 serializedPosition;
    private Quaternion serializedRotation;

    private void Awake()
    {
        targetLayer = LayerMask.NameToLayer("Character");

        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        pathFinder = GetComponent<NavMeshAgent>();
        zombieRenderer = GetComponent<Renderer>();
        zombieRigidbody = GetComponent<Rigidbody>();
        zombieHealth = GetComponent<ZombieHealth>();

        fsm = new FSM<ZombieBossState>(this);
        pathFinder.isStopped = true;

        zombieHealth.OnDeath += OnDead;
    }

    // Start is called before the first frame update
    void Start()
    {
        fsm.StartFSM(ZombieBossState.Idle);
    }

    // Update is called once per frame
    void Update()
    {
        fsm.OnUpdate();
    }

    public void Idle_Enter()
    {
        state = ZombieBossState.Idle; 
    }

    public void Idle_Update()
    {
        DetectTarget();
    }

    public void Idle_Exit()
    {

    }

    private void DetectTarget()
    {
        if(false == PhotonNetwork.IsMasterClient)
        {
            return;
        }

        // Overlap을 통해 candidate 리스트 가져오기
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectRange, targetLayer);

        for(int i = 0; i < colliders.Length; i++)
        {
            HealthComponent entity = colliders[i].GetComponent<HealthComponent>();
            if(null == entity || true == entity.Dead || this == entity)
            {
                continue;
            }

            if(null == target)
            {
                target = entity;
            }
            else
            {
                if(Vector3.Distance(target.transform.position, transform.position) > 
                    Vector3.Distance(entity.transform.position, transform.position))
                {
                    target = entity;
                }
            }
        }

        if(null != target)
        {
            photonView.RPC("TransitionState", RpcTarget.All, (int)ZombieBossState.Chasing);
        }
    }

    public void Patrol_Enter()
    {
        photonView.RPC("SetMove", RpcTarget.All, true);
    }

    public void Patrol_Update()
    {

    }

    public void Patrol_Exit()
    {
        photonView.RPC("SetMove", RpcTarget.All, false);
    }

    private void MoveToTarget()
    {
        if(false == PhotonNetwork.IsMasterClient)
        {
            transform.position = Vector3.Lerp(transform.position, serializedPosition, Time.deltaTime * pathFinder.speed);
            transform.rotation = Quaternion.Slerp(transform.rotation, serializedRotation, Time.deltaTime * rotSpeed);
            return;
        }

        if(null == target)
        {
            photonView.RPC("TransitionState", RpcTarget.All, (int)ZombieBossState.Idle);
            return;
        }

        pathFinder.SetDestination(target.transform.position);

        // 공격 범위 내에 들었을 때
        if(Vector3.Distance(target.transform.position, transform.position) <= attackRange + 1.0f)
        {
            pathFinder.isStopped = true;

            Quaternion rot = Quaternion.LookRotation(target.transform.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, rotSpeed * Time.deltaTime);

            fsm.Transition(ZombieBossState.Attack);
        }
    }

    public void Chasing_Enter()
    {
        Debug.Log("Chasing enter");
        pathFinder.isStopped = false;

        photonView.RPC("SetMove", RpcTarget.All, true);
    }

    public void Chasing_Update()
    {
        MoveToTarget();
    }

    public void Chasing_Exit()
    {
        photonView.RPC("SetMove", RpcTarget.All, false);
    }

    public void Attack_Enter()
    {
        photonView.RPC("PlayNormalAttack", RpcTarget.All);
        fsm.Transition(ZombieBossState.Chasing);
    }

    public void Dead_Enter()
    {
        pathFinder.isStopped = true;
    }

    [PunRPC]
    private void TransitionState(int nextState)
    {
        fsm.Transition((ZombieBossState)nextState);
    }

    [PunRPC]
    private void SetMove(bool value)
    {
        animator.SetBool("Move", value);
    }

    [PunRPC]
    private void PlayNormalAttack()
    {
        animator.SetTrigger("NormalAttack");
    }

    private void HitCheck()
    {

    }

    private void OnDead()
    {
        fsm.Transition(ZombieBossState.Dead);
    }

    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(true == stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            serializedPosition = (Vector3)stream.ReceiveNext();
            serializedRotation = (Quaternion)stream.ReceiveNext();
        }
    }

}
