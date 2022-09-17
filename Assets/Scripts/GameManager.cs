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

    // °£´ÜÇÑ Object pool
    private Dictionary<string, Queue<GameObject>> objectPool = new Dictionary<string, Queue<GameObject>>();
    private GameObject poolObject;

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

        poolObject = new GameObject("Object Pool");
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
        GameObject original = Resources.Load<GameObject>(name);
        if(null == original)
        {
            return false;
        }

        GameObject root = new GameObject(name);
        root.transform.SetParent(poolObject.transform);
        Queue<GameObject> queue = new Queue<GameObject>();

        for(int i = 0; i < count; i++)
        {
            GameObject gameObject = Instantiate<GameObject>(original, root.transform);
            if(null == gameObject)
            {
                return false;
            }
            gameObject.name = name;
            queue.Enqueue(gameObject);
            gameObject.SetActive(false);
        }
        objectPool.Add(name, queue);
        return true;
    }

    public GameObject PopObjectInPool(string name)
    {
        GameObject ret = null;
        Queue<GameObject> queue = null;
        if(false == objectPool.TryGetValue(name, out queue))
        {
            return null;
        }

        if(queue.Count == 0)
        {
            AddObjectInPool(name, 10);
        }
        ret = queue.Dequeue();
        ret.SetActive(true);
        return ret;
    }

    public bool PushObjectInPool(GameObject gameObject)
    {
        Queue<GameObject> queue = null;
        if(false == objectPool.TryGetValue(gameObject.name, out queue))
        {
            return false;
        }
        queue.Enqueue(gameObject);
        gameObject.SetActive(false);
        return true;
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
