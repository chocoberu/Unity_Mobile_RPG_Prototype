using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartCountDown : UIBase
{
    enum Texts
    {
        CountDown
    }

    private void Awake()
    {
        Bind<Text>(typeof(Texts));
    }

    public void SetCountDown(int countdown)
    {
        if(countdown > 0)
        {
            Get<Text>((int)Texts.CountDown).text = $"{countdown}";
        }
        else
        {
            Get<Text>((int)Texts.CountDown).text = "Game Start";
            StartCoroutine(HiddenCountDown());
        }
    }

    private IEnumerator HiddenCountDown()
    {
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);
    }
}
