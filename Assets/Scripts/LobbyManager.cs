using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    private string gameVersion = "0.1";

    public const int MaxRoomCount = 5;
    public const int MaxPlayerCount = 4;

    enum EUIMode
    {
        Lobby = 0,
        Nickname,
        CreateRoom,
        Room
    }

    EUIMode uiMode;
    
    public GameObject nicknamePanel;
    public GameObject lobbyPanel;
    public GameObject roomPanel;
    public GameObject createRoomPanel;

    public Text connectionInfoText;
    public TMP_InputField nicknameInputField;
    public Button nicknameConfirmButton;

    // Lobby
    public Button prevButton;
    public Button createRoomButton;

    // Room Panel
    public Button readyStartButton;
    public Text roomName;

    public List<Button> roomButtonList;
    public List<Button> playerButtonList;

    // Crete Room Panel
    public TMP_InputField roomNameInputField;
    public TMP_Dropdown gameTypeDropdown;
    public Button roomConfirmButton;
    public Button roomCancleButton;

    // Player List
    private List<Player> playerList;

    // Start is called before the first frame update
    void Start()
    {
        connectionInfoText = GameObject.Find("ConnectionInfoText").GetComponent<Text>();
        nicknamePanel = GameObject.Find("NicknamePanel");
        nicknameInputField = GameObject.Find("NicknameInputField").GetComponent<TMP_InputField>();
        nicknameConfirmButton = GameObject.Find("NicknameConfirmButton").GetComponent<Button>();

        prevButton = GameObject.Find("PrevButton").GetComponent<Button>();
        createRoomButton = GameObject.Find("CreateRoomButton").GetComponent<Button>();

        readyStartButton = GameObject.Find("ReadyStartButton").GetComponent<Button>();
        roomName = GameObject.Find("RoomName").GetComponent<Text>();

        lobbyPanel = GameObject.Find("LobbyPanel");
        roomPanel = GameObject.Find("RoomPanel");
        createRoomPanel = GameObject.Find("CreateRoomPanel");

        roomNameInputField = GameObject.Find("RoomNameInputField").GetComponent<TMP_InputField>();
        gameTypeDropdown = GameObject.Find("GameTypeDropdown").GetComponent<TMP_Dropdown>();
        roomConfirmButton = GameObject.Find("RoomConfirmButton").GetComponent<Button>();
        roomCancleButton = GameObject.Find("RoomCancleButton").GetComponent<Button>();

        if (null != connectionInfoText)
        {
            connectionInfoText.text = "";
        }

        nicknameConfirmButton.onClick.AddListener(OnClickNicknameConfirmButton);
        prevButton.onClick.AddListener(OnClickPrevButton);
        createRoomButton.onClick.AddListener(OnClickCreateRoomButton);

        readyStartButton.onClick.AddListener(OnClickReadyStartButton);

        roomConfirmButton.onClick.AddListener(OnClickRoomConfirmButton);
        roomCancleButton.onClick.AddListener(OnClickRoomCancleButton);

        // 게임 버전 설정
        PhotonNetwork.GameVersion = gameVersion;

        ChangeUIMode(EUIMode.Nickname);
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        connectionInfoText.text = "complete connection to master server";

        JoinLobby();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        connectionInfoText.text = "failed connect to master server";

        // 마스터 서버에 접속을 재시도
        //ConnectMasterServer();
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();

        connectionInfoText.text = "";
        PhotonNetwork.LocalPlayer.CustomProperties.Clear();
        PhotonNetwork.LocalPlayer.CustomProperties.TryAdd("ready", false);

        ChangeUIMode(EUIMode.Lobby);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);

        // 초기화
        foreach(var room in roomButtonList)
        {
            Text RoomNameText = room.transform.Find("RoomNameText").GetComponent<Text>();
            Text PlayerCount = room.transform.Find("PlayerCount").GetComponent<Text>();
            Text GameType = room.transform.Find("GameType").GetComponent<Text>();

            RoomNameText.text = "No Room";
            PlayerCount.text = $"0 / 4";
            GameType.text = "None";

            room.interactable = false;
            room.onClick.RemoveAllListeners();
        }

        int index = 0;
        foreach (var room in roomList)
        {
            if(room.PlayerCount <= 0)
            {
                continue;
            }

            Text RoomNameText = roomButtonList[index].transform.Find("RoomNameText").GetComponent<Text>();
            Text PlayerCount = roomButtonList[index].transform.Find("PlayerCount").GetComponent<Text>();
            Text GameType = roomButtonList[index].transform.Find("GameType").GetComponent<Text>();

            RoomNameText.text = room.Name;
            PlayerCount.text = $"{room.PlayerCount} / {room.MaxPlayers}";

            roomButtonList[index].interactable = true;
            roomButtonList[index].onClick.AddListener(() =>
            {
                PhotonNetwork.JoinRoom(RoomNameText.text);
            });

            index++;
            if (index >= 5)
            {
                break;
            }
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        Debug.Log($"Error : OnJoinRandomFailed(), message : {message}");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        Debug.Log($"Error : OnJoinRoomFailed(), message : {message}");
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        Debug.Log("방 참가 완료");
        ChangeUIMode(EUIMode.Room);

        object type;
        PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("GameType", out type);
        string gameType = "";
        if((int)type == 0)
        {
            GameInstance.Instance.GameType = GameInstance.EGameType.PvE;
            gameType = "PvE";
        }
        else if((int)type == 1)
        {
            GameInstance.Instance.GameType = GameInstance.EGameType.PvP;
            gameType = "PvP";
        }
        roomName.text = $"{PhotonNetwork.CurrentRoom.Name} / {gameType}";
       
        // UpdatePlayerList
        UpdateRoomPlayerList();
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        Debug.Log("방 퇴장 완료");
        ChangeUIMode(EUIMode.Lobby);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        if(null == newPlayer)
        {
            return;
        }

        ExitGames.Client.Photon.Hashtable customProperties = newPlayer.CustomProperties;
        if(true == customProperties.ContainsKey("ready"))
        {
            customProperties["ready"] = false;
        }
        else
        {
            customProperties.TryAdd("ready", false);
        }
        newPlayer.SetCustomProperties(customProperties);

        //playerList.Add(newPlayer);
        Debug.Log($"Player {newPlayer.NickName} enter");
        
        // UpdatePlayerList
        UpdateRoomPlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        //playerList.Remove(otherPlayer);
        Debug.Log($"Player {otherPlayer.NickName} left");
        
        // UpdatePlayerList
        UpdateRoomPlayerList();
    }

    public void OnClickNicknameConfirmButton()
    {
        if(null == nicknameInputField)
        {
            Debug.Log("Error : OnClickConfirmButton(), NicknameInputfield is null");
            return;
        }

        if(true == string.IsNullOrEmpty(nicknameInputField.text))
        {
            // TODO : Error Message Panel 추가 예정
            Debug.Log("Error : OnClickConfirmButton(), NicknameInputfield is empty");
            return;
        }

        PhotonNetwork.NickName = nicknameInputField.text;
        Debug.Log($"Nickname = {PhotonNetwork.NickName}");
        nicknamePanel.SetActive(false);
        
        ConnectMasterServer();
    }

    public void OnClickRoomConfirmButton()
    {
        if(null == roomNameInputField)
        {
            Debug.Log("Error : OnClickRoomConfirmButton(), RoomNameInputField is null");
            return;
        }

        if (true == string.IsNullOrEmpty(roomNameInputField.text))
        {
            // TODO : Error Message Panel 추가 예정
            Debug.Log("Error : OnClickRoomConfirmButton(), RoomNameInputField is empty");
            return;
        }

        if (PhotonNetwork.CountOfRooms >= MaxRoomCount)
        {
            Debug.Log("Error : OnClickRoomConfirmButton(), Room Full");
            return;
        }

        string RoomName = roomNameInputField.text;
        roomNameInputField.text = "";

        ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();
        customProperties.Add("GameType", gameTypeDropdown.value);

        PhotonNetwork.LocalPlayer.CustomProperties["ready"] = true;

        PhotonNetwork.CreateRoom(RoomName, new RoomOptions { MaxPlayers = MaxPlayerCount, CustomRoomProperties = customProperties });
    }

    public void OnClickRoomCancleButton()
    {
        roomNameInputField.text = "";
        ChangeUIMode(EUIMode.Lobby);
    }

    public void OnClickPrevButton()
    {
        switch(uiMode)
        {
            case EUIMode.Lobby:
                PhotonNetwork.Disconnect();
                SceneManager.LoadScene("GameStart");
                break;
            
            case EUIMode.Room:
                PhotonNetwork.LeaveRoom();
                ChangeUIMode(EUIMode.Lobby);
                break;
        }
    }

    public void OnClickCreateRoomButton()
    {
        ChangeUIMode(EUIMode.CreateRoom);
    }

    public void OnClickReadyStartButton()
    {
        // Start Button으로 동작
        if(true == PhotonNetwork.IsMasterClient)
        {
            if(false == IsStartGame())
            {
                Debug.Log("Log : 게임을 시작할 수 없습니다. 모든 플레이어가 준비된 상태여야 합니다.");
                return;
            }
            else
            {
                // TODO : Town Scene으로 이동, GameType에 따라 다른 GameMode를 사용하도록 설정 필요
                if((int)PhotonNetwork.CurrentRoom.CustomProperties["GameType"] == 0)
                {
                    GameInstance.Instance.GameType = GameInstance.EGameType.PvE;
                }
                else if((int)PhotonNetwork.CurrentRoom.CustomProperties["GameType"] == 1)
                {
                    GameInstance.Instance.GameType = GameInstance.EGameType.PvP;
                }
                
                photonView.RPC("StartGame", RpcTarget.All);
            }
        }

        // Ready Button으로 동작
        else
        {
            SetReadyState();
        }
    }

    private void ChangeUIMode(EUIMode mode)
    {
        switch(mode)
        {
            case EUIMode.Lobby:
                {
                    prevButton.gameObject.SetActive(true);
                    prevButton.interactable = true;

                    lobbyPanel.SetActive(true);
                    nicknamePanel.SetActive(false);
                    roomPanel.SetActive(false);
                    createRoomPanel.SetActive(false);
                }
            break;
            case EUIMode.Nickname:
                {
                    prevButton.gameObject.SetActive(false);
                    lobbyPanel.SetActive(false);
                    nicknamePanel.SetActive(true);
                    roomPanel.SetActive(false);
                    createRoomPanel.SetActive(false);
                }
                break;
            case EUIMode.CreateRoom:
                {
                    prevButton.gameObject.SetActive(false);
                    lobbyPanel.SetActive(false);
                    nicknamePanel.SetActive(false);
                    roomPanel.SetActive(false);
                    createRoomPanel.SetActive(true);
                }
                break;
            case EUIMode.Room:
                {
                    prevButton.gameObject.SetActive(true);
                    lobbyPanel.SetActive(false);
                    roomPanel.SetActive(true);
                    createRoomPanel.SetActive(false);

                    // 호스트인 경우 ReadyStartButton을 Start로
                    if (true == PhotonNetwork.IsMasterClient)
                    {
                        readyStartButton.GetComponentInChildren<Text>().text = "START";
                    }
                    else
                    {
                        readyStartButton.GetComponentInChildren<Text>().text = "READY";
                    }

                }
                break;
        }
        uiMode = mode;
    }

    private void ConnectMasterServer()
    {
        // 설정한 정보로 마스터 서버 접속 시도
        PhotonNetwork.ConnectUsingSettings();
        
        connectionInfoText.text = "connecting to master server";
    }

    private void JoinLobby()
    {
        // 마스터 서버에 접속 중이라면
        if (true == PhotonNetwork.IsConnected)
        {
            if (true == PhotonNetwork.JoinLobby())
            {
                connectionInfoText.text = "complete to join lobby";
            }
        }
    }

    private void UpdateRoomPlayerList()
    {
        // PlayerList 초기화
        for (int index = 0; index < MaxPlayerCount; index++)
        {
            Text playerName = playerButtonList[index].transform.Find("PlayerName").GetComponent<Text>();
            Text readyState = playerButtonList[index].transform.Find("ReadyState").GetComponent<Text>();

            playerName.text = "empty";
            readyState.text = "not\nready";
        }

        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            int index = player.ActorNumber - 1;
            if(index < 0 || index >= MaxPlayerCount)
            {
                continue;
            }

            Text playerName = playerButtonList[index].transform.Find("PlayerName").GetComponent<Text>();
            Text readyState = playerButtonList[index].transform.Find("ReadyState").GetComponent<Text>();
            
            playerName.text = player.NickName;
            object flag;

            player.CustomProperties.TryGetValue("ready", out flag);
            readyState.text = (bool)flag == true ? "ready" : "not\nready";

            Debug.Log($"player {player.NickName}, ActorNumber : {player.ActorNumber}");
        }
    }

    private bool IsStartGame()
    {
        foreach(var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if(true == player.IsMasterClient)
            {
                //continue;
            }

            if(false == player.CustomProperties.ContainsKey("ready"))
            {
                return false;
            }
            if(false == (bool)player.CustomProperties["ready"])
            {
                return false;
            }
        }
        return true;
    }

    private void SetReadyState()
    {
        object flag;
        if(false == PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("ready", out flag))
        {
            Debug.Log($"Error : Player CustomProperties not exist ready");
            return;
        }
        ExitGames.Client.Photon.Hashtable customProperties = PhotonNetwork.LocalPlayer.CustomProperties;

        customProperties["ready"] = !(bool)flag;
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);

        if (true == (bool)flag)
        {
            prevButton.interactable = false;
        }
        else
        {
            prevButton.interactable = true;
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

        if (uiMode == EUIMode.Room)
        {
            UpdateRoomPlayerList();
        }
    }

    [PunRPC]
    public void StartGame()
    {
        Debug.Log("Start Game");
        PhotonNetwork.LoadLevel("Town");
    }
}
