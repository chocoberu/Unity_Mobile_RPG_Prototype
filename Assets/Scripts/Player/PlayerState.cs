using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviourPun, IPunObservable
{
    private int teamNumber = 0;
    public int TeamNumber { get { return teamNumber; } }
    
    public int KillScore { get; set; }
    public int DeathScore { get; set; }

    private void OnEnable()
    {
        GameManager.Instance.UpdatePlayerList();
    }

    private void OnDisable()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [PunRPC]
    public void SetTeamNumber(int newTeamNumber)
    {
        teamNumber = newTeamNumber;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(true == stream.IsWriting)
        {
            stream.SendNext(teamNumber);
            stream.SendNext(KillScore);
            stream.SendNext(DeathScore);
        }
        else
        {
            teamNumber = (int)stream.ReceiveNext();
            KillScore = (int)stream.ReceiveNext();
            DeathScore = (int)stream.ReceiveNext();
        }
    }

}
