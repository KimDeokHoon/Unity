using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI ���� ���ӽ����̽�
using TMPro; // TMP ���� ���ӽ����̽�

public class MessageDisplay : MonoBehaviour
{
    public TMP_Text messageText; // �޽����� ǥ���� TMP_Text ������Ʈ

    public void ShowMessage(string message, float duration)
    {
        messageText.text = message; // �޽��� ����
        messageText.gameObject.SetActive(true); // UI Ȱ��ȭ
        StartCoroutine(HideMessageAfterDuration(duration)); // �޽����� ����� �ڷ�ƾ ȣ��
    }

    private IEnumerator HideMessageAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration); // ������ �ð� ���
        messageText.gameObject.SetActive(false); // UI ��Ȱ��ȭ
    }
}
