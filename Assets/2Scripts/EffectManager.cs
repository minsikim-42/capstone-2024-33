using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager instance;

    [SerializeField] private GameObject projectileEffect; // 포탄 이펙트
    
    GameObject effect; // 이펙트 오브젝트
    
    private void Awake()
    {
        instance = this;
    }
    
    public void ActiveProjectileEffect(Vector3 position)
    {
        // 이펙트가 활성화되어 있으면 리턴
        if (effect != null)
            return;
        
        effect = Instantiate(projectileEffect, position, Quaternion.identity); // 포탄 이펙트 생성
        
        Destroy(effect, 1f); // 1초 후 이펙트 삭제
        
        effect = null; // 이펙트 초기화
    }
}
