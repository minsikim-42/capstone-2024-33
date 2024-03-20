using Photon.Pun;
using UnityEngine;

public class OutColliderHandler : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D col)
    {
        // 마스터 클라이언트가 아닌 경우 리턴
        if (!PhotonNetwork.IsMasterClient) return;
        
        // layer가 Tank인 경우, 탱크가 맵 밖으로 나갔다는 뜻
        if (col.gameObject.layer == LayerMask.NameToLayer("Tank"))
        {
            var tank = col.GetComponent<TankHandler>(); // 탱크 
                tank.Hit(9999); // 탱크에게 9999의 데미지 적용

            var rigid = tank.GetComponent<Rigidbody2D>(); // 탱크의 Rigidbody2D
                rigid.gravityScale = 0; // 탱크의 중력 초기화
                rigid.velocity = Vector2.zero; // 탱크의 속도 초기화
                rigid.angularVelocity = 0; // 탱크의 각속도 초기화
                
            InGameManager.IT.StopTimer(); // 타이머 멈춤
            InGameManager.IT.StartTimer(); // 타이머 시작
        }
        
        // layer가 Projectile인 경우, 미사일이 맵 밖으로 나갔다는 뜻
        if (col.CompareTag("Projectile"))
        {
            var projectile = col.transform.GetComponent<ProjectileEffector>(); // 미사일 Effector
                projectile.Active(); // 미사일 Effector 활성화
        }
    }
}
