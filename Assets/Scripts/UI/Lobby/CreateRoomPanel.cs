using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomPanel : UIBase
{
    private enum Buttons
    {
        RoomCancleButton,
        RoomConfirmButton
    }

    private enum InputFields
    {
        RoomNameInputField
    }

    private enum Dropdowns
    {
        GameTypeDropdown
    }

    public Action onClickCancleButton;
    public Action<string, int> onClickConfirmButton;

    private void Awake()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TMP_InputField>(typeof(InputFields));
        Bind<TMP_Dropdown>(typeof(Dropdowns));

        Get<Button>((int)Buttons.RoomCancleButton).onClick.AddListener(OnClickCancleButton);
        Get<Button>((int)Buttons.RoomConfirmButton).onClick.AddListener(OnClickConfirmButton);
    }

    public void OnClickCancleButton()
    {
        TMP_InputField inputField = Get<TMP_InputField>((int)InputFields.RoomNameInputField);
        inputField.text = "";

        onClickCancleButton?.Invoke();
    }

    public void OnClickConfirmButton()
    {
        TMP_InputField inputField = Get<TMP_InputField>((int)InputFields.RoomNameInputField);
        TMP_Dropdown dropdown = Get<TMP_Dropdown>((int)Dropdowns.GameTypeDropdown);

        string roomName = inputField.text;
        int gameType = dropdown.value;
        
        inputField.text = "";
        dropdown.value = 0;
        onClickConfirmButton?.Invoke(roomName, gameType);
    }
}
