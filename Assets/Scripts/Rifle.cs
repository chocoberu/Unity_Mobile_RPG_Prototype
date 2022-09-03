using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rifle : MonoBehaviourPun, IWeaponable
{
    [SerializeField]
    private float attackDamage = 10.0f;

    public float AttackDamage { get { return attackDamage; } }

    // Start is called before the first frame update
    void Start()
    {
        if(false == photonView.IsMine)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach(var player in players)
            {
                if(photonView.CreatorActorNr != player.GetComponent<PhotonView>().CreatorActorNr)
                {
                    continue;
                }

                transform.SetParent(player.transform);
                break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartAttack(PlayerAttack player)
    {
        Debug.Log($"{player.gameObject.name} start attack");
        photonView.RPC("Attack", RpcTarget.MasterClient);
    }

    public void StopAttack(PlayerAttack player)
    {
        Debug.Log($"{player.gameObject.name} stop attack");
    }

    [PunRPC]
    private void Attack()
    {
        Debug.Log("Host Attack called");
    }
}
