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
    private PlayerMoveState MoveState { get; set; }
    
    public float moveSpeed = 5.0f;
    public float rotateSpeed = 180.0f;

    private Vector3 moveDirection;
    private Vector3 rollDirection;

    public float Horizon { get; set; }
    public float Vertical { get; set; }

    // Components
    private Rigidbody playerRigidbody;
    private Animator animator;
    private PlayerAttack playerAttack;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        // Component 
        playerRigidbody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        playerAttack = GetComponent<PlayerAttack>();
        playerHealth = GetComponent<PlayerHealth>();

        MoveState = PlayerMoveState.Idle;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        if(false == photonView.IsMine)
        {

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
                break;
        }        
    }

    private void Rotate()
    {
        if (moveDirection == Vector3.zero)
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

    // InputSystem Callback
    private void OnMovement(InputValue value)
    {
        // ���ÿ����� �Է� ó��
        if(false == photonView.IsMine)
        {
            return;
        }

        if(PlayerMoveState.Roll == MoveState)
        {
            return;
        }

        Vector2 movement = value.Get<Vector2>();
        Horizon = movement.x;
        Vertical = movement.y;

        if(movement.sqrMagnitude > 0.0f)
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
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
        }
    }

}