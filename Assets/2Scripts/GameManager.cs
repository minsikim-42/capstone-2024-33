using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager IT;

    [Header("Loading")]
    [SerializeField] private LoadingSlider loadingSlider; // 로딩 슬라이더
    public float loadingTime = 3f; // 로딩 시간

    public bool isResult = false; // 결과창 표시 여부
    public string result = string.Empty; // 결과창 표시 내용
    
    [Header("Test Mode")]
    [SerializeField] private bool isTestMode; // 테스트 모드 여부
    [SerializeField] private bool testCreateRoom; // 테스트 모드에서 방 생성 여부
    [SerializeField] private bool testJoinRoom; // 테스트 모드에서 방 입장 여부
    
    private void Awake()
    {
        if (IT == null)
            IT = this;
        else
            Destroy(gameObject);
        
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        loadingSlider.LoadingStart(); // 로딩 시작
        
        SignManager.IT.CheckAutoLogin(isTestMode); // 자동 로그인 여부 확인
    }

    public void CheckTestMode()
    {
        LobbyManager.IT.CheckTestMode(isTestMode, testCreateRoom, testJoinRoom); // 테스트 모드에 따라 방 생성 및 입장
    }

    public bool IsTestMode()
    {
        return isTestMode;
    }
}
