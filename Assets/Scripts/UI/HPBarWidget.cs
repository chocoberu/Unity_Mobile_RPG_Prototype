using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HPBarWidget : MonoBehaviour
{
    private Slider hpBarSlider;
    private TMP_Text nickName;
    private Image fill;

    private void Awake()
    {
        hpBarSlider = GetComponentInChildren<Slider>();
        nickName = GetComponentInChildren<TMP_Text>();
        fill = Utils.FindChild<Image>(gameObject, "Fill", true);
    }

    public void SetupHPBarWidget(float DefaultHp, float CurrentHp)
    {
        if(null == hpBarSlider)
        {
            hpBarSlider = GetComponentInChildren<Slider>();
        }

        hpBarSlider.maxValue = DefaultHp;
        hpBarSlider.value = CurrentHp;        
    }

    public void SetupNickname(string newNickname)
    {
        if (null == nickName)
        {
            nickName = GetComponentInChildren<TMP_Text>();
        }

        nickName.text = newNickname;
    }

    public void SetHPBarColor(int teamNumber)
    {
        if(null == fill)
        {
            fill = Utils.FindChild<Image>(gameObject, "Fill", true);
        }

        switch (teamNumber)
        {
            case 0:
                fill.color = Color.blue;
                break;
            case 1:
                fill.color = Color.red;
                break;
        }
    }

    public void UpdateHP(float CurrentHp)
    {
        hpBarSlider.value = CurrentHp;
    }

    // Update is called once per frame
    void Update()
    {
        Transform parent = transform.parent;
        transform.position = parent.position + Vector3.up * (parent.GetComponent<Collider>().bounds.size.y + 0.2f);
        transform.rotation = Camera.main.transform.rotation; // UI가 카메라를 보도록 설정 (빌보드)
    }
}
