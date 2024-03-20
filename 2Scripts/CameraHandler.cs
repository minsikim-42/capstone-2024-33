using System;
using System.Collections;
using Cinemachine;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    private CinemachineVirtualCamera camera; // 카메라 (Cinemachine)

    public Transform target; // 카메라 타겟
    public Vector3 position; // 카메라 위치
    public Vector3 offset; // 카메라 오프셋
    
    [SerializeField] private float zoomSpeed = 4f; // 줌 속도
    [SerializeField] private float minZoom = 5f; // 최소 줌
    [SerializeField] private float maxZoom = 15f; // 최대 줌
    
    private void Awake()
    {
        camera = GetComponent<CinemachineVirtualCamera>(); // 카메라 할당
    }
    
    public void Zoom(float value)
    {
        StartCoroutine(ZoomCoroutine(value)); // 줌 코루틴 시작
    }
    
    private IEnumerator ZoomCoroutine(float value)
    {
        // orthographicSize와 value의 차이가 0.01f보다 큰 동안 반복 
        while (Math.Abs(camera.m_Lens.OrthographicSize - value) > 0.01f)
        {
            camera.m_Lens.OrthographicSize = Mathf.MoveTowards(camera.m_Lens.OrthographicSize, value, zoomSpeed * Time.deltaTime); // 카메라의 orthographicSize를 value로 이동
            
            yield return null;
        }
    }
    
    public void SetTarget(Transform target)
    {
        this.target = target; // 타겟 설정
        
        camera.Follow = target; // 카메라의 Follow를 타겟으로 설정
    }
    
    public void SetOffset(Vector3 offset)
    {
        this.offset = offset; // 오프셋 설정
        
        camera.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX = offset.x; // 카메라의 ScreenX를 offset.x로 설정
        camera.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenY = offset.y; // 카메라의 ScreenY를 offset.y로 설정
    }
}