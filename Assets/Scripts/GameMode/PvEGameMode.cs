using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PvEGameMode : GameMode
{
    [SerializeField]
    private float respawnTime = 3.0f;

    private GameObject playerObject;
    private ZombieBoss zombieBoss;
    
    // Start is called before the first frame update
    void Start()
    {
        MatchState = EMatchState.PreMatch;
    }

    // Update is called once per frame
    void Update()
    {
        if (MatchState == EMatchState.PreMatch && playerList.Count == PhotonNetwork.CountOfPlayers && null != zombieBoss)
        {
            MatchState = EMatchState.InProgress;
            photonView.RPC("ChangeMatchState", RpcTarget.Others, (int)MatchState);
        }
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

        playerObject = PhotonNetwork.Instantiate("TestPlayer", playerStartPosition, Quaternion.identity);
        playerObject.GetComponent<PlayerState>().StartPosition = playerStartPosition;
        
        if(true == PhotonNetwork.IsMasterClient)
        {
            GameObject boss = PhotonNetwork.Instantiate("ZombieBoss", new Vector3(0.0f, 0.5f, 0.0f), Quaternion.identity);
            zombieBoss = boss.GetComponentInChildren<ZombieBoss>();
            zombieBoss.GetComponent<ZombieHealth>().OnDeath += OnBossDead;
        }
    }

    public override void StartMatch()
    {
        Debug.Log("Start Match");

        if (false == PhotonNetwork.IsMasterClient)
        {
            return;
        }

        zombieBoss.photonView.RPC("StartFSM", RpcTarget.All);
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

        if(null != player)
        {
            Debug.Log("Restart Player");
            player.transform.position = player.GetComponent<PlayerState>().StartPosition;
            player.SetActive(false);
            player.SetActive(true);
        }
    }

    private void OnBossDead()
    {
        Debug.Log("Boss Dead");
        MatchState = EMatchState.PostMatch;
    }
}
