using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class InGameManager : MonoBehaviour
{
    public static InGameManager IT; // 싱글 톤
    
    private TankHandler playerTankHandler; // 플레이어 탱크 핸들러
    private PhotonView PV; // 포톤 뷰
    private CameraHandler cameraHandler; // 카메라 핸들러
    
    private string playerPrefabName = "Tank"; // 플레이어 프리팹 이름
    private string aiPrefabName = "AI"; // AI 프리팹 이름

    public bool isTeamMode; // 팀전인지 개인전인지
	public int gameMode; // 쉬움 & 어려움
    public int redTeam1; // 빨강팀 수
    public int blueTeam2; // 플루팀 수
    public float MAX_WIDTH = 1600;
    public float MAX_HEIGHT = 1200;

    public float projectileMass = 3.3f; // 포탄 무게
    public float lineRenderCount = 2.5f; // 라인렌더러 포인트 수
    
    [Header("Spawn Points")]
    [SerializeField] private List<Transform> spawnPoints; // 스폰 포인트 리스트
    [SerializeField] private Transform spawnPoint; // 스폰 포인트
    [SerializeField] private List<Transform> aiSpawnPoints; // AI 스폰 포인트 리스트

    
    [Header("Turn")]
    [SerializeField] private int turnIndex; // 턴 순서
    [SerializeField] private List<Turn> turnList = new List<Turn>(); // 턴 리스트
    [SerializeField] private List<Damage> damageList = new List<Damage>(); // 누적 데미지 리스트

    [Header("Timer")]
    [SerializeField] private TextMeshProUGUI timerText; // 타이머 텍스트
    [SerializeField] private int turnTime = 30; // 턴 시간
    private int currentTurnTime; // 현재 턴 시간
    private float timer; // 타이머
    private bool isPlayingTimer; // 타이머 플레이 여부

    private Player currentTurnPlayer; // 현재 턴의 플레이어
    private AIHandler currentTurnAI; // 현재 턴의 AI
    
    [Header("Items")]
    public bool isDoubleShot = false; // Double Shot 여부
    public bool isAttackRange = false; // Attack Range 여부
    public bool isAttackDamage = false; // Attack Damage 여부

    public int doubleShot = 1;
    public int attackRange = 1;
    public int attackDamage = 1;
    
    [Header("Distance")]
    public Vector3 initialPosition; // 발사 포탄의 초기 위치
    
    [Header("Hit Effect")]
    [SerializeField] private GameObject hitEffectPrefab; // 히트 이펙트 프리팹
    
    [Header("Wind")]
    public int maxWindPower = 10;
    public int windPower; // 바람 세기
    public float windPowerCoefficient = 0.5f; // 바람 세기 계수 (강도 조절용)
    public float powerCoefficient = 50;
    [Header("AI")]
    private List<AIHandler> aiList = new List<AIHandler>(); // AI 핸들러 리스트
    
    [Header("MaxHp")]
    public float maxHP;

    private void Awake()
    {
        IT = this;
        
        PV = GetComponent<PhotonView>(); // 포톤 뷰 설정
        
        cameraHandler = FindObjectOfType<CameraHandler>(); // 카메라 핸들러 설정

        isTeamMode = LobbyManager.IT.isTeamMode;
		gameMode = NetworkManager.IT.GetGameMode();
		if (gameMode == 1) // easy
		{
			//
		}
		else // hard
		{
			maxWindPower = 20;
			turnTime = 25;
			powerCoefficient = 75;
		}
    }

    private async void Start()
    {
        UIManager.IT.SetDarkScreen(); // 어두운 화면 설정
        

        if (PhotonNetwork.IsMasterClient)
        {
            if (isTeamMode) {
                redTeam1 = NetworkManager.IT.GetTeamNum(1);
                blueTeam2 = NetworkManager.IT.GetTeamNum(2);
            }
            // spawnPoints 중 모든 플레이어에게 랜덤으로 하나의 Spawn Point를 할당 (중복 X)
            // Shuffle Spawn Points
            var spawnPoints = new List<Transform>(this.spawnPoints); // 스폰 포인트 리스트 복사
            for (var i = 0; i < spawnPoints.Count; i++)
            {
                var temp = spawnPoints[i]; // 임시 저장
                var randomIndex = Random.Range(i, spawnPoints.Count); // 랜덤 인덱스
                spawnPoints[i] = spawnPoints[randomIndex]; // 랜덤 인덱스의 값을 i번째에 저장
                spawnPoints[randomIndex] = temp; // i번째의 값을 랜덤 인덱스에 저장
            }
            
            // Set Spawn Points
            int index = 0;
            // Player
            for (var i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                PV.RPC(nameof(RPC_SetSpawnPoint), PhotonNetwork.PlayerList[i], spawnPoints[i].GetSiblingIndex()); // 스폰 포인트 설정
                
                index = i;
            }
            
            // AI
            if (NetworkManager.IT.aiCount > 0)
            {
                aiSpawnPoints = new List<Transform>(); // AI 스폰 포인트 리스트 초기화
                
                for (var i = index + 1; i < spawnPoints.Count; i++)
                {
                    aiSpawnPoints.Add(spawnPoints[i]); // AI 스폰 포인트 추가
                }
            }
        
            await Task.Delay(1000); // 1초 대기
            
            PV.RPC(nameof(RPC_GenerateTank), RpcTarget.All);
        }
    }

    [PunRPC]
    private void RPC_SetSpawnPoint(int spawnPointIndex)
    {
        spawnPoint = spawnPoints[spawnPointIndex]; // 스폰 포인트 설정
    }


    [PunRPC]
    private async void RPC_GenerateTank()
    {
        var player = PhotonNetwork.Instantiate(playerPrefabName, spawnPoint.position, Quaternion.identity); // 플레이어 생성
        
        playerTankHandler = player.GetComponent<TankHandler>(); // 플레이어 탱크 핸들러 설정
        
        // 카메라 설정
        cameraHandler.SetTarget(player.transform); // 카메라 타겟 설정
        cameraHandler.StopZoom();
        cameraHandler.Zoom(9); // 카메라 줌
        
        UIManager.IT.SetFade(true); // 페이드 효과
        
        if (PhotonNetwork.IsMasterClient)
        {
            GenerateAI(); // AI 생성

            SetTurn(); // 모든 플레이어, AI의 턴을 설정
            
            await Task.Delay(5000); // 5초 대기
            
            StartTurn(); // 턴 시작
        }
        else
        {
            await Task.Delay(1000); // 5초 대기
            GetTurn();
        }
    }
    private void GenerateAI()
    {
        // AI가 존재하지 않을 경우 리턴
        if (NetworkManager.IT.aiCount == 0) 
            return;
        
        // AI가 존재할 경우
        for (var i = 0; i < NetworkManager.IT.aiCount; i++)
        {
            var aiPosition = aiSpawnPoints[i].position; // AI 스폰 포인트
            var ai = PhotonNetwork.Instantiate(aiPrefabName, aiPosition, Quaternion.identity).GetComponent<AIHandler>(); // AI 생성
            ai.tankHandler.SetTeamNum(1);
            Debug.Log(ai.actorNumber); // AI actorNumber
                
            aiList.Add(ai); // AI 리스트에 추가
        }
    }

    private void SetTurn() // Master
    {
        turnList.Clear(); // 턴 리스트 초기화

        int aiIdx=0;
        // foreach (var slot in LobbyManager.IT.GetSlots()) { // 왜 로비매니저인데 작동하지?
        foreach (var slot in NetworkManager.IT.gameForSlots) {
            if (slot.actorNumber == 99)
            {
                var turn = new Turn { actorNumber = aiList[aiIdx].actorNumber, nickName = "AI_"+(aiList[aiIdx].actorNumber-1000), teamNumber = slot.teamNumber }; // AI턴 생성
                turnList.Add(turn);
                aiIdx++;
            }
            else if (slot.actorNumber != -1)
            {
                var turn = new Turn { actorNumber = slot.actorNumber, nickName = slot.nickName, teamNumber = slot.teamNumber }; // Player턴 생성
                turnList.Add(turn);
            }
            else
            {
                // 빈 슬롯
            }
        }

        turnList = NetworkManager.IT.ShuffleTurn(turnList);

        Debug.Log("InGameM::SetTurn");
        foreach (var slot in turnList) {
            if (slot.actorNumber == 99)
            {
                Debug.Log("AIslot actorNum: " + slot.actorNumber);
                Debug.Log("AIslot nickname: " + slot.nickName);
            }
            else if (slot.actorNumber != -1)
            {
                Debug.Log("slot actor Num: " + slot.actorNumber);
                Debug.Log("slot nickname: " + slot.nickName);
            }
            else
            {
                // 빈 슬롯
            }
        }
        
        damageList.Clear(); // 누적 데미지 리스트 초기화
        
        // 누적 데미지 리스트에 플레이어, AI 추가
        // foreach (var damage in turnList.Select(turn => new Damage { nickname = turn.nickname, damage = 0}))
        // {
        //     damageList.Add(damage); // 데미지 리스트에 추가
        // }
        foreach (var turn in turnList)
        {
            var damage = new Damage { nickName = turn.nickName, damage = 0, teamNumber = turn.teamNumber };
            damageList.Add(damage);
        }
    }

    private void GetTurn()
    {
        Debug.Log("InGameM::GetTurn");
        foreach (var slot in NetworkManager.IT.gameForSlots) {
            if (slot.actorNumber == 99)
            {
                Debug.Log("AIslot actorNum: " + slot.actorNumber);
                Debug.Log("AIslot nickname: " + slot.nickName);
                var turn = new Turn { actorNumber = -1, nickName = "AI_"+(-1), teamNumber = slot.teamNumber }; // AI턴 생성 // AiList[aiIdx].~
                turnList.Add(turn);
            }
            else if (slot.actorNumber != -1)
            {
                Debug.Log("slot actor Num: " + slot.actorNumber);
                Debug.Log("slot nickname: " + slot.nickName);
                var turn = new Turn { actorNumber = slot.actorNumber, nickName = slot.nickName, teamNumber = slot.teamNumber }; // Player턴 생성
                turnList.Add(turn);
            }
            else
            {
                // 빈 슬롯
            }
        }
    }

    private void StartTurn()
    {
        turnIndex = 0; // 턴 인덱스 초기화
        
        var turn = turnList[turnIndex]; // 현재 턴
        var actorNumber = turn.actorNumber; // 현재 턴의 ActorNumber
        
        timer = 1; // 타이머 초기화
        currentTurnTime = turnTime; // 현재 턴 시간 설정
        isPlayingTimer = true; // 플레이 중
        
        // 플레이어의 턴일 경우
        if (actorNumber < 1000)
        {
            isAiTurn = false; // AI 턴이 아님
            
            currentTurnAI = null; // 현재 턴의 AI 초기화
            
            SendTurn(actorNumber); // 턴을 넘김
        }
        // AI의 턴일 경우
        else
        {
            isAiTurn = true; // AI 턴
            
            var ai = aiList.Find(a => a.actorNumber == actorNumber); // AI 찾기
            
            currentTurnAI = ai; // 현재 턴의 AI 설정
            currentTurnPlayer = null; // 현재 턴의 플레이어 초기화
            
            SetWind(); // 바람 세기 설정 (RPC)
            
            SetCamera(ai.name); // 카메라 설정 (RPC)
            
            SetMissilePlayerNickname(ai.name); // 미사일 발사한 닉네임 설정
            
            UIManager.IT.ItemButtonInit(); // 아이템 버튼 초기화
            
            ai.OnTurn(); // AI의 턴 시작
        }
    }

    private bool isAiTurn = false;
    
    public bool IsAITurn()
    {
        return isAiTurn;
    }
    
    private string missilePlayerNickname; // 미사일 발사한 플레이어의 닉네임
    
    private void SetMissilePlayerNickname(string nickname)
    {
        missilePlayerNickname = nickname; // 미사일 발사한 플레이어의 닉네임 설정
    }
    
    // 누적 데미지 리스트에 데미지 추가
    public void SetDamage(float damage)
    {
        var turn = damageList.Find(t => t.nickName == missilePlayerNickname); // 턴 찾기
        turn.damage += (int)damage; // 누적 데미지 추가
    }
    
    // RPC로 player에게 턴을 넘김 
    private void SendTurn(int actorNumber)
    {
        var player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber); // 플레이어 찾기 (ActorNumber)
        
        SetMissilePlayerNickname(player.NickName); // 미사일 발사한 닉네임 설정

        currentTurnPlayer = player; // 현재 턴의 플레이어 설정
        
        PV.RPC(nameof(RPC_SendTurn), player); // RPC로 턴을 넘김
        
        // // 나머지 플레이어에게는 player의 턴을 알림
        // foreach (var otherPlayer in PhotonNetwork.PlayerList)
        // {
        //     // 다른 플레이어일 경우
        //     if (otherPlayer.ActorNumber != actorNumber)
        //         PV.RPC(nameof(RPC_SendOtherTurn), otherPlayer, currentTurnPlayer.NickName); // RPC로 플레이어의 턴을 알림
        // }
        PV.RPC(nameof(RPC_SendOtherTurn), RpcTarget.Others, currentTurnPlayer.NickName); // RPC로 플레이어의 턴을 알림
        
        SetWind(); // 바람 세기 설정 (RPC)
        SetCamera(player.NickName); // 카메라 설정 (RPC)
    }

    private void SetWind()
    {
        var wind = Random.Range(-maxWindPower, maxWindPower+1); // -10 ~ 10
        
        PV.RPC(nameof(RPC_SetWind), RpcTarget.All, wind); // RPC로 풍향 설정
    }
    
    [PunRPC]
    private void RPC_SetWind(int wind)
    {
        UIManager.IT.SetWind(wind); // UI에 풍향 설정
        
        windPower = wind; // 풍향 설정
    }
    
    [PunRPC]
    private void RPC_SendTurn()
    {
        UIManager.IT.ItemButtonInit(); // 아이템 버튼 초기화
        
        playerTankHandler.isTurn = true; // 플레이어의 턴을 시작
        playerTankHandler.MoveValueInit(); // 이동 값 초기화
        playerTankHandler.AngleInit(); // 각도 초기화

        UIManager.IT.selectableItem = true; // 아이템 선택 가능
        
        // 아이템 사용 여부 초기화
        isDoubleShot = false; 
        isAttackRange = false;
        isAttackDamage = false;
    }
    
    [PunRPC]
    private void RPC_SendOtherTurn(string nickname)
    {
        UIManager.IT.ItemButtonInit(); // 아이템 버튼 초기화
        
        playerTankHandler.AngleInit(); // 각도 초기화
    }
    
    public void SetCamera(string targetPlayerNickName, float value = 9)
    {
        PV.RPC(nameof(RPC_SetCamera), RpcTarget.All, targetPlayerNickName, value); // RPC로 카메라 설정
    }
    
    [PunRPC]
    private void RPC_SetCamera(string targetPlayerNickName, float value = 9)
    {
        var target = GameObject.Find(targetPlayerNickName); // 플레이어 찾기

        if (target != null)
        {
            cameraHandler.SetTarget(target.transform); // 카메라 타겟 설정
            cameraHandler.StopZoom();
            cameraHandler.Zoom(value); // 카메라 줌   
        }
    }
    
    
    private void Update()
    {
        if (PhotonNetwork.IsMasterClient) // 방장일 경우
        {
            // R 누르면 Result
            if (GameManager.IT.IsTestMode() && Input.GetKeyDown(KeyCode.R))
                Result(0);
            
            if (isPlayingTimer) // 플레이 중일 경우
            {
                timer -= Time.deltaTime; // 타이머 감소

                if (timer <= 0) // 타이머가 0 이하일 경우
                {
                    timer = 1; // 타이머 초기화
                    currentTurnTime--; // 현재 턴 시간 감소

                    if (currentTurnTime <= 0) // 현재 턴 시간이 0 이하일 경우
                    {   
                        UpdateTimer(); // 타이머 갱신 (RPC)
                        
                        TurnOver(); // 턴 종료
                        
                        return;
                    }
                    
                    UpdateTimer(); // 타이머 갱신 (RPC)
                }
            }
        }
    }

    public void CamZoom(float size)
    {
        cameraHandler.Zoom(size);
    }
    public void CamStopZoom()
    {
        cameraHandler.StopZoom();
    }
    public void CamMove(float x, float y)
    {
        cameraHandler.Move(x, y);
    }
    public void CamMoveBack()
    {
        cameraHandler.MoveBack();
    }
    
    private void UpdateTimer()
    {
        PV.RPC(nameof(RPC_UpdateTimer), RpcTarget.All, currentTurnTime); // 타이머 갱신 (RPC)
    }
    
    [PunRPC]
    private void RPC_UpdateTimer(int time)
    {
        timerText.text = time.ToString();
    }
    
    public bool GetIsTurn(string nickName)
    {
        return turnList[turnIndex].nickName == nickName;
    }

    private void TurnOver()
    {
        EndTurn(); // 턴 종료
    }
    
    public void NextTurn()
    {
        // 현재 씬이 Game이 아닐 경우 리턴
        if (SceneManager.GetActiveScene().name != "Game") return;
        
        ResetTimer(); // 타이머 초기화
        
        UpdateTurnList(); // 턴 리스트 갱신
        
        turnIndex++; // 턴 인덱스 증가
        
        if (turnIndex >= turnList.Count) // 턴 인덱스가 턴 리스트의 크기 이상일 경우
        {
            turnIndex = 0; // 턴 인덱스 초기화
        }

        PV.RPC(nameof(RPC_SetTurn), RpcTarget.Others, turnIndex);
        
        var turn = turnList[turnIndex]; // 현재 턴
        var actorNumber = turn.actorNumber; // 현재 턴의 ActorNumber
        
        timer = 1; // 타이머 초기화
        currentTurnTime = turnTime; // 현재 턴 시간 설정
        isPlayingTimer = true; // 플레이 중
        
        // 플레이어의 턴일 경우
        if (actorNumber < 1000)
        {
            isAiTurn = false; // AI 턴이 아님
            
            currentTurnAI = null; // 현재 턴의 AI 초기화
            
            SendTurn(actorNumber); // 턴을 넘김
        }
        // AI의 턴일 경우
        else
        {
            isAiTurn = true; // AI 턴
            
            var ai = aiList.Find(a => a.actorNumber == actorNumber); // AI 찾기
            
            currentTurnAI = ai; // 현재 턴의 AI 설정
            currentTurnPlayer = null; // 현재 턴의 플레이어 초기화
            
            SetWind(); // 바람 세기 설정 (RPC)
            
            SetCamera(ai.name); // 카메라 설정 (RPC)
            
            SetMissilePlayerNickname(ai.name); // 미사일 발사한 닉네임 설정
            
            UIManager.IT.ItemButtonInit(); // 아이템 버튼 초기화

            ai.OnTurn(); // AI의 턴 시작
        }
    }

    [PunRPC]
    private void RPC_SetTurn(int t)
    {
        turnIndex = t;
    }

    public void NextTurnToMaster()
    {
        PV.RPC(nameof(RPC_NextTurn), RpcTarget.MasterClient);
    }
    
    [PunRPC]
    private void RPC_NextTurn()
    {
        NextTurn();
    }


    
    private void EndTurn()
    {
        if (currentTurnPlayer == null)
        {
            EndAITurn(); // AI 턴 종료
        }
        else
        {
            PV.RPC(nameof(RPC_EndTurn), currentTurnPlayer); // 턴 종료 (RPC)
            currentTurnPlayer = null; // 현재 턴 플레이어 초기화    
        }
    }
    
    [PunRPC]
    private void RPC_EndTurn()
    {
        playerTankHandler.isTurn = false; // 플레이어의 턴 종료
        playerTankHandler.TurnOverFire(); // 발사
    }

    private void EndAITurn()
    {
        currentTurnAI.tankHandler.isTurn = false; // AI의 턴 종료
        currentTurnAI.TurnOver(); // AI의 턴오버
    }
    
    private void ResetTimer()
    {
        timer = 1; // 타이머 초기화
        currentTurnTime = turnTime; // 현재 턴 시간 설정
    }

    public void StopTimer()
    {
        PV.RPC(nameof(RPC_StopTimer), RpcTarget.MasterClient);
    }
    
    [PunRPC]
    private void RPC_StopTimer()
    {
        isPlayingTimer = false; // 플레이 중지
    }
    
    public async void StartTimer()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            await Task.Delay(3000);
        
            NextTurn(); // 다음 턴으로 넘김    
        }
    }
    
    // 플레이어가 존재하는지 확인
    private void UpdateTurnList()
    {
        var turnListToRemove = new List<Turn>(); // 제거할 턴 리스트
        
        foreach (var turn in turnList)
        {
            if (turn.nickName != string.Empty)
                turn.nickName = string.Empty; // 닉네임 초기화
            
            // 플레이어 (actorNumber < 1000)
            if (turn.actorNumber < 1000) 
            {
                var player = PhotonNetwork.CurrentRoom.GetPlayer(turn.actorNumber); // 플레이어 찾기 (ActorNumber)
                
                // 플레이어가 없을 경우
                if (player == null)
                {
                    turnListToRemove.Add(turn); // 제거할 턴 리스트에 추가
                    
                    DelTeamCount(turn.teamNumber);
                }
                // 플레이어가 존재할 경우
                else
                {
                    // 체력 0 이하일 경우 (죽었을 경우) 턴 리스트에서 제거
                    var tank = GameObject.Find(player.NickName).GetComponent<TankHandler>(); // 탱크 찾기

                    if (tank.currentHP <= 0)
                    {
                        turnListToRemove.Add(turn); // 제거할 턴 리스트에 추가
                        
                        DelTeamCount(turn.teamNumber);
                    }
                }
            }
            // AI (actorNumber >= 1000)
            else
            {
                // AI가 존재할 경우
                if (GameObject.Find("AI_" + (turn.actorNumber - 1000)) == null)
                {
                    // 체력 0 이하일 경우 (죽었을 경우) 턴 리스트에서 제거
                    turnListToRemove.Add(turn); // 제거할 턴 리스트에 추가
                    
                    DelTeamCount(turn.teamNumber);
                }
                // AI가 없을 경우
                else
                {
                    var ai = aiList.Find(a => a.actorNumber == turn.actorNumber); // AI 찾기
                    
                    // 체력 0 이하일 경우 (죽었을 경우) 턴 리스트에서 제거
                    if (ai.tankHandler.currentHP <= 0)
                    {
                        turnListToRemove.Add(turn); // 제거할 턴 리스트에 추가
                        
                        DelTeamCount(turn.teamNumber);
                    }
                }
            }
        }

        // 제거할 턴 리스트가 없을 경우 리턴
        if (turnListToRemove.Count == 0) return;
        // 제거할 턴 리스트가 있을 경우
        foreach (var turn in turnListToRemove)
        {
            turnList.Remove(turn); // 턴 리스트에서 제거
        }
        
        // 팀모드일경우 한팀의 모든 플레이어가 없을경우
        if (isTeamMode)
        {
            if (redTeam1 < 1)
            {
                Result(2); // Blue(2) Team Win
            }
            else if (blueTeam2 < 1)
            {
                Result(1); // Red(1) Team Win
            }
        }
        // turnList.Count가 1이면 결과 표시
        else if (turnList.Count <= 1)
            Result(0); // 결과 표시
    }

    private void DelTeamCount(int t)
    {
        if (isTeamMode)
        {
            if (t == 1)
            {
                Debug.Log("del team1");
                redTeam1--;
            }
            else
            {
                Debug.Log("del team2");
                blueTeam2--;
            }
        }
    }

    public void TankHitEffect(Vector3 pos)
    {
        var effect = Instantiate(hitEffectPrefab, pos, Quaternion.identity); // 히트 이펙트 생성
        
        Destroy(effect, 1); // 1초 후 삭제
    }
    
    
    public void ExitInGame()
    {
        // 방장이 방을 나갈 경우 
        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC(nameof(RPC_ExitInGame), RpcTarget.All);
        }
        // 방장이 아닌 플레이어가 방을 나갈 경우
        else // 나머지 클라이언트에게 알림
        {
            PV.RPC(nameof(RPC_LeaveUpdateTurn), RpcTarget.Others, PhotonNetwork.LocalPlayer.NickName);
            LeaveRoom(); // 방 나가기
        }
    }
    
    [PunRPC]
    private void RPC_ExitInGame()
    {
        LeaveRoom(); // 방 나가기
    }

    private void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom(); // 방 나가기

        PhotonNetwork.LoadLevel("Lobby"); // 로비 씬으로 이동
    }

    [PunRPC]
    private void RPC_LeaveUpdateTurn(string nickName)
    {
        var turn = turnList.Find((t) => t.nickName == nickName);
        turnList.Remove(turn);
    }
    
    public void Result(int t)
    {
        Debug.Log("result fnc! t: " + t);
        var data = string.Empty; // 데이터

        // damage가 큰 순으로 정렬
        damageList = damageList.OrderByDescending(d => d.damage).ToList();
        
        foreach (var d in damageList)
        {
            data += d.nickName + "/" + d.damage + "/" + d.teamNumber + ",";
            
            // 마지막이라면
            if (d == damageList[^1])
                data = data.Remove(data.Length - 1); // 마지막 콤마 제거
        }
        
        PV.RPC(nameof(RPC_Result), RpcTarget.All, data, t); // RPC로 결과 표시
    }
    
    [PunRPC]
    private void RPC_Result(string data, int t)
    {
        Debug.Log("result RPC fnc!");
        GameManager.IT.isResult = true; // 결과 표시
        GameManager.IT.result = data; // 결과 데이터 저장
        if (t == 0)
            GameManager.IT.resultText = "Result";
        else if (t == 1)
            GameManager.IT.resultText = "Red Team Win!";
        else
            GameManager.IT.resultText = "Blue Team Win!";

        PhotonNetwork.LeaveRoom(); // 방 나가기
        PhotonNetwork.LoadLevel("Lobby"); // 로비 씬으로 이동
    }

    public void DrawLine(float value)
    {
        if (isAiTurn && turnList == null)
            return ;

        Debug.Log("InGame::DrawL::turnIdx:" + turnIndex);
        if (turnList[turnIndex].actorNumber == PhotonNetwork.LocalPlayer.ActorNumber) {
            playerTankHandler.tankUIHandler.DrawLine(value);
        }
        // else
        {
            // Debug.Log("not your Turn");
            // Debug.Log("turnIdx: " + turnIndex + ", turnNick: " + turnList[turnIndex].nickName);
        }
    }

    public void CleanLine()
    {
        playerTankHandler.tankUIHandler.CleanLine();
    }
}

[Serializable]
public class Turn
{
    //public int orderIndex; // 순서 인덱스
    public int actorNumber; // ActorNumber
    public string nickName; // 닉네임
    public int teamNumber; // TeamNumber
}

[Serializable]
public class Damage
{
    public string nickName; // 닉네임
    public int damage; // 데미지
    public int teamNumber;
}