using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roomNameText; // 방 이름 텍스트
    [SerializeField] private TextMeshProUGUI roomPlayerCount; // 방 인원 텍스트
    
    private Button button; // 방 클릭용 버튼
    private CanvasGroup cg; // 방 캔버스 그룹
    
    private int clickCount; // 클릭 횟수
    private DateTime clickTime; // 클릭 시간
    
    private void Awake()
    {
        // 초기화
        button = GetComponent<Button>();
        cg = GetComponent<CanvasGroup>();
        
        cg.alpha = 0;
        cg.blocksRaycasts = false;
        cg.interactable = false;
        
        clickCount = 0;
        clickTime = DateTime.Now;
        
        button.onClick.AddListener(OnClick); // 버튼 이벤트 추가
    }
    
    public void Inactive()
    {
        roomNameText.text = string.Empty; // 방 이름 초기화
        roomPlayerCount.text = string.Empty; // 방 인원 초기화
        
        // 방 숨기기
        cg.alpha = 0;
        cg.blocksRaycasts = false;
        cg.interactable = false;
    }

    public void Active(string roomName, int playerCount)
    {
        roomNameText.text = roomName; // 방 이름 설정
        roomPlayerCount.text = playerCount + " / 8"; // 방 인원 설정
        
        cg.alpha = 1; // 방 보이기

        // 방 인원이 8명이면 방 클릭 비활성화
        if (playerCount == 8)
        {
            cg.blocksRaycasts = false;
            cg.interactable = false;
        }
        else
        {
            cg.blocksRaycasts = true;
            cg.interactable = true;
        }
        
        transform.SetAsFirstSibling(); // 방을 맨 앞으로
    }
    
    private void OnClick()
    {
        var currentTime = DateTime.Now; // 현재 시간
        var timeDiff = currentTime - clickTime; // 시간 차이
        
        // 더블클릭 체크용 (시간차이가 0.5초 이하면 더블클릭)
        if (timeDiff.TotalMilliseconds < 500)
        {
            clickCount++; // 클릭 횟수 증가
        }
        else
        {
            clickCount = 1; // 클릭 횟수 초기화
        }
        
        clickTime = currentTime; // 클릭 시간 갱신
        
        // 더블클릭이면
        if (clickCount == 2)
        {
            clickCount = 0;
            
            DoubleClick(); // 더블클릭 이벤트 호출
        }
    }

    private void DoubleClick()
    {
        NetworkManager.instance.JoinRoom(roomNameText.text); // 방 입장
    }
}
