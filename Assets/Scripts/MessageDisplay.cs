using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI 관련 네임스페이스
using TMPro; // TMP 관련 네임스페이스

public class MessageDisplay : MonoBehaviour
{
    public TMP_Text messageText; // 메시지를 표시할 TMP_Text 컴포넌트

    public void ShowMessage(string message, float duration)
    {
        messageText.text = message; // 메시지 설정
        messageText.gameObject.SetActive(true); // UI 활성화
        StartCoroutine(HideMessageAfterDuration(duration)); // 메시지를 숨기는 코루틴 호출
    }

    private IEnumerator HideMessageAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration); // 지정된 시간 대기
        messageText.gameObject.SetActive(false); // UI 비활성화
    }
}
