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

    // UI
    public GameObject damageWidgetObject;
    private GameObject damageList;
    private Stack<DamageTextWidget> damageWidgetStack = new Stack<DamageTextWidget>();

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
        damageList = new GameObject("DamageWidgetList");
        damageList.transform.parent = transform;

        AddDamageStack(20);
    }

    private void AddDamageStack(int count)
    {
        if(null == damageWidgetObject)
        {
            return;
        }

        for(int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(damageWidgetObject, damageList.transform);
            DamageTextWidget widget = go.GetComponent<DamageTextWidget>();
            widget.SetOwner(this);

            damageWidgetStack.Push(widget);
            go.SetActive(false);
        }
    }

    public void PushDamageWidget(DamageTextWidget widget)
    {
        if(null == widget)
        {
            return;
        }

        damageWidgetStack.Push(widget);
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
        if(true == PhotonNetwork.IsMasterClient)
        {
            Debug.Log($"OnDamage, Damage : {damage}");
            // 같은 팀의 경우 데미지 처리 X
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

            // 클라이언트에 체력 동기화
            photonView.RPC("ApplyUpdatedHealth", RpcTarget.Others, Health, Dead);
            // 다른 클라이언트에도 OnDamage 호출
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
        if(damageWidgetStack.Count == 0)
        {
            AddDamageStack(10);
        }

        DamageTextWidget damageWidget = damageWidgetStack.Pop();
        damageWidget.gameObject.SetActive(true);
        damageWidget.SetDamageText(transform.position, damage);
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

        // 호스트에서만 체력 갱신
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
}
