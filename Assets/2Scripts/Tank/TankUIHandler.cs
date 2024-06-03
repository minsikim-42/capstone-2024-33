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
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private LineRenderer greenLineRenderer;

    private void Awake()
    {
        tankHandler = GetComponent<TankHandler>();
    }

    private void Start()
    {
        SetNickname(transform.name); // Nickname 설정
        
        hpSlider.maxValue = InGameManager.IT.maxHP; // 최대 HP 설정
        SetHP(InGameManager.IT.maxHP); // 초기 HP 설정

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

        lineRenderer = GetComponent<LineRenderer>();
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

    public void DrawLine(float value) {
        float angle = UIManager.IT.GetProjectileAngle();
        float lineRenderCount = InGameManager.IT.lineRenderCount;
        float dir = tankHandler.GetDirection();
        float power = value * tankHandler.projectileFireCoefficient;

        float timeStep = 0.02f;
        Vector2 pos = tankHandler.transform.position + UIManager.IT.GetProjectileAngleVector();
        float rad = angle * Mathf.Deg2Rad;
        // float mass = InGameManager.IT.projectileMass;
        float powerX = power * Mathf.Cos(rad) * dir;
        float powerY = power * Mathf.Sin(rad);
        float g = Mathf.Abs(Physics.gravity.y);

        // Debug.Log("p: " + power + ", agl: " + angle + ", pos: " + pos);

        if (angle > 90f && angle < 180f) angle -= 180;
        else if (angle > 180f) angle -= 360;

        int renderCount = (int)(lineRenderCount * Mathf.Abs(angle) * value / 100f);
        renderCount = InGameManager.IT.gameMode == 2 ? (int)(renderCount * 0.7) : renderCount;
        lineRenderer.positionCount = renderCount;
        float t=0f;
        for (int i=0; i<renderCount; i++) {
            float x = powerX * t;
            float y = powerY * t - g/2 * t*t;
            Vector2 point = new Vector2(x,y);

            lineRenderer.SetPosition(i, pos+point);
            t += timeStep;
        }

		// Predict Wind
		if (InGameManager.IT.gameMode == 2) return ;// Hard
        float wind = InGameManager.IT.windPower * InGameManager.IT.windPowerCoefficient;
        float mass = InGameManager.IT.projectileMass;
        greenLineRenderer.positionCount = renderCount;
        t=0f;
        for (int i=0; i<renderCount; i++) {
            float x = powerX * t + wind/2 / mass * t*t;
            float y = powerY * t - g/2 * t*t;
            Vector2 point = new Vector2(x,y);

            greenLineRenderer.SetPosition(i, pos+point);
            t += timeStep;
        }
    }

    public void CleanLine()
    {
        lineRenderer.positionCount = 0;
        greenLineRenderer.positionCount = 0;
    }
}
