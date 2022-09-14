using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageTextWidget : MonoBehaviour
{
    private float presentTime = 0.5f;
    private Text damageText;

    private Vector3 randomPosValue;
    
    private HealthComponent owner;

    private void Awake()
    {
        damageText = GetComponent<Text>();
        damageText.enabled = false;
        randomPosValue = Vector3.zero;

    }

    // Update is called once per frame
    void Update()
    {
        // UI가 카메라를 보도록 설정 (빌보드)
        transform.rotation = Camera.main.transform.rotation; 
    }

    public void SetOwner(HealthComponent healthComponent)
    {
        owner = healthComponent;
    }

    public void SetDamageText(Vector3 targetPosition, float damage, int fontSize = 1)
    {
        damageText.text = damage.ToString();

        randomPosValue = Random.insideUnitSphere * 0.5f;
        randomPosValue.z = -2.0f;
        transform.position = targetPosition + Vector3.up * 2.0f + randomPosValue;

        damageText.fontSize = fontSize;
        transform.rotation = Camera.main.transform.rotation;

        StartCoroutine(ShowDamageText());
    }

    private IEnumerator ShowDamageText()
    {
        damageText.enabled = true;
        yield return new WaitForSeconds(presentTime);
        damageText.enabled = false;

        owner.PushDamageWidget(this);
        gameObject.SetActive(false);
    }
}
