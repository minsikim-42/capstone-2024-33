using UnityEngine;

public class ProjectileHandler : MonoBehaviour
{
    private Rigidbody2D rb;
    private float angle; // 이동에 따른 회전각
    public bool isDoubleShot = false; // 더블샷 여부

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); 
    }

    public void Set(float speed, Vector3 position, int direction, bool isDoubleShot)
    {
        this.isDoubleShot = isDoubleShot; // 더블샷 여부 설정
        
        // 방향 체크
        if (direction == -1)
            position.x = -position.x;

        if (!transform.name.Contains("AI"))
        {
            // 바람 적용
            var windPower = InGameManager.IT.windPower; // 바람 세기
            var windPowerCoefficient = InGameManager.IT.windPowerCoefficient; // 바람 세기 계수

            position.x += windPower / 20f * windPowerCoefficient; // 바람 적용    
        }

        rb.AddForce(position * speed, ForceMode2D.Impulse); // 발사
    }

    private void FixedUpdate()
    {
        angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg; // 이동에 따른 회전각 계산
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward); // 회전
    }

    private void OnDestroy()
    {
        // 더블샷이 아니면 타이머 시작
        if (isDoubleShot)
            return;
        
        InGameManager.IT.StartTimer(); // 타이머 시작
    }
}
