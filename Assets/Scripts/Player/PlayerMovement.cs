using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// 플레이어의 움직임 상태
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
    private bool isTeleport = true;

    // Components
    private Rigidbody playerRigidbody;
    private Animator animator;
    private PlayerAttack playerAttack;
    private PlayerHealth playerHealth;
    private PlayerState playerState;
    private PlayerInput playerInput;

    // 동기화 관련
    private Vector3 serializedPosition;
    private Quaternion serializedRotation;

    private void Awake()
    {
        // Component 
        playerRigidbody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        playerAttack = GetComponent<PlayerAttack>();
        playerHealth = GetComponent<PlayerHealth>();
        playerState = GetComponent<PlayerState>();
        playerInput = GetComponent<PlayerInput>();
        
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
            if(true == isTeleport && Vector3.Distance(transform.position, serializedPosition) >= 3.0f)
            {
                transform.position = serializedPosition;
                transform.rotation = serializedRotation;
                return;
            }

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
        transform.position += rollDirection.normalized* rollSpeed * Time.deltaTime;
    }

    // InputSystem Callback
    public void OnMovement(InputAction.CallbackContext context)
    {
        // 로컬에서만 입력 처리
        if(false == photonView.IsMine)
        {
            return;
        }

        Vector2 movement = context.ReadValue<Vector2>();

        // Red Team인 경우 처리
        if(1 == playerState.TeamNumber)
        {
            movement *= -1.0f;
        }
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

        if (InputActionPhase.Canceled != context.phase)
        {
            MoveState = PlayerMoveState.Moving;
        }
        else
        {
            MoveState = PlayerMoveState.Idle;
        }
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        // 로컬에서만 입력 처리
        if(false == photonView.IsMine || context.phase != InputActionPhase.Started)
        {
            return;
        }

        if(false == playerHealth.Dead && MoveState != PlayerMoveState.Roll)
        {
            MoveState = PlayerMoveState.Roll;
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
        
        rollDirection = new Vector3(Horizon, 0.0f, Vertical);
        if (rollDirection == Vector3.zero)
        {
            rollDirection = new Vector3(transform.forward.x, 0.0f, transform.forward.z).normalized;
        }

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
    public void RestartPlayer(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;

        gameObject.SetActive(false);
        gameObject.SetActive(true);
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

            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            serializedPosition += lag * playerRigidbody.velocity;
        }
    }

}
