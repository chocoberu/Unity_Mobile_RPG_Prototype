using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RoomPanel : UIBase
{
    private enum Buttons
    {
        ReadyStartButton
    }

    private enum Texts
    {
        RoomName
    }

    public Action onClickReadyStartButton;
    private List<Button> playerButtonList;
    
    [SerializeField]
    private int MaxPlayerCount = 4;

    private void Awake()
    {
        Bind<Button>(typeof(Buttons));
        Bind<Text>(typeof(Texts));

        Get<Button>((int)Buttons.ReadyStartButton).onClick.AddListener(OnClickReadyStartButton);

        GameObject players = Utils.FindChild(gameObject, "PlayerList", false);
        if(null == players)
        {
            Debug.LogError("PlayerList is null");
            return;
        }

        playerButtonList = players.GetComponentsInChildren<Button>().ToList();
    }

    public void UpdatePlayerList(List<Player> playerList)
    {
        // PlayerList √ ±‚»≠
        for (int index = 0; index < MaxPlayerCount; index++)
        {
            Text playerName = playerButtonList[index].transform.Find("PlayerName").GetComponent<Text>();
            Text readyState = playerButtonList[index].transform.Find("ReadyState").GetComponent<Text>();

            playerName.text = "empty";
            readyState.text = "not\nready";
        }

        for(int i = 0; i < playerList.Count; i++)
        {
            Player player = playerList[i];
            int index = player.ActorNumber - 1;
            
            if (index < 0 || index >= MaxPlayerCount)
            {
                continue;
            }

            Text playerName = playerButtonList[index].transform.Find("PlayerName").GetComponent<Text>();
            Text readyState = playerButtonList[index].transform.Find("ReadyState").GetComponent<Text>();

            playerName.text = player.NickName;
            
            object flag;
            player.CustomProperties.TryGetValue("ready", out flag);
            readyState.text = (bool)flag == true ? "ready" : "not\nready";

            //Debug.Log($"player {player.NickName}, ActorNumber : {player.ActorNumber}");
        }
    }
    
    public void OnClickReadyStartButton()
    {
        onClickReadyStartButton?.Invoke();
    }

    public void SetRoomName(string roomName)
    {
        Get<Text>((int)Texts.RoomName).text = roomName;
    }

    public void SetReadyStartButton(bool host)
    {
        if(true == host)
        {
            Get<Button>((int)Buttons.ReadyStartButton).GetComponentInChildren<Text>().text = "START";
        }
        else
        {
            Get<Button>((int)Buttons.ReadyStartButton).GetComponentInChildren<Text>().text = "READY";
        }
    }
}
