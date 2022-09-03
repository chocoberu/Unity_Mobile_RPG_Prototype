using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviourPun, IPunObservable
{
    private Animator animator;
    IWeaponable weapon;

    // Start is called before the first frame update
    void Start()
    {
        // TEST CODE
        if(true == photonView.IsMine)
        {
            GameObject Weapon = PhotonNetwork.Instantiate("TestWeapon", transform.position, Quaternion.identity);
            Weapon.transform.SetParent(gameObject.transform);
            weapon = Weapon.GetComponent<IWeaponable>();
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
