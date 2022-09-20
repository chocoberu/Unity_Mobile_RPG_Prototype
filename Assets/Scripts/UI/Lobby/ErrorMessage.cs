using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorMessage : UIBase
{
    private enum Texts
    {
        ErrorTitle,
        ErrorMessage
    }

    private enum Buttons
    {
        ConfirmButton
    }

    public Action onClickConfirmButton;

    private void Awake()
    {
        Bind<Text>(typeof(Texts));
        Bind<Button>(typeof(Buttons));

        Get<Button>((int)Buttons.ConfirmButton).onClick.AddListener(OnClickConfirmButton);
    }

    public void OnClickConfirmButton()
    {
        onClickConfirmButton?.Invoke();
    }

    public void SetErrorMessage(string str)
    {
        Get<Text>((int)Texts.ErrorMessage).text = str;
    }
}
