using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieBoss : ZombieBase
{
    enum ZombieBossPhase
    {
        Phase1 = 0,
        Phase2
    }

    // 공격 관련
    private ZombieBossPhase bossPhase;
    private float phase2Percent = 0.5f;
    private float minionSpawnTime = 10.0f;

    protected override void Awake()
    {
        base.Awake();

        zombieHealth.OnDeath += OnDead;
        zombieHealth.OnUpdate += UpdatePhase;

        bossPhase = ZombieBossPhase.Phase1;
    }

    // Start is called before the first frame update
    void Start()
    {
        //fsm.StartFSM(ZombieBossState.Idle);
    }

    // Update is called once per frame
    void Update()
    {
        fsm.OnUpdate();
    }

    public override void Idle_Enter()
    {
        pathFinder.isStopped = true; 
    }

    public override void Idle_Update()
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

            // 가장 가까운 Player를 타겟으로 설정
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
            photonView.RPC("TransitionState", RpcTarget.All, (int)ZombieState.Chasing);
        }
        else
        {
            photonView.RPC("TransitionState", RpcTarget.All, (int)ZombieState.Patrol);
        }
    }

    public override void Patrol_Enter()
    {
        base.Patrol_Enter();
        
        if(true == PhotonNetwork.IsMasterClient)
        {
            pathFinder.isStopped = false;
            Vector3 randomPatrolPos = transform.position + Random.insideUnitSphere * 5.0f;
            pathFinder.SetDestination(randomPatrolPos);
        }
    }

    public override void Patrol_Update()
    {
        if (false == PhotonNetwork.IsMasterClient)
        {
            MoveToTarget();
            return;
        }

        if (Vector3.Distance(pathFinder.destination, transform.position) <= 1.0f)
        {
            pathFinder.isStopped = true;

            photonView.RPC("TransitionState", RpcTarget.All, (int)ZombieState.Idle);
        }
    }

    public override void Patrol_Exit()
    {
        base.Patrol_Enter();
    }

    public override void Chasing_Enter()
    {
        base.Chasing_Enter();
    }

    public override void Chasing_Update()
    {
        if (false == PhotonNetwork.IsMasterClient)
        {
            MoveToTarget();
            return;
        }

        if(null == target)
        {
            photonView.RPC("TransitionState", RpcTarget.All, (int)ZombieState.Idle);
            return;
        }
        // 현재 타겟과의 거리 판단, 탐색 범위를 벗어났을 경우, 또는 타겟이 죽은 경우
        if (Vector3.Distance(target.transform.position, transform.position) > detectRange || true == target.Dead)
        {
            target = null;
            photonView.RPC("TransitionState", RpcTarget.All, (int)ZombieState.Idle);
            return;
        }

        pathFinder.SetDestination(target.transform.position);
                        
        // 공격 범위 내에 들었을 때
        if (Vector3.Distance(pathFinder.destination, transform.position) <= attackRange + 1.0f)
        {
            pathFinder.isStopped = true;

            Quaternion rot = Quaternion.LookRotation(target.transform.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, rotSpeed * Time.deltaTime);

            fsm.Transition(ZombieState.Attack);
        }
    }

    public override void Chasing_Exit()
    {
        base.Chasing_Exit();
    }

    public override void Attack_Enter()
    {
        photonView.RPC("PlayNormalAttack", RpcTarget.All);
    }

    public override void Dead_Enter()
    {
        base.Dead_Enter();
    }

    private void UpdatePhase()
    {
        if(false == PhotonNetwork.IsMasterClient)
        {
            return;
        }

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

        // TODO : 미니언 스폰 코루틴 동작
        if(true == PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(CoSpawnMinion());
        }
    }

    private IEnumerator CoSpawnMinion()
    {
        while(false == zombieHealth.Dead)
        {
            GameObject minion = PhotonNetwork.InstantiateRoomObject("ZombieMinion", transform.position + Random.insideUnitSphere * 7.0f, Quaternion.identity);
            yield return new WaitForSeconds(minionSpawnTime);
        }
    }

    protected override void OnDead()
    {
        base.OnDead();

        Debug.Log("Stop Spawn");
        StopCoroutine(CoSpawnMinion());
    }

}
