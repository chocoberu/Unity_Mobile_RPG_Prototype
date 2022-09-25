using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PvPGameMode : GameMode
{
    // Player
    private GameObject playerObject;
    private int TeamNumber;

    // Game Time
    [SerializeField]
    private float respawnTime = 3.0f;
    private float MaxTime = 60.0f;
    private float startTime;

    // GameState
    private int blueScore;
    private int redScore;

    // UI
    private GameObject gameClearUI;
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

        startCountDown = Utils.FindChild<StartCountDown>(gameObject, null, true);
        startCountDown.gameObject.SetActive(false);
        pvpHud = Utils.FindChild<PvPHUD>(gameObject, null, true);
        
        blueScore = redScore = 0;
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
            playerList[i].OnDeath -= RestartPlayer;
            playerList[i].OnDeath += RestartPlayer;

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

        int healthIndex = 0;
        for (int i = 0; i < playerList.Count; i++)
        {
            if (true == playerList[i].photonView.IsMine)
            {
                continue;
            }

            PlayerHealth player = playerList[i].GetComponent<PlayerHealth>();
            if(player.GetTeamNumber() != TeamNumber)
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
        }
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
        int playerIndex = GameInstance.Instance.PlayerIndex - 1;
        TeamNumber = playerIndex % 2;

        GameObject[] playerStartList;
        string playerStartName;
        if (TeamNumber == 0)
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
        playerState.photonView.RPC("SetTeamNumber", RpcTarget.AllViaServer, TeamNumber);
        playerObject.GetComponent<PlayerHealth>().SetHPBarColor(TeamNumber);
        playerObject.GetComponent<PlayerMovement>().enabled = false;
    }

    public override void StartMatch()
    {
        base.StartMatch();

        playerObject.GetComponent<PlayerMovement>().enabled = true;
        
        startTime = (float)PhotonNetwork.Time;
    }

    public override void EndMatch()
    {
        base.EndMatch();

        gameClearUI.SetActive(true);
        backButton.SetActive(false);

        Text gameClearTitle = Utils.FindChild<Text>(gameClearUI, "GameClearTitle");
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

    public void ExitRoom()
    {
        Debug.Log("Exit Room");

        GameManager.Instance.ExitGame();
    }
}
