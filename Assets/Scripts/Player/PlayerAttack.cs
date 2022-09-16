using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviourPun, IPunObservable
{
    private PlayerHealth playerHealth;
    private Animator animator;
    private IWeaponable weapon;

    private GameObject weaponObject;

    // ���Ⱑ ���� ��� �ʿ��� transform
    private Transform gunPivot;
    private Vector3 weaponLocalPosition;
    private Transform leftHandMount; // ���� ���� ������, �޼��� ��ġ�� ����
    private Transform rightHandMount; // ���� ������ ������, �������� ��ġ�� ����

    public List<RuntimeAnimatorController> animatorControllerList = new List<RuntimeAnimatorController>();

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        animator = GetComponent<Animator>();
        
        gunPivot = transform.Find("ShooterSocket");
        weaponLocalPosition = gunPivot.transform.localPosition;
    }

    private void OnEnable()
    {
        if(null != weaponObject)
        {
            weaponObject.SetActive(true);
            weaponObject.transform.localPosition = weaponLocalPosition;
        }

        if(null != weapon)
        {
            //weapon.SetWeaponVisible(true);
        }
    }

    private void OnDisable()
    {
        if (null != weaponObject)
        {
            weaponObject.SetActive(false);
        }

        if(null != weapon)
        {
            //weapon.SetWeaponVisible(false);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // TODO : ������ Weapon�� Instantiate �ϵ��� ����
        if(true == photonView.IsMine)
        {
            weaponObject = PhotonNetwork.Instantiate("TestWeapon", transform.position, Quaternion.identity);

            SetupWeapon(weaponObject);
        }
        
    }

    private void OnAttack(InputValue value)
    {
        if(false == photonView.IsMine || false == enabled)
        {
            return;
        }

        if(true == value.isPressed)
        {
            //Debug.Log("Pressed");
            if(weapon != null)
            {
                weapon.StartAttack();
            }
        }
        else
        {
            //Debug.Log("Released");
            if (weapon != null)
            {
                weapon.StopAttack();
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

    public void SetupWeapon(GameObject newWeapon)
    {
        weaponObject = newWeapon;

        weaponObject.transform.parent = gameObject.transform;
        weaponObject.transform.localPosition = gunPivot.localPosition;
        weapon = weaponObject.GetComponent<IWeaponable>();

        leftHandMount = weaponObject.transform.Find("LeftHandle");
        rightHandMount = weaponObject.transform.Find("RightHandle");

        // TODO : Weapon Type�� �ٶ� �ٸ� Controller�� �����ϵ��� ����
        animator.runtimeAnimatorController = animatorControllerList[0];

        PhotonAnimatorView animationView = GetComponent<PhotonAnimatorView>();

        for(int index = 0; index < animator.layerCount; index++)
        {
            animationView.SetLayerSynchronized(index, PhotonAnimatorView.SynchronizeType.Continuous);
        }

        animationView.SetParameterSynchronized("Move", PhotonAnimatorView.ParameterType.Float, PhotonAnimatorView.SynchronizeType.Continuous);
    }

    public void SetWeaponVisible(bool value)
    {
        weapon.SetWeaponVisible(value);
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
