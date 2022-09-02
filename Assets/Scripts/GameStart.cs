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

        // TODO : 멀티 플레이 작업을 우선하기 위해
        SinglePlayButton.interactable = false;
    }

    public void OnClickSinglePlayButton()
    {
        // TODO : SinglePlay 모드로 작업
        // 같은 Scene을 쓰면서 Unreal처럼 모드 별로 다른 GameMode를 사용하는 방식
    }

    public void OnClickMultiPlayButton()
    {
        // Lobby Scene으로 이동
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
