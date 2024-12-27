using System.Collections;
using System.Threading.Tasks;
using Photon.Pun;
using UnityEngine;

public class  TankHandler : MonoBehaviour
{
    public PhotonView PV; // 포톤 뷰
    
    [Header("Animator")]
    public Animator anim; // 탱크 애니메이터
    
    [Header("Sprite")]
    public SpriteRenderer sprite; // 탱크 스프라이트
    
    
    [Header("Tank Move Value")]
    public float maxMoveValue; // 최대 이동값
    public float currentMoveValue; // 현재 이동값
    public float moveCoefficient; // 이동값 계수 (이동 게이지 UI 증가량 계수)
    public float tankMoveSpeed = 0.2f; // 탱크 이동 속도 (이동 게이지 UI와 탱크의 이동값을 연동하기 위한 계수) 
    
    [Header("Tank Power Value")]
    public float maxPowerValue; // 최대 파워값
    public float currentPowerValue; // 현재 파워값
    public float powerCoefficient; // 파워값 계수 (파워 게이지 UI 증가량 계수)
    public float projectileFireCoefficient = 0.0f; // 파워값 계수 (파워 게이지 UI와 탱크 미사일의 파워값을 연동하기 위한 계수)
    
    public float projectileDegrees = 0.0f; // 미사일 발사 각도
    public float delta = 0.0f; // 미사일 발사 각도 변화량

    public bool spacePressed = false; // 스페이스바 눌림 여부
    
    [SerializeField] private int direction = 1; // 탱크 방향 (1: 오른쪽, -1: 왼쪽)
    
    [SerializeField] public ProjectileHandler projectilePrefab; // 미사일 프리팹
    
    [Header("Raycast")]
    public float raycastDistance = 2f; // 레이캐스트 거리
    
    [SerializeField] private Vector3 rayOffset; // 레이 오프셋

    private RaycastHit2D hit1; // 레이캐스트 충돌 정보 1
    // private RaycastHit2D hit2; // 레이캐스트 충돌 정보 2
    private RaycastHit2D hit3; // 레이캐스트 충돌 정보 3

    public float angle; // 미사일 발사 각도
    
    [Header("Projectile")]
    private ProjectileHandler projectile; // 미사일
    private Vector3 projectileTransformPosition; // 미사일 위치
    private Vector3 projectileTransformLocalEulerAngles; // 미사일 각도
    private float projectileSpeed; // 미사일 속도
    private Vector3 projectileAngleVector; // 미사일 발사 각도 벡터
    
    [Header("Turn")]
    public bool isTurn = false; // 턴 여부
    
    [Header("HP")]
    public float currentHP; // 현재 HP
    
    public TankUIHandler tankUIHandler; // 탱크 UI 핸들러
    
    public bool isAi = false; // AI 여부
    public bool isDead = false; // 사망 여부
    public int teamNum = 0; // 팀 번호
    
    private void Awake()
    {
        PV = GetComponent<PhotonView>(); // 포톤 뷰 컴포넌트 할당
        tankUIHandler = GetComponent<TankUIHandler>(); // 탱크 UI 핸들러 컴포넌트 할당
        rayOffset = new Vector3(0.15f,0,0);
		powerCoefficient = InGameManager.IT.powerCoefficient;
    }

    public virtual void Start()
    {
        // Initialize
        currentMoveValue = maxMoveValue; // 현재 이동값을 최대 이동값으로 초기화
        UIManager.IT.SetProjectileAngle(projectileDegrees, direction, false);  // 파워 게이지 UI 각도 초기화

        // if (!transform.name.Contains("AI")) {
        if (!transform.name.Contains("AI")) {
            transform.name = PV.Owner.NickName; // 탱크 이름을 플레이어 닉네임으로 설정
            Debug.Log("tankHandler Start");
        }

        // if (PhotonNetwork.IsMasterClient) {
        //     PV.RPC(nameof(RPC_SetTeamNum), RpcTarget.All);
        // }
        
        currentHP = InGameManager.IT.maxHP; // 현재 HP를 최대 HP로 초기화
    }
    
    private void Update()
    {
        if (!PV.IsMine) // 포톤 뷰로 생성된 탱크가 내 탱크가 아니라면 리턴
            return;
        
        if (isDead) // 사망 상태라면 리턴
            return;

        if (Input.GetKey(KeyCode.Z)) {
            if (Input.GetKeyDown(KeyCode.Z)) {
                InGameManager.IT.CamStopZoom();
                InGameManager.IT.CamZoom(13);
            }

            if (Input.GetKey(KeyCode.LeftArrow)) // 카메라 왼쪽으로 이동
                InGameManager.IT.CamMove(-0.17f, 0);
            else if (Input.GetKey(KeyCode.RightArrow)) // Right
                InGameManager.IT.CamMove(0.17f, 0);
            else if (Input.GetKey(KeyCode.UpArrow)) // Up
                InGameManager.IT.CamMove(0, 0.17f);
            else if (Input.GetKey(KeyCode.DownArrow)) // Up
                InGameManager.IT.CamMove(0, -0.17f);

            return; // 줌인 중에는 탱크 못움직이게
        } else if (Input.GetKeyUp(KeyCode.Z)) {
            InGameManager.IT.CamStopZoom();
            InGameManager.IT.CamZoom(9);
            InGameManager.IT.CamMoveBack();
        }

        // rayOffset 간격으로 2개의 레이캐스트를 발사하여 지형의 기울기를 계산
        RaycastHit2D slopeHit = Physics2D.Raycast(transform.position, Vector3.down, raycastDistance, LayerMask.GetMask("Map"));
        if (slopeHit) {
            angle = Vector2.Angle(Vector2.up, slopeHit.normal);
            if (slopeHit.normal.x < 0) {
                angle *= -1;
            }
            if (direction == 1) {
                angle *= -1;
            }

            Debug.Log("ang: "+angle);
            
            if (isTurn && ((isAi && InGameManager.IT.IsAITurn()) || (!isAi && !InGameManager.IT.IsAITurn())))
                UIManager.IT.SetTankHorizontal(-angle, direction); // 탱크 수평 각도 UI 설정
        }
        
        // hit1 = Physics2D.Raycast(transform.position + rayOffset * direction, Vector2.down, raycastDistance, LayerMask.GetMask("Map"));
        // hit3 = Physics2D.Raycast(transform.position - rayOffset * direction, Vector2.down, raycastDistance, LayerMask.GetMask("Map"));

        // // 레이캐스트 충돌 정보가 있다면
        // if (hit1 && hit3) // hit2
        // {
        //     angle = -Vector2.Angle(direction * (hit1.point - hit3.point), Vector2.right); // 레이캐스트 충돌 정보 1과 3의 점 사이의 각도
        //     angle = Mathf.Clamp(angle, -55, 55); // angle min: -60, max: 60

        //     if (hit1.point.y >= hit3.point.y) // 레이캐스트 충돌 정보 2->3의 y값이 더 작다면
        //         angle = -angle; // angle값을 반전

        //     Debug.Log("dir: " + direction + ",HitP: " + hit1.point + ", Hp2: " + hit3.point + ", ang: " + angle);
        // } 

        // if (isTurn && ((isAi && InGameManager.IT.IsAITurn()) || (!isAi && !InGameManager.IT.IsAITurn())))
        //     UIManager.IT.SetTankHorizontal(-angle, direction); // 탱크 수평 각도 UI 설정
        
        SetSprite(); // 탱크 스프라이트 설정
        
        // 스페이스바를 누른 상태라면
        if (spacePressed)
        {
            if (!isAi && isTurn && Input.GetKey(KeyCode.Space)) // 스페이스바를 누르고 있다면
            {
                SpacePressed();
            }

            if (!isAi && isTurn && Input.GetKeyUp(KeyCode.Space)) // 스페이스바를 떼면
            {
                SpaceUp();
                
                return;
            }
        }
        // 스페이스바를 누르지 않은 상태라면
        else
        {
            if (currentMoveValue > 0) 
            {
                // Left, Right : Move
                if (!isAi && isTurn && Input.GetKey(KeyCode.LeftArrow)) // 왼쪽 화살표를 누르고 있다면
                {
                    LeftArrowPressed();
                }
                else if (!isAi && isTurn && Input.GetKey(KeyCode.RightArrow)) // 오른쪽 화살표를 누르고 있다면
                {
                    RightArrowPressed();
                }
                else
                {
                    anim.SetBool("Idle", true); // Idle 애니메이션 활성화
                    anim.SetBool("Move", false); // Move 애니메이션 비활성화
                }
            }
            else
            {
                anim.SetBool("Idle", true); // Idle 애니메이션 활성화
                anim.SetBool("Move", false); // Move 애니메이션 비활성화
            }
            
            // 스페이스바를 누르는 순간
            if (!isAi && isTurn && Input.GetKeyDown(KeyCode.Space))
            {
                SpaceDown();
                
                return;
            }
            
            // Up, Down : Projectile Angle
            if (!isAi && isTurn && Input.GetKey(KeyCode.UpArrow)) // 위쪽 화살표를 누르고 있다면 
            {
                UpArrowPressed();
            }
            else if (!isAi && isTurn && Input.GetKey(KeyCode.DownArrow)) // 아래쪽 화살표를 누르고 있다면
            {
                DownArrowPressed();
            }
        }
    }
    
    public void AngleInit()
    {
        if (isAi)
            projectileDegrees = 0; // 미사일 발사 각도 초기화
        UIManager.IT.SetProjectileAngle(projectileDegrees, direction, false); // 미사일 발사 각도 UI 설정
    }

    public void MoveValueInit()
    {
        currentMoveValue = maxMoveValue; // 이동값 초기화
        
        UIManager.IT.SetMove(currentMoveValue); // 이동 게이지 UI 설정
    }
    
    public void LeftArrowPressed()
    {
        transform.Translate(Vector2.left * tankMoveSpeed * Time.deltaTime); // 왼쪽으로 이동
        direction = -1; // 방향을 왼쪽으로 설정
        transform.localScale = new Vector3(direction, 1.0f, 1.0f); // 스프라이트 방향 설정

        currentMoveValue -= Time.deltaTime * moveCoefficient; // 이동값 감소
                
        anim.SetBool("Idle", false); // Idle 애니메이션 비활성화
        anim.SetBool("Move", true); // Move 애니메이션 활성화
                    
        UIManager.IT.SetMove(currentMoveValue); // 이동 게이지 UI 설정
        UIManager.IT.SetProjectileAngle(projectileDegrees, direction, true); // 미사일 발사 각도 UI 설정
    }
    
    public void RepeatLeftArrowPressed(float time)
    {
        StartCoroutine(CoroutineLeftArrowPressed(time));
    }
    
    IEnumerator CoroutineLeftArrowPressed(float time)
    {
        // time초 동안 계속 실행
        
        var timer = 0.0f;
        while (timer < time)
        {
            timer += Time.deltaTime;
            LeftArrowPressed();
            yield return null;
        }
    }
    
    public void RepeatRightArrowPressed(float time)
    {
        StartCoroutine(CoroutineRightArrowPressed(time));
    }
    
    IEnumerator CoroutineRightArrowPressed(float time)
    {
        // time초 동안 계속 실행
        
        var timer = 0.0f;
        while (timer < time)
        {
            timer += Time.deltaTime;
            RightArrowPressed();
            yield return null;
        }
    }

    public void RightArrowPressed()
    {
        transform.Translate(Vector2.right * tankMoveSpeed * Time.deltaTime); // 오른쪽으로 이동
        direction = 1; // 방향을 오른쪽으로 설정
        transform.localScale = new Vector3(direction, 1.0f, 1.0f); // 스프라이트 방향 설정
                
        currentMoveValue -= Time.deltaTime * moveCoefficient; // 이동값 감소
                
        anim.SetBool("Idle", false); // Idle 애니메이션 비활성화
        anim.SetBool("Move", true); // Move 애니메이션 활성화
                    
        UIManager.IT.SetMove(currentMoveValue); // 이동 게이지 UI 설정
        UIManager.IT.SetProjectileAngle(projectileDegrees, direction, true); // 미사일 발사 각도 UI 설정
    }
    
    public void SpaceDown()
    {
        currentPowerValue = 0; // 파워값 초기화
        UIManager.IT.SetPower(currentPowerValue); // 파워 게이지 UI 설정
        spacePressed = true; // 스페이스바 누름 상태로 설정
        UIManager.IT.spacePressed = true; // 스페이스바 누름 상태로 설정
    }

    public void SpacePressed()
    {
        // 현재 파워값이 최대 파워값보다 크거나 같다면
        if (currentPowerValue >= maxPowerValue)
        {
            currentPowerValue = maxPowerValue; // 현재 파워값을 최대 파워값으로 설정
                
            UIManager.IT.SetPower(currentPowerValue); // 파워 게이지 UI 설정
                    
            Fire(); // 미사일 발사
                    
            return;
        }
                
        currentPowerValue += Time.deltaTime * powerCoefficient; // 파워값 증가
                
        UIManager.IT.SetPower(currentPowerValue); // 파워 게이지 UI 설정
    }

    public void SpaceUp()
    {
        Fire(); // 미사일 발사
    }

    public void TurnOverFire()
    {
        if (spacePressed)
        {
            spacePressed = false; // 스페이스바 누름 상태 해제
            UIManager.IT.spacePressed = false; // 스페이스바 누름 상태 해제
            Fire(); // 미사일 발사
        }
        else
        {
            InGameManager.IT.NextTurnToMaster(); // 다음 턴으로 변경   
        }
    }
    
    public void UpArrowPressed()
    {
        // 미사일 발사 각도가 90도보다 크거나 같다면
        if (projectileDegrees >= 90f)
        {
            projectileDegrees = 90f; // 미사일 발사 각도를 90도로 설정
                
            UIManager.IT.SetProjectileAngle(projectileDegrees, direction, true); // 미사일 발사 각도 UI 설정
            return;
        }

        projectileDegrees += delta * Time.deltaTime; // 미사일 발사 각도 증가
                
        UIManager.IT.SetProjectileAngle(projectileDegrees, direction, true); // 미사일 발사 각도 UI 설정
    }

    public void DownArrowPressed()
    {
        // 미사일 발사 각도가 0보다 작거나 같다면
        if (projectileDegrees <= 0)
        {
            projectileDegrees = 0; // 미사일 발사 각도를 0으로 설정
                
            UIManager.IT.SetProjectileAngle(projectileDegrees, direction, true); // 미사일 발사 각도 UI 설정
            return;
        }
            
        projectileDegrees -= delta * Time.deltaTime; // 미사일 발사 각도 감소
                
        UIManager.IT.SetProjectileAngle(projectileDegrees, direction, true); // 미사일 발사 각도 UI 설정
    }
    
    
    private void Fire()
    {
        Debug.Log("dir: " + direction);
        UIManager.IT.SetMove(currentMoveValue); // 이동 게이지 UI 설정
        UIManager.IT.SetProjectileAngle(projectileDegrees, direction, true); // 미사일 발사 각도 UI 설정

        SoundManager.IT.PlaySFX();
        isTurn = false; // 턴 종료
        UIManager.IT.selectableItem = false; // 아이템 선택 불가능으로 변경
        
        projectileTransformPosition = transform.position + UIManager.IT.GetProjectileAngleVector(); // 미사일 발사 위치 설정

        // 탱크의 방향이 왼쪽이라면
        if (direction == -1)
            projectileTransformLocalEulerAngles = Vector3.forward * (180 - UIManager.IT.GetProjectileAngle()); // 미사일 발사 각도 설정
        // 탱크의 방향이 오른쪽이라면
        else
            projectileTransformLocalEulerAngles = Vector3.forward * UIManager.IT.GetProjectileAngle(); // 미사일 발사 각도 설정
        
        projectileSpeed = currentPowerValue * projectileFireCoefficient; // 미사일 발사 속도 설정
        projectileAngleVector = UIManager.IT.GetProjectileAngleVector(); // 미사일 발사 각도 벡터 설정
        
        // 사용 아이템 체크
        var itemType = string.Empty;
        if (InGameManager.IT.isDoubleShot)
        {
            itemType = "Double";
            InGameManager.IT.doubleShot--;
        }
        else if (InGameManager.IT.isAttackRange)
        {
            itemType = "Range";
            InGameManager.IT.attackRange--;
        }
        else if (InGameManager.IT.isAttackDamage)
        {
            itemType = "Damage";
            InGameManager.IT.attackDamage--;
        }
        
        // RPC_Fire 함수 호출
        PV.RPC(nameof(RPC_Fire), RpcTarget.All, projectileTransformPosition, projectileTransformLocalEulerAngles, projectileSpeed, projectileAngleVector, direction, itemType);
        
        spacePressed = false; // 스페이스바 누름 상태 해제
        UIManager.IT.spacePressed = false; // 스페이스바 누름 상태 해제
        
        UIManager.IT.SetPreviousPower(); // 이전 파워값 설정
    }

    [PunRPC]
    private async void RPC_Fire(Vector3 projectileTransformPosition, Vector3 projectileTransformLocalEulerAngles, float projectileSpeed, Vector3 projectileAngleVector, int direction, string itemType)
    {
        InGameManager.IT.StopTimer(); // 타이머 멈춤
        InGameManager.IT.initialPosition = projectileTransformPosition; // 미사일 초기 위치 설정 (거리 변수 계산용)
        
        // 아이템 체크
        InGameManager.IT.isAttackRange = false; // 초기화
        InGameManager.IT.isAttackDamage = false; // 초기화
        var isDouble = false; // 초기화
        
        switch (itemType)
        {
            case "Double": // 더블샷
                isDouble = true;
                break;
            case "Range": // 피격범위 증폭 1.5배
                InGameManager.IT.isAttackRange = true;
                break;  
            case "Damage": // 데미지 증폭 1.5배
                InGameManager.IT.isAttackDamage = true;
                break;
        }
        
        var isDoubleShot = false;

        // 미사일 생성
        GenerateProjectile();

        if (isDouble)
        {
            await Task.Delay(400);
            
            isDoubleShot = true;
            
            GenerateProjectile(); // 미사일 생성
        }
        
        void GenerateProjectile()
        {
            projectile = Instantiate(projectilePrefab); // 미사일 생성
            projectile.name = transform.name + "'s Projectile"; // 미사일 이름 설정
            projectile.transform.position = projectileTransformPosition; // 미사일 위치 설정
            projectile.transform.localEulerAngles = projectileTransformLocalEulerAngles; // 미사일 각도 설정
        
            projectile.Set(projectileSpeed, projectileAngleVector, direction, isDoubleShot); // 미사일 설정(속도, 각도, 방향, 더블샷 여부)
        
            // SetCamera
            if (!isDoubleShot)
                InGameManager.IT.SetCamera(projectile.transform.name, 10);
        
            projectile = null; // 미사일 초기화
        }
    }
    
    private void SetSprite()
    {
        // angle에 따라 탱크 스프라이트의 각도 설정
        float ang = Mathf.Clamp(angle, -45, 45);
        ang = (int)(ang * 5) / 5; // 5단위로 나눔
        sprite.transform.localEulerAngles = Vector3.forward * ang;
        // switch
        // {
        //     >= -60 and < -30 => Vector3.forward * -30,
        //     >= -30 and < -15 => Vector3.forward * -15,
        //     > -15 and < 15 => Vector3.zero,
        //     >= 15 and < 30 => Vector3.forward * 15,
        //     >= 30 and < 60 => Vector3.forward * 30,
        //     _ => sprite.transform.localEulerAngles
        // };
    }

    public void Hit(float damage)
    {
        // 소수점 버림
        damage = Mathf.Floor(damage);
        
        currentHP -= damage; // 현재 체력 감소

        if (currentHP <= 0) // 현재 체력이 0보다 작거나 같다면
        {
            currentHP = 0; // 현재 체력을 0으로 설정
            
            isDead = true; // 탱크 사망 처리
            
            Die(); // 죽음
        }

        // damage = 9999 -> 탱크가 맵 밖으로 나갔다는 뜻
        if (damage != 9999)
        {
            InGameManager.IT.SetDamage(damage); // 누적 데미지 리스트에 데미지 추가
        }
        
        PV.RPC(nameof(RPC_UpdateUI), RpcTarget.All, currentHP, damage); // RPC_UpdateUI 함수 호출
    }

    [PunRPC]
    private void RPC_UpdateUI(float currentHP, float damage)
    {
        tankUIHandler.UpdateUI(currentHP, damage); // 탱크 UI 갱신
        
        if (PV.IsMine) // 내 탱크라면
        {
            // AI라면 리턴
            if (PhotonNetwork.IsMasterClient && isAi)
                return;
            
            UIManager.IT.SetHP(currentHP); // HP UI 갱신
        }
    }

    private void Die()
    {
        PV.RPC(nameof(RPC_Die), RpcTarget.All); // RPC_Die 함수 호출
    }
    
    [PunRPC]
    private void RPC_Die()
    {
        GetComponent<Collider2D>().enabled = false; // 탱크 콜라이더 비활성화
        transform.GetChild(0).gameObject.SetActive(false); // 탱크 스프라이트 비활성화
        transform.GetChild(1).gameObject.SetActive(false); // 탱크 UI 비활성화
        transform.GetChild(2).gameObject.SetActive(false); // 탱크 미니맵 아이콘 비활성화
    }

    public void SetTeamNum(int t) {
        teamNum = t;
    }

    [PunRPC]
    private void RPC_SetTeamNum() {
        if (transform.name.Contains("AI")) return ;

        Debug.Log("tankHandler RPC setTeamNum");
        foreach (var slot in NetworkManager.IT.gameForSlots) {
            if (slot.actorNumber > 0)
                Debug.Log("transName: " + transform.name + ", slot.nickName : " + slot.nickName + ", slot.t : " + slot.teamNumber);
            if (transform.name == slot.nickName) {
                teamNum = slot.teamNumber;
                break;
            }
        }
        if (transform.name == PV.Owner.NickName) {
            if (teamNum == 1)
                teamNum = 3; // bright red
            else if (teamNum == 2)
                teamNum = 4; // bright blue
            else
                teamNum = -1; // yellow
        }

        tankUIHandler.SetColor(teamNum);
    }

    public int GetDirection()
    {
        return direction;
    }
}
