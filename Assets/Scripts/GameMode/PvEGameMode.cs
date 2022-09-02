using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PvEGameMode : GameMode
{
    // Start is called before the first frame update
    void Start()
    {
        MatchState = EMatchState.PreMatch;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void InitializeMatch()
    {
        if(false == PhotonNetwork.IsMasterClient)
        {
            GameManager.Instance.SetGameMode(this);
        }

        Debug.Log("Instantiate Character");
    }

    public override void StartMatch()
    {

    }

    public override void EndMatch()
    {

    }
}
