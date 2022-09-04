using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : HealthComponent
{
    private Animator animator;
    private PlayerMovement playerMovement;
    private PlayerAttack playerAttack;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        playerAttack = GetComponent<PlayerAttack>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if(null != playerMovement)
        {
            playerMovement.enabled = true;
        }
        if(null != playerAttack)
        {
            playerAttack.enabled = true;
        }
    }

    [PunRPC]
    public override void RestoreHealth(float healthAmount)
    {
        base.RestoreHealth(healthAmount);

    }

    [PunRPC]
    public override void OnDamage(float damage, Vector3 hitPosition, Vector3 hitNormal)
    {
        if(false == Dead)
        {

        }

        base.OnDamage(damage, hitPosition, hitNormal);

    }

    public override void Die()
    {
        base.Die();

        animator.SetTrigger("Die");
        if (null != playerMovement)
        {
            playerMovement.enabled = false;
        }
        if (null != playerAttack)
        {
            playerAttack.enabled = false;
        }
    }
}
