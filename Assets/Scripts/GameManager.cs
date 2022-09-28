using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    // 간단한 Object pool
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
            case GameInstance.EGameType.SinglePlay:
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
                        GameObject gameModeObject = PhotonNetwork.InstantiateRoomObject("PvPGameMode", Vector3.zero, Quaternion.identity);
                        gameMode = gameModeObject.GetComponent<GameMode>();
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
            Debug.Log($"{gameObject.name}이 Object pool 내부에 없음. Destroy() 처리");
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
            case GameInstance.EGameType.SinglePlay:
                break;
            case GameInstance.EGameType.PvE:
            case GameInstance.EGameType.PvP:
                PhotonNetwork.LoadLevel("Lobby");
                break;
        }
        
    }

    public override void OnConnectedToMaster()
    {
        if (GameInstance.EGameType.SinglePlay == GameInstance.Instance.GameType)
        {
            // SinglePlay에서 게임을 시작할 때만 Room 생성
            if (null == gameMode)
            {
                PhotonNetwork.CreateRoom("SinglePlay");
            }
            else
            {
                PhotonNetwork.Disconnect();
            }
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Debug.Log($"Disconnect : {cause}");
        
        if(GameInstance.EGameType.SinglePlay == GameInstance.Instance.GameType)
        {
            SceneManager.LoadScene("GameStart");
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        GameObject gameModeObject = PhotonNetwork.InstantiateRoomObject("SinglePlayGameMode", Vector3.zero, Quaternion.identity);
        gameMode = gameModeObject.GetComponent<GameMode>();
    }
}
