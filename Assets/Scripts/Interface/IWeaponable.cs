using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    Gun,
    Sword
}

public interface IWeaponable
{
    WeaponType weaponType { get; }

    void StartAttack();

    void StopAttack();

    void SetWeaponVisible(bool value);
}
