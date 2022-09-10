using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieBase : MonoBehaviourPun, IPunObservable
{
    protected LayerMask targetLayer;

    protected NavMeshAgent pathFinder;
    protected AudioSource audioSource;
    protected Animator animator;
    protected Renderer zombieRenderer;
    protected Rigidbody zombieRigidbody;
    protected ZombieHealth zombieHealth;

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

        pathFinder.isStopped = true;

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
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
        }
    }
}
