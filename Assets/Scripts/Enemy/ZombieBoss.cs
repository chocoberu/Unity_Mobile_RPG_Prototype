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

    // ���� ����
    private ZombieBossPhase bossPhase;
    private float phase2Percent = 0.5f;
    private float minionSpawnTime = 10.0f;
    [SerializeField]
    private int maxMinionCount = 4;

    private List<ZombieMinion> minionList = new List<ZombieMinion>();

    protected override void Awake()
    {
        base.Awake();

        moveSpeed = 6.0f;
        pathFinder.speed = moveSpeed;

        zombieHealth.OnDeath += OnDead;
        zombieHealth.OnUpdate += UpdatePhase;

        bossPhase = ZombieBossPhase.Phase1;
    }

    // Update is called once per frame
    void Update()
    {
        if(false == PhotonNetwork.IsMasterClient)
        {
            UpdateTransform();
            return;
        }

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

        // Overlap�� ���� candidate ����Ʈ ��������
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectRange, targetLayer);

        for(int i = 0; i < colliders.Length; i++)
        {
            HealthComponent entity = colliders[i].GetComponent<HealthComponent>();
            if(null == entity || true == entity.Dead || this == entity)
            {
                continue;
            }

            // ���� ����� Player�� Ÿ������ ����
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
            fsm.Transition(ZombieState.Chasing);
        }
        else
        {
            fsm.Transition(ZombieState.Patrol);
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
            return;
        }

        if (Vector3.Distance(pathFinder.destination, transform.position) <= 1.0f)
        {
            pathFinder.isStopped = true;
            fsm.Transition(ZombieState.Idle);
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
            return;
        }

        if(null == target)
        {
            fsm.Transition(ZombieState.Idle);
            return;
        }
        // ���� Ÿ�ٰ��� �Ÿ� �Ǵ�, Ž�� ������ ����� ���, �Ǵ� Ÿ���� ���� ���
        if (Vector3.Distance(target.transform.position, transform.position) > detectRange || true == target.Dead)
        {
            target = null;
            fsm.Transition(ZombieState.Idle);
            return;
        }

        pathFinder.SetDestination(target.transform.position);
                        
        // ���� ���� ���� ����� ��
        if (Vector3.Distance(target.transform.position, transform.position) <= attackRange + 1.0f)
        {
            pathFinder.isStopped = true;

            Quaternion rot = Quaternion.LookRotation(target.transform.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, rotationSpeed * Time.deltaTime);

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
        damage *= 2.0f;

        pathFinder.speed = moveSpeed * 1.5f;
        zombieRenderer.material.color = Color.red;

        // TODO : �̴Ͼ� ���� �ڷ�ƾ ����
        if(true == PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(CoSpawnMinion());
        }
    }

    private IEnumerator CoSpawnMinion()
    {
        while(false == zombieHealth.Dead)
        {
            if(minionList.Count < maxMinionCount)
            {
                GameObject minionObject = PhotonNetwork.InstantiateRoomObject("ZombieMinion", transform.position + Random.insideUnitSphere * 7.0f, Quaternion.identity);
                ZombieMinion minion = minionObject.GetComponentInChildren<ZombieMinion>();
                minionList.Add(minion);
                minion.SetOwner(this);
            }
            yield return new WaitForSeconds(minionSpawnTime);
        }
    }

    protected override void OnDead()
    {
        base.OnDead();

        if(false == PhotonNetwork.IsMasterClient)
        {
            return;
        }

        Debug.Log("Stop Spawn");
        StopCoroutine(CoSpawnMinion());

        // ��� �̴Ͼ� ��� ó��
        Debug.Log($"current minion count : {minionList.Count}");
        for(int i = 0; i < minionList.Count; i++)
        { 
            ZombieHealth zombie = minionList[i].GetComponent<ZombieHealth>();
            zombie.OnDamage(zombie.Health, zombie.transform.position, Vector3.up, -100);
        }

        minionList.Clear();
    }

    public void RemoveMinion(ZombieMinion minion)
    {
        if(true == zombieHealth.Dead)
        {
            return;
        }

        if(true == minionList.Remove(minion))
        {
            Debug.Log($"RemoveMinion() success, current minion : {minionList.Count}");
        }
        else
        {
            Debug.Log($"RemoveMinion() failed, current minion : {minionList.Count}");
        }
    }

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        base.OnPhotonSerializeView(stream, info);
    }
}
