using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    private string gameVersion = "0.1";
    
    public GameObject nicknamePanel;
    public GameObject lobbyPanel;
    public GameObject RoomPanel;

    public Text connectionInfoText;
    public TMP_InputField nicknameInputField;
    public Button confirmButton;

    // Start is called before the first frame update
    void Start()
    {
        connectionInfoText = GameObject.Find("ConnectionInfoText").GetComponent<Text>();
        nicknamePanel = GameObject.Find("NicknamePanel");
        nicknameInputField = GameObject.Find("NicknameInputField").GetComponent<TMP_InputField>();
        confirmButton = GameObject.Find("ConfirmButton").GetComponent<Button>();
        
        if (null != connectionInfoText)
        {
            connectionInfoText.text = "";
        }

        confirmButton.onClick.AddListener(OnClickConfirmButton);

        // 게임 버전 설정
        PhotonNetwork.GameVersion = gameVersion;
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
        ConnectMasterServer();
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();

        connectionInfoText.text = "";
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
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
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
    }

    public void OnClickConfirmButton()
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
}
