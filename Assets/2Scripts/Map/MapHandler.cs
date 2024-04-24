using System;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

public class MapHandler : MonoBehaviour
{
    private SpriteRenderer spriteRenderer; // 맵 스프라이트 렌더러
    private Texture2D texture; // 맵 텍스쳐
    [SerializeField] private Texture2D mapTexture; // 사용할 맵 텍스쳐
    private float worldWidth; // 월드 너비
    private float worldHeight; // 월드 높이
    private int pixelWidth; // 픽셀 너비
    private int pixelHeight; // 픽셀 높이
    
    private PhotonView PV; // 포톤 뷰
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        PV = GetComponent<PhotonView>();
        
        // 사용할 맵 텍스쳐로 생성 후 할당
        texture = Instantiate(mapTexture);

        Debug.Log(texture.Size());

        int width = (int)InGameManager.IT.MAX_WIDTH;
        int height = (int)InGameManager.IT.MAX_HEIGHT;
        for (int h=height; h<texture.Size().y; h++) {
            for (int w=0; w<texture.Size().x; w++) {
                texture.SetPixel(w, h, Color.clear);
            }
        }
        for (int w=width; w<texture.Size().x; w++) {
            for (int h=0; h<texture.Size().y; h++) {
                texture.SetPixel(w, h, Color.clear);
            }
        }

        texture.Apply();
        SetSprite();
        
        // 맵 크기 계산
        worldWidth = spriteRenderer.bounds.size.x;
        worldHeight = spriteRenderer.bounds.size.y;
        pixelWidth = spriteRenderer.sprite.texture.width;
        pixelHeight = spriteRenderer.sprite.texture.height;

        gameObject.AddComponent<PolygonCollider2D>(); // 폴리곤 콜라이더 추가
    }

    public void MakeRange(CircleCollider2D collider)
    {
        var center = collider.bounds.center; // 충돌한 콜라이더의 중심 위치

        var attackRangeRatio = InGameManager.IT.isAttackRange ? 1.5f : 1.0f; // Range 아이템 사용 시 1.5배 증가
        
        var radius = Mathf.RoundToInt(collider.bounds.size.x / 2 * pixelWidth / worldWidth * attackRangeRatio); // 반경 (rangeRatio 적용)

        PV.RPC(nameof(RPC_MakeRange), RpcTarget.All, center, radius); // RPC 호출
    }
    
    [PunRPC]
    private void RPC_MakeRange(Vector3 colliderBoundsCenter, int radius)
    {
        var center = GetPixelPosition(colliderBoundsCenter); // 중심 위치 픽셀 좌표
        
        int px, nx, py, ny, d; // 픽셀 변수들

        // 반경 내의 픽셀들을 투명하게 처리
        for (var i = 0; i < radius; i++)
        {
            d = Mathf.RoundToInt(Mathf.Sqrt(radius * radius - i * i));

            for (var j = 0; j < d; j++)
            {
                px = center.x + i;
                nx = center.x - i;
                py = center.y + j;
                ny = center.y - j;
                
                texture.SetPixel(px, py, Color.clear); // 랜더러도 갱신됨
                texture.SetPixel(nx, py, Color.clear);
                texture.SetPixel(px, ny, Color.clear);
                texture.SetPixel(nx, ny, Color.clear);
            }
        }
        
        texture.Apply(); // 바뀐 픽셀대로 텍스처를 바꿈
        SetSprite(); // 맵밖 텍스처 색깔 적용
        
        Destroy(gameObject.GetComponent<PolygonCollider2D>()); // 기존 폴리곤 콜라이더 삭제
        gameObject.AddComponent<PolygonCollider2D>(); // 새로운 폴리곤 콜라이더 추가
        
        EffectManager.IT.ActiveProjectileEffect(colliderBoundsCenter); // 포탄 이펙트 활성화
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        // 포톤 뷰가 내 것이 아니면 리턴
        if (!PV.IsMine) return;
        // 충돌한 콜라이더가 CircleCollider2D가 아니면 리턴
        if (!col.GetComponent<CircleCollider2D>()) return;
        
        MakeRange(col.GetComponent<CircleCollider2D>()); // MakeRange 호출
        
        // 반경 내에 있는 탱크에 거리에 따른 데미지 적용
        var attackRangeRatio = InGameManager.IT.isAttackRange ? 1.5f : 1.0f; // Range 아이템 사용 시 1.5배 증가
        
        InGameManager.IT.isAttackRange = false; // Range 아이템 사용 후 초기화
        
        var radius = col.bounds.size.x / 2 * attackRangeRatio; // 반경 (rangeRatio 적용)

        var colliders = Physics2D.OverlapCircleAll(col.bounds.center, radius); // 반경 내의 모든 콜라이더

        // 거리 변수 계산
        var distance = Vector3.Distance(col.bounds.center, InGameManager.IT.initialPosition);
        
        var maxDistance = 50; // 최대 거리 (임의로 50으로 설정)
        // ratio // distance = 0 -> 1, distance > maxDistance -> 0.5
        if (distance > maxDistance) distance = maxDistance;
        
        var distanceRatio = 1 - distance / maxDistance / 2; // 거리 변수

        foreach (var collider in colliders)
        {
            // 탱크에게 데미지 적용
            if (collider.CompareTag("Tank"))
            {
                var tank = collider.GetComponent<TankHandler>(); // 탱크 핸들러
                
                // 범위 변수 계산
                var range = Vector2.Distance(col.bounds.center, collider.bounds.center + new Vector3(0, 0.27f, 0));
                    range = Mathf.Round(range * 10) / 10; // 소수 첫째자리에서 반올림
                
                // ratio // range = 0 -> 1, range = radius -> 0.5
                if (radius < range)
                    range = radius;

                var rangeRatio = 1 - range / radius / 2; // 범위 변수

                
                // 데미지 증폭 아이템 유무
                var damageRatio = InGameManager.IT.isAttackDamage ? 1.5f : 1f;
                
                var damage = 100 * damageRatio * distanceRatio * rangeRatio; // 데미지 계산
                
                tank.Hit(damage); // 데미지 적용
            }
        }
    }

    private Vector2Int GetPixelPosition(Vector3 position)
    {
        var pixelPosition = Vector2Int.zero; // 픽셀 위치 초기화
        
        var dx = position.x - transform.position.x; // x 거리
        var dy = position.y - transform.position.y; // y 거리
        
        pixelPosition.x = Mathf.RoundToInt(dx * (pixelWidth / worldWidth) + pixelWidth / 2); // x 픽셀 위치 계산
        pixelPosition.y = Mathf.RoundToInt(dy * (pixelHeight / worldHeight) + pixelHeight / 2); // y 픽셀 위치 계산
        
        return pixelPosition;
    }
    private void SetSprite()
    {
        spriteRenderer.sprite = Sprite.Create(texture, 
            new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f); // 스프라이트 설정
    }
}
