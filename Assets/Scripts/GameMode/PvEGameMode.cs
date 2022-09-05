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

        // TEST CODE
        Debug.Log("Instantiate Character");
        GameObject player = PhotonNetwork.Instantiate("TestPlayer", new Vector3(0.0f, 2.0f, 0.0f), Quaternion.identity);
    }

    public override void StartMatch()
    {

    }

    public override void EndMatch()
    {

    }
}
