using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviourPun, IPunObservable
{
    private PlayerHealth playerHealth;
    private Animator animator;
    IWeaponable weapon;

    // ���Ⱑ ���� ��� �ʿ��� transform
    private Transform gunPivot;
    private Transform leftHandMount; // ���� ���� ������, �޼��� ��ġ�� ����
    private Transform rightHandMount; // ���� ������ ������, �������� ��ġ�� ����

    public List<RuntimeAnimatorController> animatorControllerList = new List<RuntimeAnimatorController>();

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        animator = GetComponent<Animator>();
        
        gunPivot = transform.Find("ShooterSocket");
    }

    // Start is called before the first frame update
    void Start()
    {
        // TODO : ������ Weapon�� Instantiate �ϵ��� ����
        if(true == photonView.IsMine)
        {
            GameObject Weapon = PhotonNetwork.Instantiate("TestWeapon", transform.position, Quaternion.identity);

            SetupWeapon(Weapon);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnAttack(InputValue value)
    {
        if(false == photonView.IsMine)
        {
            return;
        }

        if(true == value.isPressed)
        {
            Debug.Log("Pressed");
            if(weapon != null)
            {
                weapon.StartAttack(this);
            }
        }
        else
        {
            Debug.Log("Released");
            if (weapon != null)
            {
                weapon.StopAttack(this);
            }
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        // ���� ������ gunPivot�� 3D ���� ������ �Ȳ�ġ ��ġ�� �̵�
        gunPivot.position = animator.GetIKHintPosition(AvatarIKHint.RightElbow);

        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);

        animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandMount.position);
        animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandMount.rotation);

        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1.0f);
        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1.0f);

        animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandMount.position);
        animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandMount.rotation);

    }

    public void SetupWeapon(GameObject weaponObject)
    {
        weaponObject.transform.SetParent(gameObject.transform);
        weaponObject.transform.localPosition = gunPivot.localPosition;
        weapon = weaponObject.GetComponent<IWeaponable>();

        leftHandMount = weaponObject.transform.Find("LeftHandle");
        rightHandMount = weaponObject.transform.Find("RightHandle");

        // TODO : Weapon Type�� �ٶ� �ٸ� Controller�� �����ϵ��� ����
        animator.runtimeAnimatorController = animatorControllerList[0];
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (true == stream.IsWriting)
        {

        }
        else
        {

        }
    }
}
