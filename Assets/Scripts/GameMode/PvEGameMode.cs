using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PvEGameMode : GameMode
{
    [SerializeField]
    private float respawnTime = 3.0f;
    // Start is called before the first frame update
    void Start()
    {
        MatchState = EMatchState.PreMatch;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void UpdatePlayerList()
    {
        base.UpdatePlayerList();

        for(int i = 0; i < playerList.Count; i++)
        {
            playerList[i].OnDeath -= RestartPlayer;
            playerList[i].OnDeath += RestartPlayer;
        }
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
        player.GetComponent<PlayerState>().StartPosition = playerStartPosition;
    }

    public override void StartMatch()
    {
        if(false == PhotonNetwork.IsMasterClient)
        {
            return;
        }
        
    }

    public override void EndMatch()
    {

    }

    private void RestartPlayer(GameObject player)
    {
        StartCoroutine(CoRestartPlayer(player));
    }

    IEnumerator CoRestartPlayer(GameObject player)
    {
        yield return new WaitForSeconds(respawnTime);

        Debug.Log("Restart Player");
        player.transform.position = player.GetComponent<PlayerState>().StartPosition;
        player.SetActive(false);
        player.SetActive(true);
    }
}
