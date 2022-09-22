using Photon.Pun;
using Photon.Realtime;
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

    // ������ Object pool
    private Dictionary<string, Queue<GameObject>> objectPool = new Dictionary<string, Queue<GameObject>>();
    private ObjectPool poolObject;

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

        poolObject = new ObjectPool();
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

    public bool AddObjectInPool(string name, int count)
    {
        return poolObject.AddObjects(name, count);
    }

    public GameObject PopObjectInPool(string name)
    {
        return poolObject.PopObject(name);
    }

    public void PushObjectInPool(GameObject gameObject)
    {
        if(false == poolObject.PushObject(gameObject))
        {
            Debug.Log($"{gameObject.name}�� Object pool ���ο� ����. Destroy() ó��");
            Destroy(gameObject);
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
        switch(GameInstance.Instance.GameType)
        {
            case GameInstance.EGameType.Single:
                PhotonNetwork.LoadLevel("GameStart");
                break;
            case GameInstance.EGameType.PvE:
            case GameInstance.EGameType.PvP:
                PhotonNetwork.LoadLevel("Lobby");
                break;
        }
        
    }

    public override void OnConnectedToMaster()
    {
        // SinglePlay���� ������ ������ ���� Room ����
        if (GameInstance.EGameType.Single == GameInstance.Instance.GameType && null == gameMode)
        {
            PhotonNetwork.CreateRoom("SinglePlay");
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        GameObject gameModeObject = PhotonNetwork.InstantiateRoomObject("SinglePlayGameMode", Vector3.zero, Quaternion.identity);
        gameMode = gameModeObject.GetComponent<GameMode>();
    }
}
