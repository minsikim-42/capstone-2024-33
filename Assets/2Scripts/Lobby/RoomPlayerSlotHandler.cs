using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomPlayerSlotHandler : MonoBehaviourPun
{
    private int slotIndex; // 슬롯의 인덱스
    
    [SerializeField] private TextMeshProUGUI playerNicknameText; // 플레이어 닉네임 텍스트
    [SerializeField] private Button emptyButton; // Empty 버튼
    [SerializeField] private TextMeshProUGUI emptyButtonText; // Empty 버튼 텍스트

    private void Awake()
    {
        slotIndex = transform.GetSiblingIndex(); // 슬롯의 인덱스를 저장
        
        transform.name = "Slot " + slotIndex; // 슬롯의 이름을 변경
        
        Init(); // 초기화
    }

    private void Init()
    {
        SetEmptySlot(); // 빈 슬롯으로 초기화
    }
    
    public void SetEmptySlot()
    {
        playerNicknameText.text = string.Empty; // 플레이어 닉네임 텍스트를 비움
        emptyButtonText.text = "Empty"; // Empty 버튼 텍스트를 Empty로 변경

        // 마스터 클라이언트인 경우 Empty 버튼을 활성화하고, 클릭 이벤트를 추가
        if (PhotonNetwork.IsMasterClient)
        {
            emptyButton.enabled = true;
            
            emptyButton.onClick.RemoveAllListeners();
            emptyButton.onClick.AddListener(SetAI);
        }
        else
        {
            emptyButton.enabled = false;
        }
    }
    
    public void SetPlayerNickname(string nickname)
    {
        playerNicknameText.text = nickname; // 플레이어 닉네임 텍스트를 변경
        SetPlayerSlot(); // 플레이어 슬롯으로 변경
    }
    
    private void SetPlayerSlot()
    {
        emptyButtonText.text = string.Empty; // Empty 버튼 텍스트를 비움
        emptyButton.enabled = false; // Empty 버튼 비활성화
    }

    public void SetSlotAI()
    {
        playerNicknameText.text = "AI"; // 플레이어 닉네임 텍스트를 AI로 변경
        emptyButtonText.text = string.Empty; // Empty 버튼 텍스트를 비움
    }
    
    private void SetAI()
    {
        // 플레이어 닉네임 텍스트가 AI인 경우 Empty 버튼 텍스트를 Empty로 변경하고, 플레이어 닉네임 텍스트를 비움
        if (playerNicknameText.text == "AI")
        {
            emptyButtonText.text = "Empty";
            playerNicknameText.text = string.Empty;
            NetworkManager.IT.SetSlotToEmpty(slotIndex); // 슬롯을 빈 슬롯으로 변경
        }
        else
        {
            emptyButtonText.text = string.Empty;
            playerNicknameText.text = "AI";
            NetworkManager.IT.SetSlotToAI(slotIndex); // 슬롯을 AI 슬롯으로 변경
        }
    }
}
