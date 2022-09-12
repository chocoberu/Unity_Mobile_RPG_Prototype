using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PvEGameMode : GameMode
{
    [SerializeField]
    private float respawnTime = 3.0f;

    private GameObject playerObject;
    private ZombieBoss zombieBoss;

    // UI
    public GameObject gameClearUI;
    private Text gameClearTitle;
    private Text detail;
    
    // Start is called before the first frame update
    void Start()
    {
        gameClearTitle = gameClearUI.transform.Find("GameClearTitle").GetComponent<Text>();
        detail = gameClearUI.transform.Find("Detail").GetComponent<Text>();
        gameClearUI.SetActive(false);
        MatchState = EMatchState.PreMatch;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log($"{PhotonNetwork.ServerTimestamp}");
    }

    public override void UpdatePlayerList()
    {
        base.UpdatePlayerList();

        for(int i = 0; i < playerList.Count; i++)
        {
            playerList[i].OnDeath -= RestartPlayer;
            playerList[i].OnDeath += RestartPlayer;
        }

        if (false == PhotonNetwork.IsMasterClient)
        {
            return;
        }

        if (MatchState == EMatchState.PreMatch && playerList.Count == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            MatchState = EMatchState.InProgress;
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
    }

    public override void StartMatch()
    {
        Debug.Log("Start Match");

        if (false == PhotonNetwork.IsMasterClient)
        {
            zombieBoss = GameObject.FindGameObjectWithTag("Enemy").GetComponent<ZombieBoss>();
            if(null == zombieBoss)
            {
                Debug.Log("zombie boss is null");
                return;
            }
            zombieBoss.GetComponent<ZombieHealth>().OnDeath += OnBossDead;
            return;
        }
        
        // Boss Spawn
        if(true == PhotonNetwork.IsMasterClient)
        {
            GameObject boss = PhotonNetwork.InstantiateRoomObject("ZombieBoss", new Vector3(0.0f, 0.5f, 0.0f), Quaternion.identity);
            zombieBoss = boss.GetComponentInChildren<ZombieBoss>();
            zombieBoss.GetComponent<ZombieHealth>().OnDeath += OnBossDead;

            zombieBoss.photonView.RPC("StartFSM", RpcTarget.All);

            photonView.RPC("ChangeMatchState", RpcTarget.Others, (int)MatchState);
        }
    }

    public override void EndMatch()
    {
        Debug.Log("End Match");

        gameClearUI.SetActive(true);
        PlayerState playerState = playerObject.GetComponent<PlayerState>();

        detail.text = $"Kill : {playerState.KillScore} Death : {playerState.DeathScore}";
    }

    private void RestartPlayer(GameObject player)
    {
        StartCoroutine(CoRestartPlayer(player));
    }

    private IEnumerator CoRestartPlayer(GameObject player)
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

        StartCoroutine(CoEndMatch());
    }

    private IEnumerator CoEndMatch()
    {
        yield return new WaitForSeconds(3.0f);

        MatchState = EMatchState.PostMatch;
        photonView.RPC("ChangeMatchState", RpcTarget.Others, (int)MatchState);
    }

    public void ExitRoom()
    {
        Debug.Log("Exit Room");
        
        GameManager.Instance.ExitGame();
    }

}
