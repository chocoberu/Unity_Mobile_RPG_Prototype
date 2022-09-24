using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PvPGameMode : GameMode
{
    private GameObject playerObject;

    [SerializeField]
    private float respawnTime = 3.0f;

    private float startTime;

    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    private void Start()
    {
        //gameClearUI.SetActive(false);
        MatchState = EMatchState.PreMatch;
    }

    // Update is called once per frame
    private void Update()
    {

    }

    public override void UpdatePlayerList()
    {
        base.UpdatePlayerList();

        for (int i = 0; i < playerList.Count; i++)
        {
            playerList[i].OnDeath -= RestartPlayer;
            playerList[i].OnDeath += RestartPlayer;
        }

        UpdatePlayerHealthList();

        if (false == PhotonNetwork.IsMasterClient)
        {
            return;
        }

        if (MatchState == EMatchState.PreMatch && playerList.Count == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            StartCoroutine(CoStartCountDown());
        }
    }

    private void UpdatePlayerHealthList()
    {
        
    }

    private void RestartPlayer(GameObject player)
    {
        StartCoroutine(CoRestartPlayer(player));
    }

    private IEnumerator CoRestartPlayer(GameObject player)
    {
        yield return new WaitForSeconds(respawnTime);

        if (null != player)
        {
            Debug.Log("Restart Player");
            player.transform.position = player.GetComponent<PlayerState>().StartPosition;
            player.SetActive(false);
            player.SetActive(true);
        }
    }

    public override void InitializeMatch()
    {
        base.InitializeMatch();

        if (false == PhotonNetwork.IsMasterClient)
        {
            GameManager.Instance.SetGameMode(this);
        }
        // DamageText Widget 생성
        GameManager.Instance.AddObjectInPool(DamageTextWidget.WidgetPath, 80);

        // 팀에 맞는 PlayerStart를 찾아서 Player Spawn
        int playerIndex = GameInstance.Instance.PlayerIndex;

        GameObject[] playerStartList;
        string playerStartName;
        if (playerIndex % 2 == 1)
        {
            playerStartList = GameObject.FindGameObjectsWithTag("BluePlayerStart");
            playerStartName = $"BluePlayer{GameInstance.Instance.PlayerIndex / 2 + 1}";
        }
        else
        {
            playerStartList = GameObject.FindGameObjectsWithTag("RedPlayerStart");
            playerStartName = $"RedPlayer{GameInstance.Instance.PlayerIndex / 2 + 1}";
        }

        Debug.Log($"PlayerStart : {playerStartName}");

        Vector3 playerStartPosition = playerStartList[0].transform.position;
        Quaternion playerStartRotation = playerStartList[0].transform.rotation;
        foreach (var playerStart in playerStartList)
        {
            if (true == playerStartName.Equals(playerStart.name))
            {
                playerStartPosition = playerStart.transform.position;
                playerStartRotation = playerStart.transform.rotation;
                break;
            }
        }

        playerObject = PhotonNetwork.Instantiate("TestPlayer", playerStartPosition, playerStartRotation);
        PlayerState playerState = playerObject.GetComponent<PlayerState>();
        playerState.StartPosition = playerStartPosition;
        playerState.photonView.RPC("SetTeamNumber", RpcTarget.AllViaServer, playerIndex % 2 + 1);
        playerObject.GetComponent<PlayerMovement>().enabled = false;
    }

    public override void StartMatch()
    {
        base.StartMatch();

        playerObject.GetComponent<PlayerMovement>().enabled = true;
        playerObject.GetComponent<PlayerHealth>().enabled = false;
        playerObject.GetComponent<PlayerHealth>().enabled = true;
    }

    public override void EndMatch()
    {
        base.EndMatch();
    }

    private IEnumerator CoStartCountDown()
    {
        int countdown = 3;
        while(countdown >= 0)
        {
            yield return new WaitForSeconds(1.0f);
            photonView.RPC("UpdateStartCountDown", RpcTarget.AllViaServer, countdown);
            countdown--;
        }

        MatchState = EMatchState.InProgress;
        photonView.RPC("ChangeMatchState", RpcTarget.Others, (int)MatchState);
    }
    
    [PunRPC]
    public void UpdateStartCountDown(int countdown)
    {
        Debug.Log($"count down : {countdown}");
    }

    public void ExitRoom()
    {
        Debug.Log("Exit Room");

        GameManager.Instance.ExitGame();
    }
}
