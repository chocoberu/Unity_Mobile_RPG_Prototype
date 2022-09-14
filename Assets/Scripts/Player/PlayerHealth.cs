using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : HealthComponent
{
    private Animator animator;
    private PlayerMovement playerMovement;
    private PlayerAttack playerAttack;
    private PlayerState playerState;

    private ParticleSystem hitEffect;

    protected override void Awake()
    {
        base.Awake();

        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        playerAttack = GetComponent<PlayerAttack>();
        playerState = GetComponent<PlayerState>();

        hitEffect = transform.Find("BloodSplatDirectional").GetComponent<ParticleSystem>();
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

        hpBarWidget.SetupNickname(photonView.Controller.NickName);
    }

    [PunRPC]
    public override void RestoreHealth(float healthAmount)
    {
        base.RestoreHealth(healthAmount);

    }

    [PunRPC]
    public override void OnDamage(float damage, Vector3 hitPosition, Vector3 hitNormal, int AttackerTeamNumber)
    {
        if (false == Dead)
        {
            // 피격 이펙트 플레이
            hitEffect.transform.rotation = Quaternion.LookRotation(hitNormal);
            hitEffect.Play();
        }

        base.OnDamage(damage, hitPosition, hitNormal, AttackerTeamNumber);
    }

    public override int GetTeamNumber()
    {
        if(null != playerState)
        {
            return playerState.TeamNumber;
        }
        return base.GetTeamNumber();
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
        
        playerState.DeathScore++;
    }
}
