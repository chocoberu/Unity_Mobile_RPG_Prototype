using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviourPun, IPunObservable
{
    private int teamNumber = 0;
    public int TeamNumber { get { return teamNumber; } }

    public Vector3 StartPosition { get; set; }

    [SerializeField]
    private int killScore = 0;
    [SerializeField]
    private int deathScore = 0;
    
    public int KillScore 
    { 
        get { return killScore; }
        set 
        {
            if(killScore >= value)
            {
                return;
            }

            killScore = value;
            OnKill?.Invoke(teamNumber);
            photonView.RPC("SetKillScore", RpcTarget.Others, killScore);
        }
    }
    public int DeathScore
    {
        get { return deathScore; }
        set
        {
            if(deathScore >= value )
            {
                return;   
            }

            deathScore = value;
            OnDeath?.Invoke(gameObject);
        }
    }
    public event Action<GameObject> OnDeath;
    public event Action<int> OnKill;
    
    private void OnEnable()
    {
        GameManager.Instance.UpdatePlayerList();
    }
    
    private void OnDestroy()
    {
        GameManager.Instance.UpdatePlayerList();
    }

    [PunRPC]
    public void SetTeamNumber(int newTeamNumber)
    {
        teamNumber = newTeamNumber;
    }

    [PunRPC]
    private void SetKillScore(int newKillScore)
    {
        killScore = newKillScore;
        Debug.Log($"Kill score : {killScore}");
        OnKill?.Invoke(teamNumber);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
    }
}
