using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    
    [Header("Sliders")]
    [SerializeField] private SliderHandler hpSlider; // HP 슬라이더
    [SerializeField] private PowerSlider powerSlider; // Power 슬라이더
    [SerializeField] private SliderHandler moveSlider; // Move 슬라이더
    [SerializeField] private WindSliderHandler windSliderHandler; // 바람 슬라이더

    [Header("Instruments")]
    [SerializeField] private InstrumentHandler tankHorizontalInstrument; // 탱크의 수평각
    [SerializeField] private InstrumentHandler projectileAngleInstrument; // 포탄의 발사각
    [SerializeField] private CanvasGroup instrumentCgForAI; // AI용
    [Header("Items")]
    [SerializeField] private Button doubleShotButton; // Double Shot 버튼
    [SerializeField] private Button attackRangeButton; // Attack Range 버튼
    [SerializeField] private Button attackDamageButton; // Attack Damage 버튼
    public bool selectableItem = false; // 아이템 선택 가능 여부
    
    [Header("Fade")]
    [SerializeField] private Image fadeImage; // Fade Image
    
    [Header("Exit")]
    [SerializeField] private Button exitButton; // Exit 버튼
    
    public bool spacePressed = false; // 스페이스바 눌림 여부
    private void Awake()
    {
        instance = this;
        
        doubleShotButton.onClick.AddListener(SetDoubleShot); // Double Shot 버튼 클릭 이벤트 추가
        attackRangeButton.onClick.AddListener(SetAttackRange); // Attack Range 버튼 클릭 이벤트 추가
        attackDamageButton.onClick.AddListener(SetAttackDamage); // Attack Damage 버튼 클릭 이벤트 추가
        
        exitButton.onClick.AddListener(ExitInGame); // Exit 버튼 클릭 이벤트 추가
    }

    private void Start()
    {
    }

    #region Sliders
    public void SetHP(float value)
    {
        hpSlider.SetValue(value); // HP 슬라이더의 값을 변경
    }
    
    public void SetPower(float value)
    {
        if (InGameManager.instance.IsAITurn())
            return;

        powerSlider.SetValue(value); // Power 슬라이더의 값을 변경
    }
    
    public void SetMove(float value)
    {
        if (InGameManager.instance.IsAITurn())
            return;

        moveSlider.SetValue(value); // Move 슬라이더의 값을 변경
    }

    public void SetPreviousPower()
    {
        if (InGameManager.instance.IsAITurn())
            return;

        powerSlider.SetPreviousPower(); // 이전 Power로 변경
    }
    
    public void SetWind(float value)
    {
        windSliderHandler.SetWind(value); // 바람 슬라이더의 값을 변경
    }
    #endregion
    
    #region Instruments
    public void SetTankHorizontal(float value, int direction)
    {
        if (InGameManager.instance.IsAITurn())
            instrumentCgForAI.alpha = 1; // AI용 Instrument의 Alpha를 1로 변경
        else
            instrumentCgForAI.alpha = 0; // AI용 Instrument의 Alpha를 0으로 변경
        
        tankHorizontalInstrument.SetHorizontal(value, direction); // 탱크의 수평각 변경
    }
    
    public void SetProjectileAngle(float value,  int direction)
    {
        projectileAngleInstrument.SetAngle(value, direction); // 포탄의 발사각 변경
    }
    
    public float GetProjectileAngle()
    {
        return projectileAngleInstrument.GetAngle(); // 포탄의 발사각 반환
    }
    
    public Vector3 GetProjectileAngleVector()
    {
        return  projectileAngleInstrument.GetAngleVector(); // 포탄의 발사각 벡터 반환
    }
    #endregion

    #region Items
    public void ItemButtonInit()
    {
        InGameManager.instance.isDoubleShot = false; // Double Shot 초기화
        InGameManager.instance.isAttackRange = false; // Attack Range 초기화
        InGameManager.instance.isAttackDamage = false; // Attack Damage 초기화
        
        UpdateItemButtonUI(); // Item Button UI 업데이트
    }
    
    private void SetDoubleShot()
    {
        if (!selectableItem) // 아이템 선택 가능하지 않으면 return
            return;

        if (spacePressed) // 스페이스바 누른 상태에서는 아이템 사용 불가
            return;
        
        InGameManager.instance.isDoubleShot = !InGameManager.instance.isDoubleShot; // Double Shot 여부 변경
        InGameManager.instance.isAttackRange = false; // Attack Range는 무조건 false
        InGameManager.instance.isAttackDamage = false; // Attack Damage는 무조건 false
        
        UpdateItemButtonUI(); // Item Button UI 업데이트
    }

    private void SetAttackRange()
    {
        if (!selectableItem) // 아이템 선택 가능하지 않으면 return
            return;

        if (spacePressed) // 스페이스바 누른 상태에서는 아이템 사용 불가
            return;

        InGameManager.instance.isAttackRange = !InGameManager.instance.isAttackRange; // Attack Range 여부 변경
        InGameManager.instance.isDoubleShot = false; // Double Shot은 무조건 false
        InGameManager.instance.isAttackDamage = false; // Attack Damage는 무조건 false
        
        UpdateItemButtonUI(); // Item Button UI 업데이트
    }
    
    private void SetAttackDamage()
    {
        if (!selectableItem) // 아이템 선택 가능하지 않으면 return
            return;

        if (spacePressed) // 스페이스바 누른 상태에서는 아이템 사용 불가
            return;

        InGameManager.instance.isAttackDamage = !InGameManager.instance.isAttackDamage; // Attack Damage 여부 변경
        InGameManager.instance.isDoubleShot = false; // Double Shot은 무조건 false
        InGameManager.instance.isAttackRange = false; // Attack Range는 무조건 false
        
        UpdateItemButtonUI(); // Item Button UI 업데이트
    }

    private void UpdateItemButtonUI()
    {
        doubleShotButton.image.color = InGameManager.instance.isDoubleShot ? Color.green : Color.white; // Double Shot 버튼 색상 변경
        attackRangeButton.image.color = InGameManager.instance.isAttackRange ? Color.green : Color.white; // Attack Range 버튼 색상 변경
        attackDamageButton.image.color = InGameManager.instance.isAttackDamage ? Color.green : Color.white; // Attack Damage 버튼 색상 변경
    } 
    #endregion
    
    #region Fade
    public void SetDarkScreen()
    {
        fadeImage.color = new Color(0, 0, 0, 1); // 검은 화면으로 변경
    }
    
    public void SetFade(bool isFadeIn, float time = 1f)
    {
        if (isFadeIn)
            fadeImage.CrossFadeAlpha(0, time, false); // Fade In
        else
            fadeImage.CrossFadeAlpha(1, time, false); // Fade Out
    }
    #endregion
    
    #region Exit
    private void ExitInGame()
    {
        if (spacePressed) // 스페이스바 누른 상태에서는 Exit 불가
            return;

        InGameManager.instance.ExitInGame(); // 게임 종료
    }
    #endregion
}
