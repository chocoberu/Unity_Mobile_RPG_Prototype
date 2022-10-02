using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStart : MonoBehaviour
{
    private Button SinglePlayButton;
    private Button MultiPlayButton;
    private Button ExitButton;

    // Start is called before the first frame update
    void Start()
    {
        SinglePlayButton = GameObject.Find("SinglePlay Button").GetComponent<Button>();
        MultiPlayButton = GameObject.Find("MultiPlay Button").GetComponent<Button>();
        ExitButton = GameObject.Find("Exit Button").GetComponent<Button>();
        
        if(null == ExitButton || null == SinglePlayButton || null == MultiPlayButton)
        {
            return;
        }

        SinglePlayButton.onClick.AddListener(OnClickSinglePlayButton);
        MultiPlayButton.onClick.AddListener(OnClickMultiPlayButton);
        ExitButton.onClick.AddListener(OnClickExitButton);

    }

    public void OnClickSinglePlayButton()
    {
        // TODO : 캐릭터 선택 or 무기 선택 UI 이후에 진입하도록 수정
        // GameInstance를 SinglePlay에 맞게 설정
        GameInstance.Instance.GameType = GameInstance.EGameType.SinglePlay;
        GameInstance.Instance.Nickname = "Player";
        GameInstance.Instance.PlayerIndex = 1;
        SceneManager.LoadScene("Town");
    }

    public void OnClickMultiPlayButton()
    {
        // Lobby Scene으로 이동
        GameInstance.Instance.Nickname = null;
        SceneManager.LoadScene("Lobby");
    }

    public void OnClickExitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
        Application.OpenURL("https://www.google.com");
#else
        Application.Quit();
#endif
    }
}
