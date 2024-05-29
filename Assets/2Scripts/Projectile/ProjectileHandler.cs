using UnityEngine;
using System.Threading.Tasks;

public class ProjectileHandler : MonoBehaviour
{
    private Rigidbody2D rb;
    private float angle; // 이동에 따른 회전각
    public bool isDoubleShot = false; // 더블샷 여부

    public float massPower;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); 

        rb.mass = InGameManager.IT.projectileMass; // 1.5f: 화살, 3.5f: 미사일
        massPower = rb.mass;
    }

    public void Set(float speed, Vector3 position, int direction, bool isDoubleShot)
    {
        this.isDoubleShot = isDoubleShot; // 더블샷 여부 설정
        
        // 방향 체크
        // position.x *= direction;
        
        Debug.Log("set proj x: " + position.x + ", dir: " + direction);

        // if (!transform.name.Contains("AI"))
        // {
        //     // 바람 적용
        //     var windPower = InGameManager.IT.windPower; // 바람 세기
        //     var windPowerCoefficient = InGameManager.IT.windPowerCoefficient; // 바람 세기 계수

        //     position.x += windPower / 20f * windPowerCoefficient; // 바람 적용    
        // }

        rb.AddForce(position * speed * massPower, ForceMode2D.Impulse); // 발사

        DeleteTimer(5);
    }

    private void FixedUpdate()
    {
        angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg; // 이동에 따른 회전각 계산
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward); // 회전

        var windPower = InGameManager.IT.windPower;
        var windPowerCoefficient = InGameManager.IT.windPowerCoefficient; // 바람 세기 계수
        rb.AddForce(new Vector3(windPower * windPowerCoefficient,0,0), ForceMode2D.Force); // 바람 재적용 // AI 명중률 감소
    }

    private void OnDestroy()
    {
        // 더블샷이 아니면 타이머 시작
        if (isDoubleShot)
            return;
        
        InGameManager.IT.StartTimer(); // 타이머 시작
        InGameManager.IT.CleanLine(); // 예상각도 UI 제거
    }

    private async void DeleteTimer(float sec)
    {
        int t = (int)(sec * 1000);
        await Task.Delay(t);

        if (this)
            Destroy(gameObject);
    }
}
