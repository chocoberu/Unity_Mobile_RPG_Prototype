using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageTextWidget : MonoBehaviour
{
    private float presentTime = 0.5f;
    private Text damageText;

    [SerializeField]
    private int fontSize = 1;
    private Vector3 randomPosValue;
    private Transform parent;

    private void Awake()
    {
        damageText = GetComponent<Text>();
        damageText.enabled = false;
        randomPosValue = Vector3.zero;

        parent = transform.parent;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = parent.position + Vector3.up * 2.0f + randomPosValue;
        // UI�� ī�޶� ������ ���� (������)
        transform.rotation = Camera.main.transform.rotation; 
    }

    public void SetDamageText(float damage)
    {
        damageText.text = damage.ToString();

        randomPosValue = Random.insideUnitSphere * 0.5f;
        randomPosValue.z = -2.0f;
        Vector3 posisition = parent.position + Vector3.up * 2.0f + randomPosValue;
        transform.position = posisition;

        damageText.fontSize = fontSize;
        transform.rotation = Camera.main.transform.rotation;

    }

    private IEnumerator ShowDamageText()
    {
        damageText.enabled = true;
        yield return new WaitForSeconds(presentTime);
        damageText.enabled = false;

        // TODO : DamageTextWidget�� ��� �������� ���� �ʿ�
    }
}
