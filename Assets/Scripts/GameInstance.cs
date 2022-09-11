using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInstance : MonoBehaviour
{
    private static GameInstance instance;
    public static GameInstance Instance
    {
        get
        {
            return instance;
        }
    }

    public enum EGameType
    {
        Single = 0,
        PvE,
        PvP
    }

    [SerializeField]
    private EGameType gameType;
    public EGameType GameType
    {
        get
        {
            return gameType;
        }
        set
        {
            gameType = value;
        }
    }

    public string nickname;
    
    private void Awake()
    {
        if(null == instance)
        {
            instance = this;
        }
        // Singleton�� 1���� �ν��Ͻ��� �����ؾ� ��
        else
        {
            Destroy(gameObject);
            return;
        }

        Initialize();
    }

    private void Initialize()
    {
        Application.targetFrameRate = 60;

        // TODO : ���� Static Info Load
        nickname = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        GameType = EGameType.Single;
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
