using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingSlider : MonoBehaviour
{
    private Slider slider; // 로딩 슬라이더
    [SerializeField] private TextMeshProUGUI alertText; // 경고 텍스트
    [SerializeField] private TextMeshProUGUI titleText; // 타이틀 텍스트
    [SerializeField] private TextMeshProUGUI titleShadowText; // 타이틀 그림자 텍스트
    
    private void Awake()
    {
        slider = GetComponent<Slider>();
    }
    
    public void LoadingStart()
    {
        StartCoroutine(Loading()); // 로딩 코루틴 시작
    }
    
    private IEnumerator Loading()
    {
        // 초기화
        titleText.alpha = 0;
        titleShadowText.alpha = 0;
        
        var time = 0f;
        var timeToLoadDefault = GameManager.instance.loadingTime; // 로딩 시간
        var timeToLoadAfterChecking = .5f;
        
        
        // 로딩 슬라이더, 타이틀 텍스트, 타이틀 그림자 텍스트 알파값 변경
        while (time < timeToLoadDefault)
        {
            time += Time.deltaTime;
            slider.value = time / timeToLoadDefault * 0.9f;
            titleText.alpha = time / timeToLoadDefault;
            titleShadowText.alpha = time / timeToLoadDefault;
            yield return null;
        }
        
        slider.value = 0.9f; // 로딩 슬라이더 값 변경
        
        time = 0f; // 초기화
        
        // Check Backend Server Connection
        if (BackendManager.instance.IsSuccess)
        {
            // 로딩 슬라이더 값 변경
            while (time < timeToLoadAfterChecking)
            {
                time += Time.deltaTime;
                slider.value = 0.9f + (time) / timeToLoadAfterChecking * 0.1f; 
                yield return null;
            }

            SignManager.instance.ShowSignIn(); // 로그인 화면 보이기

            gameObject.SetActive(false); // 로딩 슬라이더 비활성화
        }
        else
        {
            alertText.text = "서버 연결에 실패했습니다.";
        }
    }
}
