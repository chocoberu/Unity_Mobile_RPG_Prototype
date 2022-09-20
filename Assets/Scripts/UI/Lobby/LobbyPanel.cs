using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPanel : UIBase
{
    private enum Buttons
    {
        CreateRoomButton,
        NicknameButton
    }

    public Action onClickCreateRoomButton;
    public Action onClickNicknameButton;
    public Action<string> onClickRoomButton;
    private List<Button> roomButtonList;

    private void Awake()
    {
        Bind<Button>(typeof(Buttons));

        Get<Button>((int)Buttons.CreateRoomButton).onClick.AddListener(OnClickCreaetButton);
        Get<Button>((int)Buttons.NicknameButton).onClick.AddListener(OnClickNicknameButton);

        GameObject roomList = Utils.FindChild(gameObject, "RoomList", true);
        if(null == roomList)
        {
            Debug.LogError("room list is null");
            return;
        }

        roomButtonList = roomList.GetComponentsInChildren<Button>().ToList();
    }

    public void UpdateRoomList(List<RoomInfo> roomList)
    {
        // UI 리스트 초기화
        for (int i = 0; i < roomButtonList.Count; i++)
        {
            Button room = roomButtonList[i];
            Text RoomNameText = room.transform.Find("RoomNameText").GetComponent<Text>();
            Text PlayerCount = room.transform.Find("PlayerCount").GetComponent<Text>();
            Text GameType = room.transform.Find("GameType").GetComponent<Text>();

            RoomNameText.text = "No Room";
            PlayerCount.text = $"0 / 4";
            GameType.text = "None";

            room.interactable = false;
            room.onClick.RemoveAllListeners();
        }

        // UI 리스트 설정
        int buttonIndex = 0;
        for (int i = 0; i < roomList.Count; i++) 
        {
            RoomInfo room = roomList[i];
            
            if (room.PlayerCount <= 0)
            {
                continue;
            }

            Text RoomNameText = roomButtonList[buttonIndex].transform.Find("RoomNameText").GetComponent<Text>();
            Text PlayerCount = roomButtonList[buttonIndex].transform.Find("PlayerCount").GetComponent<Text>();
            Text GameType = roomButtonList[buttonIndex].transform.Find("GameType").GetComponent<Text>();

            RoomNameText.text = room.Name;
            PlayerCount.text = $"{room.PlayerCount} / {room.MaxPlayers}";

            if (true == room.IsOpen)
            {
                roomButtonList[buttonIndex].interactable = true;
            }
            roomButtonList[buttonIndex].onClick.AddListener(() =>
            {
                onClickRoomButton?.Invoke(RoomNameText.text);
            });

            buttonIndex++;
            if (buttonIndex >= 5)
            {
                break;
            }
        }
    }

    public void OnClickCreaetButton()
    {
        onClickCreateRoomButton?.Invoke();
    }
    
    public void OnClickNicknameButton()
    {
        onClickNicknameButton?.Invoke();
    }
}
