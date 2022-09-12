using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// �÷��̾��� ������ ����
public enum PlayerMoveState
{
    Idle,
    Moving,
    Roll
};

public class PlayerMovement : MonoBehaviourPun, IPunObservable
{
    public PlayerMoveState MoveState { get; set; }
    
    public float moveSpeed = 5.0f;
    public float rotateSpeed = 180.0f;
    public float rollSpeed = 10.0f;

    private Vector3 moveDirection;
    private Vector3 rollDirection;

    public float Horizon { get; set; }
    public float Vertical { get; set; }

    public bool RotationFix { get; set; }

    // Components
    private Rigidbody playerRigidbody;
    private Animator animator;
    private PlayerAttack playerAttack;
    private PlayerHealth playerHealth;

    // ����ȭ ����
    private Vector3 serializedPosition;
    private Quaternion serializedRotation;

    private void Awake()
    {
        // Component 
        playerRigidbody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        playerAttack = GetComponent<PlayerAttack>();
        playerHealth = GetComponent<PlayerHealth>();

        MoveState = PlayerMoveState.Idle;
    }

    private void OnEnable()
    {
        MoveState = PlayerMoveState.Idle;
    }

    private void FixedUpdate()
    {
        if(false == photonView.IsMine)
        {
            transform.position = Vector3.Lerp(transform.position, serializedPosition, Time.deltaTime * moveSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, serializedRotation, Time.deltaTime * rotateSpeed);
            return;
        }

        switch(MoveState)
        {
            case PlayerMoveState.Idle:
                {
                    animator.SetFloat("Move", 0.0f);
                }
                break;
            case PlayerMoveState.Moving:
                {
                    Rotate();
                    Move();
                    animator.SetFloat("Move", 1.0f);
                }
                break;
            case PlayerMoveState.Roll:
                {
                    Roll();
                }
                break;
        }        
    }

    private void Rotate()
    {
        if (moveDirection == Vector3.zero || true == RotationFix)
        {
            return;
        }

        Quaternion rot = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, rotateSpeed * Time.deltaTime);
    }

    private void Move()
    {
        moveDirection.Set(Horizon, 0.0f, Vertical);
        moveDirection = moveDirection.normalized * moveSpeed * Time.deltaTime;

        transform.position += moveDirection;
    }

    private void Roll()
    {
        //transform.position += transform.forward * rollSpeed * Time.deltaTime;
        transform.position += rollDirection.normalized* rollSpeed * Time.deltaTime;
    }

    // InputSystem Callback
    private void OnMovement(InputValue value)
    {
        // ���ÿ����� �Է� ó��
        if(false == photonView.IsMine)
        {
            return;
        }

        Vector2 movement = value.Get<Vector2>();
        Horizon = movement.x;
        Vertical = movement.y;

        if (PlayerMoveState.Roll == MoveState)
        {
            return;
        }

        if (movement.sqrMagnitude > 0.0f)
        {
            MoveState = PlayerMoveState.Moving;
        }
        else
        {
            MoveState = PlayerMoveState.Idle;
        }
    }

    private void OnRoll(InputValue value)
    {
        // ���ÿ����� �Է� ó��
        if(false == photonView.IsMine)
        {
            return;
        }

        if(false == playerHealth.Dead && MoveState != PlayerMoveState.Roll)
        {
            MoveState = PlayerMoveState.Roll;
            rollDirection = new Vector3(Horizon, 0.0f, Vertical);
            photonView.RPC("OnRollProcessClient", RpcTarget.All);
        }
    }

    private void RollFinish()
    {
        photonView.RPC("RollFinishProcessClient", RpcTarget.All);
    }

    [PunRPC]
    private void OnRollProcessClient()
    {
        Quaternion rot = Quaternion.LookRotation(rollDirection);
        transform.rotation = rot;

        animator.SetTrigger("Roll");
        playerAttack.SetWeaponVisible(false);
    }

    [PunRPC]
    private void RollFinishProcessClient()
    {
        playerAttack.SetWeaponVisible(true);
        if (Horizon * Horizon + Vertical * Vertical >= 0.1f)
        {
            MoveState = PlayerMoveState.Moving;
        }
        else
        {
            MoveState = PlayerMoveState.Idle;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
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
