using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthComponent : MonoBehaviourPun, IDamageable
{
    [SerializeReference]
    private float defaultHealth = 100.0f;

    protected HPBarWidget hpBarWidget;

    public float Health { get; protected set; }
    public float DefaultHealth { get { return defaultHealth; } }
    public bool Dead { get; protected set; }
    public event Action OnDeath;

    protected virtual void OnEnable()
    {
        Dead = false;
        Health = defaultHealth;
        
        if(null == hpBarWidget)
        {
            hpBarWidget = GetComponentInChildren<HPBarWidget>();
        }
        hpBarWidget.gameObject.SetActive(true); 
        hpBarWidget.SetupHPBarWidget(defaultHealth, Health);
    }

    protected virtual void Awake()
    {
        
    }

    [PunRPC]
    public void ApplyUpdatedHealth(float newHealth, bool newDead)
    {
        Health = newHealth;
        Dead = newDead;

        if (null != hpBarWidget)
        {
            hpBarWidget.UpdateHP(Health);
        }
    }

    [PunRPC]
    public virtual void OnDamage(float damage, Vector3 hitPosition, Vector3 hitNormal, int AttackerTeamNumber)
    {
        if(true == PhotonNetwork.IsMasterClient)
        {
            Debug.Log($"OnDamage, Damage : {damage}");
            // ���� ���� ��� ������ ó�� X
            if(GetTeamNumber() == AttackerTeamNumber)
            {
                return;
            }

            Health -= damage;
            if (null != hpBarWidget)
            {
                hpBarWidget.UpdateHP(Health);
            }

            // Ŭ���̾�Ʈ�� ü�� ����ȭ
            photonView.RPC("ApplyUpdatedHealth", RpcTarget.Others, Health, Dead);
            // �ٸ� Ŭ���̾�Ʈ���� OnDamage ȣ��
            photonView.RPC("OnDamage", RpcTarget.Others, damage, hitPosition, hitNormal, AttackerTeamNumber);
        }

        Debug.Log($"OnDamage, Damage : {damage}");
        if (Health <= 0.0f && false == Dead)
        {
            Die();
        }
    }

    public virtual int GetTeamNumber()
    {
        return -1;
    }

    [PunRPC]
    public virtual void RestoreHealth(float healthAmount)
    {
        if(true == Dead)
        {
            return;
        }

        // ȣ��Ʈ������ ü�� ����
        if(true == PhotonNetwork.IsMasterClient)
        {
            Health = Mathf.Clamp(Health + healthAmount, 0.0f, defaultHealth);
            if(null != hpBarWidget)
            {
                hpBarWidget.UpdateHP(Health);
            }
            
            photonView.RPC("ApplyUpdatedHealth", RpcTarget.Others, Health, Dead);
            photonView.RPC("RestoreHealth", RpcTarget.Others, healthAmount);
        }
    }

    public virtual void Die()
    {
        if (null != OnDeath)
        {
            OnDeath.Invoke();
        }

        Dead = true;

        if (null != hpBarWidget)
        {
            hpBarWidget.gameObject.SetActive(false);
        }
    }
}
