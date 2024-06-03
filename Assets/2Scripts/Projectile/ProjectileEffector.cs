using UnityEngine;

public class ProjectileEffector : MonoBehaviour
{
    [SerializeField] private GameObject effector; // 미사일 폭발 이펙트
    [SerializeField] private SpriteRenderer sprite; // 미사일 스프라이트

    private void OnCollisionEnter2D(Collision2D col)
    {
        // 충둘한 대상이 맵이 아니거나 플레이어가 아니면 리턴
        if (col.gameObject.CompareTag("Map")) {
			Debug.Log("Collision(Map)");
			Active();
            return;
		}
		if (col.gameObject.CompareTag("Tank")) {
			Debug.Log("Collision(Tank)");
			Active();
            return;
		}
    }

    public void Active()
    {
        effector.SetActive(true); // 폭발 이펙트 활성화
        sprite.enabled = false; // 미사일 스프라이트 비활성화
        Destroy(this.gameObject, 0.02f); // 0.02초 뒤에 미사일 오브젝트 삭제
    }
}
