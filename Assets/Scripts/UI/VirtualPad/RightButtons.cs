using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RightButtons : UIBase
{
    private enum Images
    {
        Attack,
        Roll
    }

    private enum Texts
    {
        AttackCount,
        RollText
    }

    private void Awake()
    {
        Bind<Image>(typeof(Images));
        Bind<Text>(typeof(Texts));
    }

    public void OnUpdateAttackCount(int count)
    {
        Get<Text>((int)Texts.AttackCount).text = $"{count}";
    }
}
