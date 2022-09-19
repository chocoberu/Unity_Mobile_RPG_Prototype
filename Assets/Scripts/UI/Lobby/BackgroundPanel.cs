using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundPanel : UIBase
{
    private enum Buttons
    {
        PrevButton
    }

    private enum Texts
    {
        ConnectionInfoText
    }

    public Action onClickPrevButton;

    private void Awake()
    {
        Bind<Button>(typeof(Buttons));
        Bind<Text>(typeof(Texts));

        Get<Button>((int)Buttons.PrevButton).onClick.AddListener(OnClickPrevButton);
    }

    public void OnClickPrevButton()
    {
        onClickPrevButton?.Invoke();
    }

    public void SetConectionInfoText(string str)
    {
        Get<Text>((int)Texts.ConnectionInfoText).text = str;
    }

    public void SetPrevButtonInteractable(bool value)
    {
        Get<Button>((int)Buttons.PrevButton).interactable = value;
    }

    public void SetPrevButtonVisible(bool value)
    {
        Get<Button>((int)Buttons.PrevButton).gameObject.SetActive(value);
    }
}
