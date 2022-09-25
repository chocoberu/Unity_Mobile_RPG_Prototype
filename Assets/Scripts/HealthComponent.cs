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
    public event Action<float> OnHPChanged;

    protected bool invincible = false;
    protected float invincibleTime = 2.0f;

    // UI
    public GameObject damageWidgetObject;

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

        OnHPChanged?.Invoke(newHealth);
    }

    [PunRPC]
    public virtual void OnDamage(float damage, Vector3 hitPosition, Vector3 hitNormal, int AttackerTeamNumber)
    {
        // TODO : ���� Ÿ��, ������ �߰� �ʿ�
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

            OnHPChanged?.Invoke(Health);

            // Ŭ���̾�Ʈ�� ü�� ����ȭ
            photonView.RPC("ApplyUpdatedHealth", RpcTarget.Others, Health, Dead);
            // �ٸ� Ŭ���̾�Ʈ���� OnDamage ȣ��
            photonView.RPC("OnDamage", RpcTarget.Others, damage, hitPosition, hitNormal, AttackerTeamNumber);
        }

        ShowDamageTextWidget(damage);
        if (Health <= 0.0f && false == Dead)
        {
            Die();
        }
    }

    private void ShowDamageTextWidget(float damage)
    {
        GameObject damageTextObject = GameManager.Instance.PopObjectInPool(DamageTextWidget.WidgetPath);
        DamageTextWidget damageWidget = damageTextObject?.GetComponent<DamageTextWidget>();
        if(null == damageWidget)
        {
            return;
        }

        damageWidget.SetDamageText(transform.position, damage);
    }

    public virtual int GetTeamNumber()
    {
        return -1;
    }

    public void SetHPBarColor(int teamNumber)
    {
        hpBarWidget.SetHPBarColor(teamNumber);
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

            OnHPChanged?.Invoke(Health);

            photonView.RPC("ApplyUpdatedHealth", RpcTarget.Others, Health, Dead);
            photonView.RPC("RestoreHealth", RpcTarget.Others, healthAmount);
        }
    }

    public virtual void Die()
    {
        Dead = true;

        if (null != OnDeath)
        {
            OnDeath.Invoke();
        }

        if (null != hpBarWidget)
        {
            hpBarWidget.gameObject.SetActive(false);
        }
    }

    protected IEnumerator CoActiveInvincible()
    {
        // TODO : Particle �߰�
        invincible = true;
        yield return new WaitForSeconds(invincibleTime);
        invincible = false;
    }
}
