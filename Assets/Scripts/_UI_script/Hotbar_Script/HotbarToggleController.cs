using UnityEngine;
using System.Collections;

public class HotbarToggleController : MonoBehaviour
{
    public RectTransform hotbarPanel;
    public float slideDistance = 150f; // 이동 거리
    public float slideDuration = 0.25f; // 애니메이션 속도

    private bool isVisible = true;
    private Coroutine slideCoroutine;

    private float defaultY; //기본 UI 위치값

    void Start()
    {
        defaultY = hotbarPanel.anchoredPosition.y; // 초기 Y값 저장
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleHotbar();
        }
    }

    void ToggleHotbar()
    {
        //이전에 이미 실행중인 애니메이션 있을 경우 중단시켜주기
        if (slideCoroutine != null)
        StopCoroutine(slideCoroutine);

        isVisible = !isVisible;

        float targetY = isVisible ? defaultY : defaultY - slideDistance; // 기준 위치로 보정
        slideCoroutine = StartCoroutine(SlideToY(targetY));
    }

    IEnumerator SlideToY(float targetY)
    {
        Vector2 startPos = hotbarPanel.anchoredPosition; // hotbarPanel의 현재 위치(==default상태) 저장 변수
        Vector2 endPos = new Vector2(startPos.x, targetY); // 이동할(내려갈) 목표 위치
        float elapsed = 0f; // 애니메이션 경과 시간, 실행할 때마다 0으로 초기화해줌

        while (elapsed < slideDuration)
        {
            float t = elapsed / slideDuration; //진행율
            hotbarPanel.anchoredPosition = Vector2.Lerp(startPos, endPos, t); //진행율에따라 부드럽게 이동시켜주기
            elapsed += Time.unscaledDeltaTime; //게임 시간과 무관하게 작동
            yield return null;
        }

        hotbarPanel.anchoredPosition = endPos; //애니메이션 종료 후 정확하게 위치 보정
    }
}