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

        // 알맞는 PlayerStart를 찾아서 Player Spawn
        GameObject[] playerStartList = GameObject.FindGameObjectsWithTag("BluePlayerStart");
        string playerStartName = $"BluePlayer{PhotonNetwork.LocalPlayer.ActorNumber}";

        Vector3 playerStartPosition = Vector3.up;
        foreach(var playerStart in playerStartList)
        {
            if(true == playerStartName.Equals(playerStart.name))
            {
                playerStartPosition = playerStart.transform.position;
                break;
            }
        }

        GameObject player = PhotonNetwork.Instantiate("TestPlayer", playerStartPosition, Quaternion.identity);


    }

    public override void StartMatch()
    {
        if(true == PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Start Match");
        }
        
    }

    public override void EndMatch()
    {

    }
}
