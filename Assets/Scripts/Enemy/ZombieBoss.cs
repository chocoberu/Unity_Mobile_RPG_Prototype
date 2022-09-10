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

enum ZombieBossPhase
{
    Phase1 = 0,
    Phase2
}

public class ZombieBoss : ZombieBase
{ 
    private FSM<ZombieBossState> fsm;

    private HealthComponent target;

    // 공격 관련
    private ZombieBossPhase bossPhase;
    private float damage = 30.0f;
    private float timeBetAttack = 5.0f;
    private float attackRange = 2.0f;
    private float detectRange = 20.0f;
    private float phase2Percent = 0.5f;

    private float rotSpeed = 30.0f;

    protected override void Awake()
    {
        base.Awake();

        fsm = new FSM<ZombieBossState>(this);
        pathFinder.isStopped = true;

        zombieHealth.OnDeath += OnDead;
        zombieHealth.OnUpdate += UpdatePhase;

        bossPhase = ZombieBossPhase.Phase1;
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
        pathFinder.isStopped = true; 
    }

    public void Idle_Update()
    {
        DetectTarget();
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
        else
        {
            photonView.RPC("TransitionState", RpcTarget.All, (int)ZombieBossState.Patrol);
        }
    }

    public void Patrol_Enter()
    {
        photonView.RPC("SetMove", RpcTarget.All, true);
        
        if(true == PhotonNetwork.IsMasterClient)
        {
            pathFinder.isStopped = false;
            Vector3 randomPatrolPos = transform.position + Random.insideUnitSphere * 5.0f;
            pathFinder.SetDestination(randomPatrolPos);
        }
    }

    public void Patrol_Update()
    {
        MoveToTarget();

        if(Vector3.Distance(pathFinder.destination, transform.position) <= 1.0f)
        {
            pathFinder.isStopped = true;

            photonView.RPC("TransitionState", RpcTarget.All, (int)ZombieBossState.Idle);
        }
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
    }

    public void Chasing_Enter()
    {
        Debug.Log("Chasing enter");
        pathFinder.isStopped = false;

        photonView.RPC("SetMove", RpcTarget.All, true);
    }

    public void Chasing_Update()
    {
        if(true == PhotonNetwork.IsMasterClient)
        {
            if(null == target)
            {
                photonView.RPC("TransitionState", RpcTarget.All, (int)ZombieBossState.Idle);
                return;
            }
            pathFinder.SetDestination(target.transform.position);
        }
        
        MoveToTarget();
        
        // 공격 범위 내에 들었을 때
        if (Vector3.Distance(pathFinder.destination, transform.position) <= attackRange + 1.0f)
        {
            pathFinder.isStopped = true;

            Quaternion rot = Quaternion.LookRotation(target.transform.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, rotSpeed * Time.deltaTime);

            fsm.Transition(ZombieBossState.Attack);
        }
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
        Debug.Log($"Transition State : {(ZombieBossState)nextState}");
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

    private void NormalAttackHitCheck()
    {
        if(false == PhotonNetwork.IsMasterClient)
        {
            return;
        }

        // Overlap을 통해 충돌 검사
        Collider[] colliders = Physics.OverlapCapsule(transform.position, transform.position + transform.forward * attackRange, 1.0f, targetLayer);
        for (int i = 0; i < colliders.Length; i++)
        {
            HealthComponent entity = colliders[i].GetComponent<HealthComponent>();
            if (null != entity && false == entity.Dead && this != entity && false == zombieHealth.Dead)
            {
                entity.OnDamage(damage, colliders[i].transform.position, colliders[i].transform.up, zombieHealth.GetTeamNumber());
                break;
            }
        }
    }

    private void UpdatePhase()
    {
        if(zombieHealth.Health / zombieHealth.DefaultHealth <= phase2Percent && bossPhase == ZombieBossPhase.Phase1)
        {
            photonView.RPC("Berserk", RpcTarget.All);
        }
    }

    [PunRPC]
    private void Berserk()
    {
        Debug.Log("Boss Phase 2 Enter");

        bossPhase = ZombieBossPhase.Phase2;
        damage *= 1.5f;

        pathFinder.speed = 5.5f;
        zombieRenderer.material.color = Color.red;
    }

    private void OnDead()
    {
        fsm.Transition(ZombieBossState.Dead);
    }

}
