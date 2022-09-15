using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamHPWidget : MonoBehaviour
{
    private TMP_Text nicknameText;
    private Slider hpBar;

    private void Awake()
    {
        nicknameText = GetComponentInChildren<TMP_Text>();
        hpBar = GetComponentInChildren<Slider>();
    }

    public void SetNickname(string newName)
    {
        nicknameText.text = newName;
    }

    public void UpdateHP(float newValue)
    {
        hpBar.value = newValue;
    }

    public void SetMaxHP(float maxHP)
    {
        hpBar.maxValue = maxHP;
    }
}
