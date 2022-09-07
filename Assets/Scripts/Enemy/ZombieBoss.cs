using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum ZombieBossState
{
    Idle = 0,
    Detect,
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

    private ZombieBossState state;
    public ZombieBossState State { get; }

    private FSM<ZombieBossState> fsm;

    private void Awake()
    {
        targetLayer = LayerMask.NameToLayer("Character");

        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        pathFinder = GetComponent<NavMeshAgent>();
        zombieRenderer = GetComponent<Renderer>();
        zombieRigidbody = GetComponent<Rigidbody>();

        fsm = new FSM<ZombieBossState>(this);
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
        Debug.Log("Idle Enter");
    }

    public void Idle_Update()
    {
        Debug.Log("Idle update!");
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
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
        }
    }

}
