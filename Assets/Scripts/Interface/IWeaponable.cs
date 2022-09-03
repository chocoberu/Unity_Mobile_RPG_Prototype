using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWeaponable
{
    void StartAttack(PlayerAttack player);

    void StopAttack(PlayerAttack player);
}
