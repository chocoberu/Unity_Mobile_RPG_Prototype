using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieMinion : ZombieBase
{

    protected override void Awake()
    {
        base.Awake();

        damage = 15.0f;
        attackRange = 1.5f;

        zombieHealth.OnDeath += OnDead;
    }

    // Start is called before the first frame update
    void Start()
    {
        fsm.StartFSM(ZombieState.Idle);
    }

    // Update is called once per frame
    void Update()
    {
        fsm.OnUpdate();
    }

    public override void Idle_Enter()
    {
        base.Idle_Enter();
    }

    public override void Idle_Update()
    {
        DetectTarget();
    }

    private void DetectTarget()
    {
        if (false == PhotonNetwork.IsMasterClient)
        {
            return;
        }

        // Overlap�� ���� candidate ����Ʈ ��������
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectRange, targetLayer);

        for (int i = 0; i < colliders.Length; i++)
        {
            HealthComponent entity = colliders[i].GetComponent<HealthComponent>();
            if (null == entity || true == entity.Dead || this == entity)
            {
                continue;
            }

            // ���� ����� Player�� Ÿ������ ����
            if (null == target)
            {
                target = entity;
            }
            else
            {
                if (Vector3.Distance(target.transform.position, transform.position) >
                    Vector3.Distance(entity.transform.position, transform.position))
                {
                    target = entity;
                }
            }
        }

        if (null != target)
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

        if (true == PhotonNetwork.IsMasterClient)
        {
            pathFinder.isStopped = false;
            Vector3 randomPatrolPos = transform.position + Random.insideUnitSphere * 3.0f;
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
        base.Patrol_Exit();
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

        if (null == target)
        {
            photonView.RPC("TransitionState", RpcTarget.All, (int)ZombieState.Idle);
            return;
        }
        // ���� Ÿ�ٰ��� �Ÿ� �Ǵ�, Ž�� ������ ����� ���, �Ǵ� Ÿ���� ���� ���
        if (Vector3.Distance(target.transform.position, transform.position) > detectRange || true == target.Dead)
        {
            target = null;
            photonView.RPC("TransitionState", RpcTarget.All, (int)ZombieState.Idle);
            return;
        }

        pathFinder.SetDestination(target.transform.position);

        // ���� ���� ���� ����� ��
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

    protected override void OnDead()
    {
        base.OnDead();
    }
}
