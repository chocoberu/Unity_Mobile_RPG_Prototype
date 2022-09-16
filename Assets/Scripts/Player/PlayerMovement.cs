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
        RotationFix = false;
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
#if UNITY_ANDROID || UNITY_IOS
        Vector2 direction = GetInputMovement(movement);
        movement = Vector2.Dot(movement, direction) * direction;
#endif
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
            if(rollDirection == Vector3.zero)
            {
                rollDirection = new Vector3(transform.forward.x, 0.0f, transform.forward.z).normalized;
            }
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
        MoveState = PlayerMoveState.Roll;
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

    private Vector2 GetInputMovement(Vector2 input)
    {
        Vector2 ret = new Vector2(0.0f, 0.0f);
        if(input == Vector2.zero)
        {
            return ret;
        }

        float angle = Mathf.Rad2Deg * Mathf.Atan2(input.y, input.x);

        if(-22.5f <= angle && angle < 22.5f)
        {
            ret = new Vector2(1.0f, 0.0f);
        }
        else if(22.5f <= angle && angle < 67.5f)
        {
            ret = new Vector2(1.0f, 1.0f).normalized;
        }
        else if(67.5f <= angle && angle < 112.5f)
        {
            ret = new Vector2(0.0f, 1.0f);
        }
        else if(112.5f <= angle && angle < 157.5f)
        {
            ret = new Vector2(-1.0f, 1.0f).normalized;
        }
        else if((157.5f <= angle && angle <= 180.0f) || (-180.0f <= angle && angle < -157.5f))
        {
            ret = new Vector2(-1.0f, 0.0f);
        }
        else if(-157.5f <= angle && angle < -112.5f)
        {
            ret = new Vector2(-1.0f, -1.0f).normalized;
        }
        else if(-112.5f <= angle && angle < -67.5f)
        {
            ret = new Vector2(0.0f, -1.0f);
        }
        else if(-67.5f <= angle && angle < -22.5f)
        {
            ret = new Vector2(1.0f, -1.0f).normalized;
        }

        //Debug.Log($"angle : {angle}");

        return ret;
    }

    [PunRPC]
    public void Teleport(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
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
