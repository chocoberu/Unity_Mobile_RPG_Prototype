using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
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

    private string DataPath;

    public string Nickname { get; set; }

    private int playerIndex = 1;
    public int PlayerIndex { get { return playerIndex; } set { playerIndex = value; } }
    
    private void Awake()
    {
        if(null == instance)
        {
            instance = this;
        }
        // Singleton은 1개의 인스턴스만 존재해야 함
        else
        {
            Destroy(gameObject);
            return;
        }

        Initialize();
    }

    private void Initialize()
    {
#if UNITY_ANDROID || UNITY_IOS
        Application.targetFrameRate = 60;
#endif
        DataPath = Application.dataPath + "/Data";
        // TODO : 게임 Static Info Load
        Nickname = null;
    }

    // Start is called before the first frame update
    private void Start()
    {
        GameType = EGameType.Single;
        DontDestroyOnLoad(gameObject);

        // TEST 

        GameModeData gameModeData = LoadJsonFile<GameModeData>(DataPath, "GameModeTest");
        Debug.Log($"gameMode, respawnTime : {gameModeData.respawnTime}, maxTime : {gameModeData.maxTime}");
    }

    private string ObjectToJson(object obj) { return JsonUtility.ToJson(obj); }
    private T JsonToOject<T>(string jsonData) { return JsonUtility.FromJson<T>(jsonData); }
    
    private void CreateJsonFile(string path, string fileName, string jsonData)
    {
        FileStream fileStream = new FileStream(string.Format("{0}/{1}.json", path, fileName), FileMode.Create);
        byte[] data = Encoding.UTF8.GetBytes(jsonData);
        fileStream.Write(data, 0, data.Length);
        fileStream.Close();
    }

    private T LoadJsonFile<T>(string path, string fileName) 
    { 
        FileStream fileStream = new FileStream(string.Format("{0}/{1}.json", path, fileName), FileMode.Open); 
        byte[] data = new byte[fileStream.Length]; 
        fileStream.Read(data, 0, data.Length);
        fileStream.Close(); string jsonData = Encoding.UTF8.GetString(data); 
        return JsonUtility.FromJson<T>(jsonData); 
    }
}
