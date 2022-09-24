using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    private string gameVersion = "0.1";

    public static readonly int MaxRoomCount = 5;
    public static readonly int MaxPlayerCount = 4;

    private enum EUIMode
    {
        Background = 0,
        Lobby,
        Nickname,
        CreateRoom,
        Room
    }

    private EUIMode uiMode;
    private FSM<EUIMode> fsm;

    // UI 
    private BackgroundPanel background;
    private LobbyPanel lobbyPanel;
    private RoomPanel roomPanel;
    private NicknamePanel nicknamePanel;
    private CreateRoomPanel createRoomPanel;
    private ErrorMessage errorMessage;

    private void Awake()
    {
        fsm = new FSM<EUIMode>(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        GameObject canvas = GameObject.Find("Canvas");
        background = Utils.FindChild<BackgroundPanel>(canvas, "Background", false);
        lobbyPanel = Utils.FindChild<LobbyPanel>(canvas, "LobbyPanel", false);
        roomPanel = Utils.FindChild<RoomPanel>(canvas, "RoomPanel", false);
        nicknamePanel = Utils.FindChild<NicknamePanel>(canvas, "NicknamePanel", false);
        createRoomPanel = Utils.FindChild<CreateRoomPanel>(canvas, "CreateRoomPanel", false);
        errorMessage = Utils.FindChild<ErrorMessage>(canvas, "ErrorMessage", false);

        background.SetConectionInfoText("");
        background.onClickPrevButton += OnClickPrevButton;
        
        lobbyPanel.onClickCreateRoomButton += OnClickCreateRoomButton;
        lobbyPanel.onClickNicknameButton += OnClickNicknameButton;
        lobbyPanel.onClickRoomButton += OnClickRoomButton;

        roomPanel.onClickReadyStartButton += OnClickReadyStartButton;
        createRoomPanel.onClickCancleButton += OnClickRoomCancleButton;
        createRoomPanel.onClickConfirmButton += OnClickRoomConfirmButton;

        nicknamePanel.onClickConfirmButton += OnClickNicknameConfirmButton;
        errorMessage.onClickConfirmButton += OnClickErrorMessageConfirmButton;

        // 게임 버전 설정
        PhotonNetwork.GameVersion = gameVersion;

        if(null == GameInstance.Instance.Nickname || false == PhotonNetwork.IsConnected)
        {
            fsm.StartFSM(EUIMode.Nickname);
        }
        else
        {
            fsm.StartFSM(EUIMode.Lobby);
        }
    }

    public override void OnConnectedToMaster()
    {
        // TODO : public enum으로 처리?
        background.SetConectionInfoText("complete connection to master server");
        JoinLobby();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        background.SetConectionInfoText("failed connect to master server");
    }

    public override void OnJoinedLobby()
    {
        background.SetConectionInfoText("");
        PhotonNetwork.LocalPlayer.CustomProperties.Clear();
        PhotonNetwork.LocalPlayer.CustomProperties.TryAdd("ready", false);

        ChangeUIMode(EUIMode.Lobby);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        lobbyPanel.UpdateRoomList(roomList);
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

        roomPanel.SetRoomName($"{PhotonNetwork.CurrentRoom.Name} / {gameType}");
        roomPanel.SetReadyStartButton(PhotonNetwork.IsMasterClient);
        
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

        Debug.Log($"Player {newPlayer.NickName}, actorNumber : {newPlayer.ActorNumber} enter");
        
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

        // 방장이 된 경우 관련된 부분 처리
        if(true == PhotonNetwork.IsMasterClient)
        {
            roomPanel.SetReadyStartButton(true);
            background.SetPrevButtonInteractable(true);
            ExitGames.Client.Photon.Hashtable customProperties = PhotonNetwork.LocalPlayer.CustomProperties;
            customProperties["ready"] = true;
            PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
        }
        
    }

    public void OnClickNicknameConfirmButton(string nickname)
    {
        if (true == string.IsNullOrEmpty(nickname))
        {
            Debug.Log("Error : OnClickConfirmButton(), NicknameInputfield is empty");
            errorMessage.gameObject.SetActive(true);
            errorMessage.SetErrorMessage("닉네임을 입력하세요");
            return;
        }

        PhotonNetwork.NickName = nickname;
        GameInstance.Instance.Nickname = PhotonNetwork.NickName;
        Debug.Log($"Nickname : {PhotonNetwork.NickName}");
        
        if (false == PhotonNetwork.IsConnected || true == PhotonNetwork.OfflineMode)
        {
            ConnectMasterServer();
        }
        else
        {
            ChangeUIMode(EUIMode.Lobby);
        }
    }

    public void OnClickRoomConfirmButton(string roomName, int gameType)
    {
        if (true == string.IsNullOrEmpty(roomName))
        {
            Debug.Log("Error : OnClickRoomConfirmButton(), RoomNameInputField is empty");
            errorMessage.gameObject.SetActive(true);
            errorMessage.SetErrorMessage("방 제목을 입력하세요");
            return;
        }

        // TODO : PvP 구현 이후 삭제 예정
        //if(1 == gameType)
        //{
        //    errorMessage.gameObject.SetActive(true);
        //    errorMessage.SetErrorMessage("PvP는 개발 예정입니다");
        //    return;
        //}

        if (PhotonNetwork.CountOfRooms >= MaxRoomCount)
        {
            Debug.Log("Error : OnClickRoomConfirmButton(), Room Full");
            errorMessage.gameObject.SetActive(true);
            errorMessage.SetErrorMessage("방을 더 만들 수 없습니다");
            return;
        }

        ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();
        customProperties.Add("GameType", gameType);
        var roomPropertiesLobby = new string[1];
        roomPropertiesLobby[0] = "GameType";

        PhotonNetwork.LocalPlayer.CustomProperties["ready"] = true;
        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = (byte)MaxPlayerCount, CustomRoomProperties = customProperties, CustomRoomPropertiesForLobby = roomPropertiesLobby });
        
        ChangeUIMode(EUIMode.Background);
        background.SetConectionInfoText("create room");
    }

    public void OnClickRoomCancleButton()
    {
        ChangeUIMode(EUIMode.Lobby);
    }

    public void OnClickPrevButton()
    {
        switch(uiMode)
        {
            case EUIMode.Lobby:
                PhotonNetwork.Disconnect();
                GameInstance.Instance.Nickname = null;
                SceneManager.LoadScene("GameStart");
                break;
            
            case EUIMode.Room:
                PhotonNetwork.LeaveRoom();
                break;
        }
    }

    public void OnClickCreateRoomButton()
    {
        ChangeUIMode(EUIMode.CreateRoom);
    }

    public void OnClickNicknameButton()
    {
        ChangeUIMode(EUIMode.Nickname);
    }

    public void OnClickRoomButton(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public void OnClickReadyStartButton()
    {
        // Start Button으로 동작
        if(true == PhotonNetwork.IsMasterClient)
        {
            if(false == IsStartGame())
            {
                Debug.Log("Log : 게임을 시작할 수 없습니다. 모든 플레이어가 준비된 상태여야 합니다.");
                errorMessage.gameObject.SetActive(true);
                errorMessage.SetErrorMessage("모든 플레이어가 준비된 상태가 아닙니다");
                return;
            }
            else
            {
                switch((int)PhotonNetwork.CurrentRoom.CustomProperties["GameType"])
                {
                    case 0:
                        GameInstance.Instance.GameType = GameInstance.EGameType.PvE;
                        break;
                    case 1:
                        GameInstance.Instance.GameType = GameInstance.EGameType.PvP;
                        break;
                }
                
                PhotonNetwork.CurrentRoom.IsOpen = false;
                photonView.RPC("StartGame", RpcTarget.All);
            }
        }

        // Ready Button으로 동작
        else
        {
            SetReadyState();
        }
    }

    public void OnClickErrorMessageConfirmButton()
    {
        errorMessage.gameObject.SetActive(false);
    }

    public void Background_Enter()
    {
        background.SetPrevButtonVisible(true);
    }

    public void Lobby_Enter()
    {
        lobbyPanel.gameObject.SetActive(true);
        background.SetPrevButtonVisible(true);
    }

    public void Lobby_Exit()
    {
        lobbyPanel.gameObject.SetActive(false);
    }

    public void Nickname_Enter()
    {
        nicknamePanel.gameObject.SetActive(true);
        background.SetPrevButtonVisible(false);
    }

    public void Nickname_Exit()
    {
        nicknamePanel.gameObject.SetActive(false);
        background.SetPrevButtonVisible(true);
    }

    public void CreateRoom_Enter()
    {
        createRoomPanel.gameObject.SetActive(true);
        background.SetPrevButtonVisible(false);
    }

    public void CreateRoom_Exit()
    {
        createRoomPanel.gameObject.SetActive(false);
        background.SetPrevButtonVisible(true);
    }

    public void Room_Enter()
    {
        roomPanel.gameObject.SetActive(true);
    }

    public void Room_Exit()
    {
        roomPanel.gameObject.SetActive(false);
        background.SetPrevButtonInteractable(true);
    }

    private void ChangeUIMode(EUIMode mode)
    {
        Debug.Log($"ChangeUIMode() called, current : {uiMode} next : {mode}");
        fsm.Transition(mode);
        uiMode = mode;
    }

    private void ConnectMasterServer()
    {
        // 설정한 정보로 마스터 서버 접속 시도
        PhotonNetwork.ConnectUsingSettings();

        ChangeUIMode(EUIMode.Background);
        background.SetConectionInfoText("connecting to master server");
    }

    private void JoinLobby()
    {
        // 마스터 서버에 접속 중이라면
        if (true == PhotonNetwork.IsConnected)
        {
            if (true == PhotonNetwork.JoinLobby())
            {
                background.SetConectionInfoText("complete to join lobby");
            }
        }
    }

    private void UpdateRoomPlayerList()
    {
        List<Player> playerList = PhotonNetwork.CurrentRoom.Players.Values.ToList();
        playerList.Sort((lhs, rhs) => lhs.ActorNumber < rhs.ActorNumber ? -1 : 1);

        // PlayerIndex를 설정, 플레이어 시작 위치를 지정할 때 사용
        for (int i = 0; i < playerList.Count; i++)
        {
            if(true == playerList[i].IsLocal)
            {
                GameInstance.Instance.PlayerIndex = i + 1;
                break;
            }
        }

        roomPanel.UpdatePlayerList(playerList);
    }

    private bool IsStartGame()
    {
        foreach(var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if(true == player.IsMasterClient)
            {
                continue;
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
        if (false == PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("ready", out flag))
        {
            Debug.Log($"Error : Player CustomProperties not exist ready");
            return;
        }

        ExitGames.Client.Photon.Hashtable customProperties = PhotonNetwork.LocalPlayer.CustomProperties;

        customProperties["ready"] = !(bool)flag;
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
        background.SetPrevButtonInteractable((bool)flag);
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
