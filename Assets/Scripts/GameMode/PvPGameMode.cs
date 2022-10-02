using Cinemachine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PvPGameMode : GameMode
{
    // GameState
    private int bluePlayers;
    private int redPlayers;
    private int blueScore;
    private int redScore;

    // UI
    private GameObject gameClearUI;
    private Text gameClearTitle;
    private GameObject backButton;
    [SerializeField]
    private List<TeamHPWidget> teamHPWidgetList;
    private StartCountDown startCountDown;
    private PvPHUD pvpHud;

    private void Awake()
    {
        gameClearUI = transform.Find("HUD Canvas/GameClearUI").gameObject;
        backButton = transform.Find("HUD Canvas/BackButton").gameObject;
        teamHPWidgetList = transform.GetComponentsInChildren<TeamHPWidget>().ToList();
        gameClearTitle = Utils.FindChild<Text>(gameClearUI, "GameClearTitle");

        startCountDown = Utils.FindChild<StartCountDown>(gameObject, null, true);
        startCountDown.gameObject.SetActive(false);
        pvpHud = Utils.FindChild<PvPHUD>(gameObject, null, true);

        blueFollowCamera = GameObject.Find("BlueFollowCamera").GetComponent<CinemachineVirtualCamera>();
        redFollowCamera = GameObject.Find("RedFollowCamera").GetComponent<CinemachineVirtualCamera>();

        blueScore = redScore = 0;
        bluePlayers = redPlayers = 0;
    }

    // Start is called before the first frame update
    private void Start()
    {
        MatchState = EMatchState.PreMatch;
        pvpHud.UpdateTimer(MaxTime);
    }

    // Update is called once per frame
    private void Update()
    {
        if(EMatchState.InProgress == MatchState)
        {
            float currentTime = MaxTime - ((float)PhotonNetwork.Time - startTime);
            if(currentTime <= 0.0f)
            {
                currentTime = 0.0f;
                MatchState = EMatchState.PostMatch;
                photonView.RPC("ChangeMatchState", RpcTarget.Others, (int)MatchState);
            }
            pvpHud.UpdateTimer(currentTime);
        }
    }

    public override void UpdatePlayerList()
    {
        base.UpdatePlayerList();

        for (int i = 0; i < playerList.Count; i++)
        {
            playerList[i].OnKill -= UpdateScore;
            playerList[i].OnKill += UpdateScore;
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
        // PlayerHealthList 업데이트
        for (int i = 0; i < teamHPWidgetList.Count; i++)
        {
            teamHPWidgetList[i].gameObject.SetActive(false);
        }

        // 
        /*int healthIndex = 0;
        for (int i = 0; i < playerList.Count; i++)
        {
            if (true == playerList[i].photonView.IsMine)
            {
                continue;
            }

            PlayerHealth player = playerList[i].GetComponent<PlayerHealth>();

            if (i % 2 != TeamNumber)
            {
                continue;
            }

            player.OnHPChanged -= teamHPWidgetList[healthIndex].UpdateHP;
            player.OnHPChanged += teamHPWidgetList[healthIndex].UpdateHP;

            teamHPWidgetList[healthIndex].gameObject.SetActive(true);
            teamHPWidgetList[healthIndex].SetMaxHP(player.DefaultHealth);
            teamHPWidgetList[healthIndex].UpdateHP(player.Health);
            teamHPWidgetList[healthIndex].SetNickname(player.photonView.Controller.NickName);
            healthIndex++;
        }*/
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
        int playerIndex = GameInstance.Instance.PlayerIndex - 1;
        TeamNumber = playerIndex % 2;

        GameObject[] playerStartList = null;
        string playerStartName = "";
        switch(TeamNumber)
        {
            case 0:
                playerStartList = GameObject.FindGameObjectsWithTag("BluePlayerStart");
                playerStartName = $"BluePlayer{GameInstance.Instance.PlayerIndex / 2 + 1}";
                redFollowCamera.enabled = false;
                break;
            case 1:
                playerStartList = GameObject.FindGameObjectsWithTag("RedPlayerStart");
                playerStartName = $"RedPlayer{GameInstance.Instance.PlayerIndex / 2 + 1}";
                blueFollowCamera.enabled = false;
                break;
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

        // 플레이어 스폰 및 팀 설정
        playerObject = PhotonNetwork.Instantiate("TestPlayer", playerStartPosition, playerStartRotation);
        PlayerState playerState = playerObject.GetComponent<PlayerState>();
        playerState.StartPosition = playerStartPosition;
        playerState.StartRotation = playerStartRotation;
        playerState.photonView.RPC("SetTeamNumber", RpcTarget.AllViaServer, TeamNumber);

        // Follow Camera 설정
        SetFollowCamera();

        playerObject.GetComponent<PlayerMovement>().enabled = false;
    }

    public override void StartMatch()
    {
        base.StartMatch();

        playerObject.GetComponent<PlayerMovement>().enabled = true;
        
        if(true == PhotonNetwork.IsMasterClient)
        {
            startTime = (float)PhotonNetwork.Time;
            photonView.RPC("SetStartTime", RpcTarget.Others, startTime);
        }
    }

    public override void EndMatch()
    {
        base.EndMatch();

        gameClearUI.SetActive(true);
        backButton.SetActive(false);

        if (blueScore > redScore)
        {
            gameClearTitle.text = "Blue Win";
        }
        else if(blueScore < redScore)
        {
            gameClearTitle.text = "Red Win";
        }
        else
        {
            gameClearTitle.text = "Draw";
        }
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
        if (false == startCountDown.gameObject.activeInHierarchy)
        {
            startCountDown.gameObject.SetActive(true);
        }

        startCountDown.SetCountDown(countdown);
    }

    private void UpdateScore(int teamNumber)
    {
        if(EMatchState.InProgress != MatchState)
        {
            return;
        }

        switch(teamNumber)
        {
            case 0:
                blueScore++;
                pvpHud.UpdateBlueScore(blueScore);
                break;
            case 1:
                redScore++;
                pvpHud.UpdateRedScore(redScore);
                break;
        }
        Debug.Log($"UpdateScore Blue : {blueScore}, Red : {redScore}");
    }

    private void AddeTeamCount(int teamNumber)
    {
        switch(teamNumber)
        {
            case 0:
                bluePlayers++;
                break;
            case 1:
                redPlayers++;
                break;
        }
    }

    private void SubtractTeamCount(int teamNumber)
    {
        switch (teamNumber)
        {
            case 0:
                bluePlayers--;
                break;
            case 1:
                redPlayers--;
                break;
        }
    }

    public void ExitRoom()
    {
        Debug.Log("Exit Room");

        GameManager.Instance.ExitGame();
    }
}
