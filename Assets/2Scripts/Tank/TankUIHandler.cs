using DG.Tweening;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TankUIHandler : MonoBehaviour
{
    private TankHandler tankHandler;
    [SerializeField] private Transform flipTransform;
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI hitDamageText;
    [SerializeField] private SpriteRenderer minimapIcon;

    private void Awake()
    {
        tankHandler = GetComponent<TankHandler>();
    }

    private void Start()
    {
        SetNickname(transform.name); // Nickname 설정
        
        hpSlider.maxValue = tankHandler.maxHP; // 최대 HP 설정
        SetHP(tankHandler.maxHP); // 초기 HP 설정

        minimapIcon.color = tankHandler.PV.IsMine ? Color.black : Color.red; // Minimap Icon 색상 설정
        if (TryGetComponent<AIHandler>(out var aiHandler)) {
            minimapIcon.color = Color.red; // Minimap Icon 색상 설정
            return ;
        }
        
        int teamNum = 0;
        Debug.Log("TankUI slot len: " + NetworkManager.IT.gameForSlots.Count);
        foreach (var slot in NetworkManager.IT.gameForSlots) {
            Debug.Log("transName: " + transform.name + ", slot.nickName : " + slot.nickName + ", slot.t : " + slot.teamNumber);
            if (transform.name == slot.nickName) {
                teamNum = slot.teamNumber;
                break;
            }
        }
        if (tankHandler.transform.name == PhotonNetwork.LocalPlayer.NickName) {
            if (teamNum == 1)
                teamNum = 3; // bright red
            else if (teamNum == 2)
                teamNum = 4; // bright blue
            else
                teamNum = -1; // yellow
        }
        if (teamNum == 0)
            minimapIcon.color = Color.red; // Minimap Icon 색상 설정
        else if (teamNum == 1)
            minimapIcon.color = Color.red; // Minimap Icon 색상 설정
		else if (teamNum == 2)
			minimapIcon.color = Color.blue;
        else if (teamNum == 3) // PV.MINE red
            minimapIcon.color = new Color(1f, 0.5f, 1f);
        else if (teamNum == 4) // PV.MINE blue
            minimapIcon.color = new Color(0.1f, 0.5f, 1f);
        else
            minimapIcon.color = Color.yellow;
        
        // AIHandler가 있으면 Minimap Icon 색상을 빨간색으로 설정
    }

    private void Update()
    {
        CheckFlip(); // Flip 체크
    }

    private void CheckFlip()
    {
        // tank 방향과 다르면 flip
        if (transform.localScale.x != flipTransform.localScale.x)
            flipTransform.transform.localScale = new Vector3(transform.localScale.x, 1, 1);
    }
    
    public void SetNickname(string nickname)
    {
        nicknameText.text = nickname; // Nickname 설정
    }

    public void UpdateUI(float currentHp, float damage)
    {
        SetHP(currentHp); // HP 설정
        
        SetHitDamage(damage); // Hit Damage 설정
        
        InGameManager.IT.TankHitEffect(transform.position); // Hit Effect 생성
    }
    
    public void SetHP(float value)
    {
        hpSlider.value = value; // HP Slider 설정
    }
    
    public void SetHitDamage(float value)
    {
        if (value < 1)
            return;
        
        hitDamageText.text = value.ToString(); // Hit Damage Text 설정
        
        hitDamageText.transform.DOLocalMoveY(95, 1f).From(44).SetEase(Ease.Linear).OnComplete(() => hitDamageText.text = string.Empty); // Hit Damage Text Animation
    }

    public void SetColor(int t) {
        Debug.Log("TankUI SetColor(" + tankHandler.transform.name + ") t: " + t);
        if (t == 0)
            minimapIcon.color = Color.red; // Minimap Icon 색상 설정
        else if (t == 1)
            minimapIcon.color = Color.red; // Minimap Icon 색상 설정
		else if (t == 2)
			minimapIcon.color = Color.blue;
        else if (t == 3) // PV.MINE red
            minimapIcon.color = new Color(1f, 0.5f, 1f);
        else if (t == 4) // PV.MINE blue
            minimapIcon.color = new Color(0.1f, 0.5f, 1f);
        else {
            Debug.Log("t else: " + t);
            minimapIcon.color = Color.yellow;
        }
    }
}
