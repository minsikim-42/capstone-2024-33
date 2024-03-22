using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AIHandler : MonoBehaviour
{
    public string aiName; // AI 이름
    public int actorNumber; // 플레이어 번호 (AI 번호로 사용)
    
    public TankHandler tankHandler; // ai의 탱크 핸들러
    [SerializeField] private List<TankHandler> targetList = new List<TankHandler>(); // 타겟 리스트
    private TankHandler target; // 타겟

    private float angle; // 각도
    private float power; // 파워
    
    private bool isLeftMove; // 왼쪽 방향 전환
    private bool isRightMove; // 오른쪽 방향 전환
 
    private int reCount = -1; // 재시도 횟수
    private float preX; // 이전 x값
    private void Awake()
    {
        tankHandler = GetComponent<TankHandler>(); // 탱크 핸들러 할당
        actorNumber = tankHandler.PV.ViewID; // 플레이어 번호 설정
        aiName = "AI_" + (actorNumber - 1000); // AI 이름 설정
        transform.name = aiName; // 이름 설정
        tankHandler.isAi = true; // AI 여부 설정
    }

    private void Init()
    {
        tankHandler.isTurn = true; // 차례 설정
        
        reCount = -1; // 재시도 횟수 초기화
        preX = 10000; // 이전 x값 초기화
        UpdateTargetInfo(); // 타겟 정보 업데이트
        
        SetNearestTarget(); // 가장 가까운 타겟 설정

        Calculate(); // 각도, 파워 계산
    }
    
    // ai 차례임을 호출 받았을 때
    public void OnTurn()
    {
        Init(); // 초기화
    }

    private void UpdateTargetInfo()
    {
        targetList.Clear(); // targetList 초기화
        
        var tanks = FindObjectsOfType<TankHandler>(); // TankHandler를 찾아서 targetList에 추가

        foreach (var tank in tanks)
        {
            // 자신은 제외
            if (tank.name == gameObject.name)
                continue;
            
            // 체력이 0 이하면 제외
            if (tank.currentHP <= 0)
                continue;
            
            targetList.Add(tank); // targetList에 추가
        }
    }

    private void SetNearestTarget()
    {
        // targetList에서 가장 가까운 탱크를 찾아서 타겟으로 설정
        var minDistance = float.MaxValue;
        target = null;
        
        // targetList를 순회하면서 가장 가까운 탱크를 찾음
        foreach (var tank in targetList)
        {
            var distance = Vector3.Distance(transform.position, tank.transform.position); // 거리 계산
            
            // 거리가 minDistance보다 작으면 distance로 minDistance보다 설정하고 타겟을 tank로 설정
            if (distance < minDistance)
            {
                minDistance = distance;
                target = tank;
            }
        }
    }
    
    private void SetRandomTarget()
    {
        // targetList에서 랜덤으로 타겟 설정
        if (targetList.Count > 0)
        {
            var randomIndex = Random.Range(0, targetList.Count); // 랜덤 인덱스 설정
            
            // 타겟 설정
            target = targetList[randomIndex]; 
        }
    }

    private void Calculate()
    {
        reCount++; // 재시도 횟수 증가

        if (target == null)
        {
            Debug.LogWarning("타겟이 없습니다.");
        }
        else
        {
            var myPosition = transform.position; // 내 위치
            var targetPosition = target.transform.position; // 타겟 위치
            
            isLeftMove = false; // 왼쪽 방향 전환 초기화
            isRightMove = false; // 오른쪽 방향 전환 초기화
            
            // 타겟이 왼쪽에 있는지 오른쪽에 있는지 확인
            var direction = 1;
            if (targetPosition.x < myPosition.x)
            {
                direction = -1;

                if (transform.localScale.x != -1) isLeftMove = true;
            }
            else
            {
                direction = 1;
                
                if (transform.localScale.x != 1) isRightMove = true;
            }
            
            // 탱크가 낭떠러지에 가까운지 확인 (-1: 왼쪽, 1: 오른쪽, 0: 중앙)
            var checkPosition = CheckPosition();
            
            // 랜덤 이동
            if (Random.Range(0, 2) == 0)
            {
                if (checkPosition == -1)
                    tankHandler.RepeatRightArrowPressed(1);
                else if (checkPosition == 1)
                    tankHandler.RepeatLeftArrowPressed(1);
                else
                {
                    if (Random.Range(0, 2) == 0)
                        tankHandler.RepeatRightArrowPressed(1);
                    else
                        tankHandler.RepeatLeftArrowPressed(1);
                }

                preX = target.transform.position.x; // 이전 x값 설정
                    
                Invoke(nameof(Calculate), 1); // 1초 후 다시 계산
                
                return;
            }

            
            // 두 점을 포물선 공식에 대입하여 각도 계산
            var g = Physics.gravity.y; // 중력 가속도
            
            angle = Random.Range(30, 60); // 각도 랜덤 설정
            
            var launchSpeed = CalculateLaunchSpeed(targetPosition, myPosition, angle); // 발사 속도 계산
            
            float CalculateLaunchSpeed(Vector3 targetPosition, Vector3 launchPosition, float angle)
            {
                var theta = angle * Mathf.Deg2Rad; // 각도를 라디안으로 변환
                var delta = targetPosition - launchPosition; // 타겟까지의 벡터 차이
                var deltaDistance = Vector2.Distance(new Vector2(delta.x, delta.z), Vector2.zero); // 수평 거리
                var deltaY = delta.y; // 수직 거리

                // 발사 속도 계산의 일부
                var numerator = g * Mathf.Pow(deltaDistance, 2);
                var denominator = 2 * (deltaY - Mathf.Tan(theta) * deltaDistance) * Mathf.Pow(Mathf.Cos(theta), 2);
                var velocity = Mathf.Sqrt(numerator / denominator);

                return velocity;
            }

            // launchSpeed가 NaN이면
            if (float.IsNaN(launchSpeed))
            {
                // 재시도 횟수가 4보다 작으면 재시도
                if (reCount < 4)
                {
                    if (direction == 1)
                    {
                        // preX와 targetPosition.x가 2 이하로 차이가 나지 않으면 방향을 바꿔서 재시도
                        if (Mathf.Abs(preX - targetPosition.x) < 2)
                            tankHandler.RepeatLeftArrowPressed(1);
                        else
                            tankHandler.RepeatRightArrowPressed(1);
                    }
                    else
                    {
                        // preX와 targetPosition.x가 2 이하로 차이가 나지 않으면 방향을 바꿔서 재시도
                        if (Mathf.Abs(preX - targetPosition.x) < 2)
                            tankHandler.RepeatRightArrowPressed(1);
                        else
                            tankHandler.RepeatLeftArrowPressed(1);
                    }
                    
                    preX = target.transform.position.x; // 이전 x값 설정
                    
                    Invoke(nameof(Calculate), 1); // 1초 후 다시 계산
                }
                
                return;
            }

            power = launchSpeed * 6.67f * Random.Range(0.9f, 1.0f); // 파워 랜덤 설정
            
            // power는 100보다 크면 100으로 설정
            if (power > 100)
            {
                // 재시도 횟수가 4보다 작으면 재시도
                if (reCount < 4)
                {
                    if (direction == 1)
                        tankHandler.RepeatRightArrowPressed(1);
                    else
                        tankHandler.RepeatLeftArrowPressed(1);
                    
                    Invoke(nameof(Calculate), 1);
                    
                    return;
                }

                power = 100;
            }
            
            // angle이 NaN이면 0으로 설정
            if (float.IsNaN(angle))
            {
                if (direction == 1)
                    tankHandler.RepeatRightArrowPressed(1);
                else
                    tankHandler.RepeatLeftArrowPressed(1);
                
                return;
            }
            
            tankHandler.projectileDegrees = 0; // 각도 초기화
            tankHandler.currentPowerValue = 0; // 파워 초기화
            
            angle *= Random.Range(0.9f, 1.1f); // 각도 랜덤 설정
            
            StartCoroutine(SetAngle()); // 각도 설정 코루틴 시작
        }
    }
    private int CheckPosition()
    {
        // 왼쪽 낭떠러지에 가까운 경우
        if (transform.localPosition.x <= -25)
            return -1;

        // 오른쪽 낭떠러지에 가까운 경우
        if (transform.localPosition.x >= 25)
            return 1;

        return 0;
    }
    
    // Angle Set Coroutine
    private IEnumerator SetAngle()
    {
        // Move bool에 따라 추가 이동
        if (isLeftMove)
        {
            // 왼쪽 방향으로 이동
            while (transform.localScale.x != -1)
            {
                tankHandler.RepeatLeftArrowPressed(0.2f);
                
                yield return null;
            }
            
            isLeftMove = false;
            
        }
        else if (isRightMove)
        {
            // 오른쪽 방향으로 이동
            while (transform.localScale.x != 1)
            {
                tankHandler.RepeatRightArrowPressed(0.2f);
                
                yield return null;
            }
            
            isRightMove = false;
        }
        
        // 목표 각도까지 키보드 입력
        while (tankHandler.projectileDegrees + tankHandler.angle < angle)
        {
            tankHandler.UpArrowPressed(); // 각도 조정

            // tankHandler.projectileDegrees이 90도 크면 while문 탈출
            if (tankHandler.projectileDegrees >= 90)
                break;

            yield return null;
        }
        
        StartCoroutine(SetPower()); // 파워 설정 코루틴 시작
    }
    
    // Power Set Coroutine
    private IEnumerator SetPower()
    {
        tankHandler.SpaceDown(); // 파워 조정

        // 파워가 목표 파워보다 작으면 키보드 입력
        while (tankHandler.currentPowerValue <= power)
        {
            tankHandler.SpacePressed();

            yield return null;
        }

        tankHandler.SpaceUp(); // 파워 조정 종료
        
        tankHandler.isTurn = false; // 차례 종료
    }
    
    public void TurnOver()
    {
        StopAllCoroutines(); // 모든 코루틴 정지
        
        tankHandler.TurnOverFire();
    }
}
