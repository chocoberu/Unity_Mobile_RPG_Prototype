using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PvPHUD : UIBase
{
    private enum Texts
    {
        GameTimer,
        BlueScore,
        RedScore
    }

    private void Awake()
    {
        Bind<Text>(typeof(Texts));
    }

    public void UpdateTimer(float currentTime)
    {
        int time = (int)currentTime;
        Get<Text>((int)Texts.GameTimer).text = string.Format("{0:D2}:{1:D2}", time / 60, time % 60);
    }

    public void UpdateBlueScore(int score)
    {
        Get<Text>((int)Texts.BlueScore).text = string.Format("{0:D2}", score);
    }

    public void UpdateRedScore(int score)
    {
        Get<Text>((int)Texts.RedScore).text = string.Format("{0:D2}", score);
    }
}
