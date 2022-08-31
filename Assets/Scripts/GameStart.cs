using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClickSinglePlayButton()
    {

    }

    public void OnClickMultiPlayButton()
    {

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
