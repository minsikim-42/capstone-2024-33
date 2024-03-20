using TMPro;
using UnityEngine;

public class ResultSlotHandler : MonoBehaviour
{
    [SerializeField] private CanvasGroup cg; // 캔버스 그룹
    [SerializeField] private TextMeshProUGUI nickNameText; // 닉네임 텍스트
    [SerializeField] private TextMeshProUGUI damageText; // 데미지 텍스트
    
    public int rank; // 슬롯의 순위
    private void Awake()
    {
        rank = int.Parse(transform.name); // 슬롯의 이름을 숫자로 변환 후 rank에 저장
        
        cg.alpha = 0; // 캔버스 그룹의 알파값을 0으로 변경
    }
    
    public void SetSlot(string nickName, string damage)
    {
        nickNameText.text = nickName; // 닉네임 텍스트 변경
        damageText.text = damage; // 데미지 텍스트 변경
        
        cg.alpha = 1; // 캔버스 그룹의 알파값을 1로 변경
    }
}