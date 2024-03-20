using DG.Tweening;
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

        minimapIcon.color = tankHandler.PV.IsMine ? Color.yellow : Color.red; // Minimap Icon 색상 설정
        
        
        // AIHandler가 있으면 Minimap Icon 색상을 빨간색으로 설정
        var aiHandler = GetComponent<AIHandler>();
        
        if (aiHandler != null)
            minimapIcon.color = Color.red; // Minimap Icon 색상 설정
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
}
