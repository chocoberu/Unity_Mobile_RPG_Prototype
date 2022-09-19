using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NicknamePanel : UIBase
{
    private enum Buttons
    {
        NicknameConfirmButton
    }
    
    private enum InputFields
    {
        NicknameInputField
    }

    public Action<string> onClickConfirmButton;

    private void Awake()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TMP_InputField>(typeof(InputFields));

        Get<Button>((int)Buttons.NicknameConfirmButton).onClick.AddListener(OnClickNicknameConfirmButton);
    }

    public void OnClickNicknameConfirmButton()
    {
        TMP_InputField inputField = Get<TMP_InputField>((int)InputFields.NicknameInputField);
        
        string nickname = inputField.text;
        inputField.text = "";
        
        onClickConfirmButton?.Invoke(nickname);
    }

}
