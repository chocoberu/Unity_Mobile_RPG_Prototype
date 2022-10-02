using Cinemachine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SinglePlayGameMode : GameMode
{
    // Enemy
    private ZombieBoss zombieBoss;

    // UI
    public GameObject gameClearUI;
    public GameObject backButton;
    public List<TeamHPWidget> teamHPWidgetList;
    private Text gameClearTitle;
    private Text detail;

    private void Awake()
    {
        gameClearUI = transform.Find("HUD Canvas/GameClearUI").gameObject;
        backButton = transform.Find("HUD Canvas/BackButton").gameObject;
        teamHPWidgetList = transform.GetComponentsInChildren<TeamHPWidget>().ToList();
        gameClearTitle = Utils.FindChild<Text>(gameClearUI, "GameClearTitle");
        detail = Utils.FindChild<Text>(gameClearUI, "Detail");

        gameClearUI.SetActive(false);

        CinemachineVirtualCamera redFollowCamera = GameObject.Find("RedFollowCamera").GetComponent<CinemachineVirtualCamera>();
        blueFollowCamera = GameObject.Find("BlueFollowCamera").GetComponent<CinemachineVirtualCamera>();
        redFollowCamera.enabled = false;

        TeamNumber = 0;
    }

    // Start is called before the first frame update
    private void Start()
    {
        for (int i = 0; i < teamHPWidgetList.Count; i++)
        {
            teamHPWidgetList[i].gameObject.SetActive(false);
        }
        MatchState = EMatchState.PreMatch;
    }

    public override void UpdatePlayerList()
    {
        base.UpdatePlayerList();

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
        if (false == PhotonNetwork.IsMasterClient)
        {
            GameManager.Instance.SetGameMode(this);
        }

        // DamageText Widget 생성
        GameManager.Instance.AddObjectInPool(DamageTextWidget.WidgetPath, 80);

        // 알맞는 PlayerStart를 찾아서 Player Spawn
        GameObject[] playerStartList = GameObject.FindGameObjectsWithTag("BluePlayerStart");
        string playerStartName = $"BluePlayer{PhotonNetwork.LocalPlayer.ActorNumber}";

        Vector3 playerStartPosition = Vector3.up;
        Quaternion playerStartRotation = Quaternion.identity;
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

        // Follow Camera 설정
        SetFollowCamera();

        playerObject.GetComponent<PlayerState>().StartPosition = playerStartPosition;
        playerObject.GetComponent<PlayerState>().StartRotation = playerStartRotation;
    }

    public override void StartMatch()
    {
        Debug.Log("Start Match");

        // Boss Spawn
        if (true == PhotonNetwork.IsMasterClient)
        {
            GameObject boss = PhotonNetwork.InstantiateRoomObject("ZombieBoss", new Vector3(0.0f, 0.5f, 0.0f), Quaternion.identity);
            zombieBoss = boss.GetComponentInChildren<ZombieBoss>();
            zombieBoss.GetComponent<ZombieHealth>().OnDeath += OnBossDead;

            zombieBoss.StartFSM();
            photonView.RPC("ChangeMatchState", RpcTarget.Others, (int)MatchState);
        }
        else
        {
            zombieBoss = GameObject.FindGameObjectWithTag("Enemy").GetComponent<ZombieBoss>();
            if (null == zombieBoss)
            {
                Debug.Log("zombie boss is null");
                return;
            }
            zombieBoss.GetComponent<ZombieHealth>().OnDeath += OnBossDead;
            return;
        }
    }

    public override void EndMatch()
    {
        Debug.Log("End Match");

        gameClearUI.SetActive(true);
        backButton.SetActive(false);

        PlayerState playerState = playerObject.GetComponent<PlayerState>();

        detail.text = $"Kill : {playerState.KillScore} Death : {playerState.DeathScore}";
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
