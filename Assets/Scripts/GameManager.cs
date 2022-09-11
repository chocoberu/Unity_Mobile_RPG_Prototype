using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            return instance;
        }
    }

    [SerializeField]
    private GameMode gameMode;

    private void Awake()
    {
        if(null == instance)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        switch(GameInstance.Instance.GameType)
        {
            case GameInstance.EGameType.Single:
                {
                    Debug.Log("SinglePlay GameMode");
                    PhotonNetwork.OfflineMode = true;
                }
                break;

            case GameInstance.EGameType.PvE:
                {
                    if(true == PhotonNetwork.IsMasterClient)
                    {
                        Debug.Log("PvE GameMode");
                        GameObject gameModeObject = PhotonNetwork.InstantiateRoomObject("PvEGameMode", Vector3.zero, Quaternion.identity);
                        gameMode = gameModeObject.GetComponent<GameMode>();
                    }
                }
                break;
            case GameInstance.EGameType.PvP:
                {
                    if (true == PhotonNetwork.IsMasterClient)
                    {
                        Debug.Log("PvP GameMode");
                    }
                }
                break;
        }
    }

    public void SetGameMode(GameMode newGameMode)
    {
        gameMode = newGameMode;
    }

    public void UpdatePlayerList()
    {
        if (null == gameMode)
        {
            return;
        }
        gameMode.UpdatePlayerList();
    }

    public void ExitGame()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel("Lobby");
    }
}
