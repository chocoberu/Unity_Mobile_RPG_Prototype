using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : HealthComponent
{


    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void OnEnable()
    {
        base.OnEnable();

    }

    [PunRPC]
    public override void RestoreHealth(float healthAmount)
    {
        base.RestoreHealth(healthAmount);

    }

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

    }
}
