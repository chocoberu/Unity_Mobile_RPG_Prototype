using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthComponent : MonoBehaviourPun, IDamageable
{
    [SerializeReference]
    private float defaultHealth = 100.0f;

    public float Health { get; protected set; }
    public float DefaultHealth { get { return defaultHealth; } }
    public bool Dead { get; protected set; }
    public event Action OnDeath;

    protected virtual void OnEnable()
    {
        Dead = false;
        Health = defaultHealth;
    }

    [PunRPC]
    public void ApplyUpdatedHealth(float newHealth, bool newDead)
    {
        Health = newHealth;
        Dead = newDead;
    }

    [PunRPC]
    public virtual void OnDamage(float damage, Vector3 hitPosition, Vector3 hitNormal)
    {
        if(true == PhotonNetwork.IsMasterClient)
        {
            Health -= damage;

            // 클라이언트에 체력 동기화
            photonView.RPC("ApplyUpdatedHealth", RpcTarget.Others, Health, Dead);
            // 다른 클라이언트에도 OnDamage 호출
            photonView.RPC("OnDamage", RpcTarget.Others, damage, hitPosition, hitNormal);
        }
        if(Health <= 0.0f && false == Dead)
        {
            Die();
        }
    }

    [PunRPC]
    public virtual void RestoreHealth(float healthAmount)
    {
        if(true == Dead)
        {
            return;
        }

        // 호스트에서만 체력 갱신
        if(true == PhotonNetwork.IsMasterClient)
        {
            Health = Mathf.Clamp(Health + healthAmount, 0.0f, defaultHealth);

            photonView.RPC("ApplyUpdatedHealth", RpcTarget.Others, Health, Dead);
            photonView.RPC("RestoreHealth", RpcTarget.Others, healthAmount);
        }
    }

    public virtual void Die()
    {
        if(null != OnDeath)
        {
            OnDeath.Invoke();
        }

        Dead = true;
    }
}
