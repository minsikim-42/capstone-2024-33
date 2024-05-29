using BackEnd;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SignManager : MonoBehaviour
{
    public static SignManager IT; // 싱글톤
    
    [SerializeField] private string testId; // 테스트 아이디
    [SerializeField] private string testPassword; // 테스트 비밀번호
    
    [SerializeField] private CanvasGroup signInCg; // 로그인 캔버스 그룹
    [SerializeField] private CanvasGroup signUpCg; // 회원가입 캔버스 그룹
    
    [SerializeField] private TMP_InputField signInId; // 로그인 아이디
    [SerializeField] private TMP_InputField signInPassword; // 로그인 비밀번호
    [SerializeField] private TextMeshProUGUI signInAlertText; // 로그인 경고 텍스트
    [SerializeField] private Button signInButton; // 로그인 버튼
    [SerializeField] private Button signUpButton; // 회원가입 버튼
    
    [SerializeField] private TMP_InputField signUpId; // 회원가입 아이디
    [SerializeField] private TMP_InputField signUpPassword; // 회원가입 비밀번호
    [SerializeField] private TextMeshProUGUI signUpAlertText; // 회원가입 경고 텍스트
    [SerializeField] private Button signUpCreateButton; // 회원가입 생성 버튼
    [SerializeField] private Button signUpBackButton; // 회원가입 뒤로 버튼
    
    private void Awake()
    {
        IT = this;
        
        // 버튼 이벤트 추가
        signInButton.onClick.AddListener(SignIn);
        signUpButton.onClick.AddListener(ShowSignUp);
        signUpCreateButton.onClick.AddListener(SignUp);
        signUpBackButton.onClick.AddListener(ShowSignIn);
    }

    public void CheckAutoLogin(bool isTestMode)
    {
        if (!isTestMode)
            return;
        
        signInId.text = testId; // 테스트 아이디 입력
        signInPassword.text = testPassword; // 테스트 비밀번호 입력
            
        SignIn(); // 로그인
    }

    public void ShowSignIn()
    {
        // Init
        signInId.text = "";
        signInPassword.text = "";
        signInAlertText.text = "";

        // 로그인 화면 보이기
        signInCg.alpha = 1;
        signInCg.blocksRaycasts = true;
        signInCg.interactable = true;
        
        // 회원가입 화면 숨기기
        signUpCg.alpha = 0;
        signUpCg.blocksRaycasts = false;
        signUpCg.interactable = false;
    }
    
    public void ShowSignUp()
    {
        // Init 
        signUpId.text = "";
        signUpPassword.text = "";
        signUpAlertText.text = "";
        
        // 로그인 화면 숨기기
        signInCg.alpha = 0;
        signInCg.blocksRaycasts = false;
        signInCg.interactable = false;
        
        // 회원가입 화면 보이기
        signUpCg.alpha = 1;
        signUpCg.blocksRaycasts = true;
        signUpCg.interactable = true;
    }

    private void SignIn()
    {
        var id = signInId.text; // 로그인 아이디
        var password = signInPassword.text; // 로그인 비밀번호

        // 아이디 또는 비밀번호가 입력되지 않았을 경우
        if (id.Trim().Equals(""))
        {
            signInAlertText.text = "아이디를 입력해주세요.";
            return;
        }
        
        if (password.Trim().Equals(""))
        {
            signInAlertText.text = "비밀번호를 입력해주세요.";
            return;
        }
        
        signInButton.interactable = false; // 로그인 버튼 비활성화
        
        // 로그인
        Backend.BMember.CustomLogin(id, password, callback =>
        {
            // 로그인 콜백
            if (callback.IsSuccess())
            {
                SceneManager.LoadScene("Lobby"); // 로비 씬으로 이동
            }
            else
            {
                // https://docs.thebackend.io/sdk-docs/backend/base/all-errors/
                // 위 링크에서 에러 코드 확인 가능
                // 서버 상태, 아이디 또는 비밀번호가 틀렸을 경우에 대한 에러 코드
                
                var code = int.Parse(callback.GetStatusCode());
                signInAlertText.text = $"다시 시도해주세요. {code}";
                
                
                signInButton.interactable = true; // 로그인 버튼 활성화
            }
        });
    }
    
    private void SignUp()
    {
        // 아이디 패스워드 변수
        var id = signUpId.text;
        var password = signUpPassword.text;
     
        // 아이디 또는 비밀번호가 입력되지 않았을 경우
        if (id.Trim().Equals(""))
        {
            signUpAlertText.text = "아이디를 입력해주세요.";
            return;
        }
        // 아이디가 10자리 초과일 경우
        if (id.Length > 10)
        {
            signUpAlertText.text = "아이디를 10자 이내로 입력해주세요.";
            return;
        }
        
        if (password.Trim().Equals(""))
        {
            signUpAlertText.text = "비밀번호를 입력해주세요.(4자리 이상)";
            return;
        }
        
        // 비밀번호가 4자리 미만일 경우
        if (password.Length < 4)
        {
            signUpAlertText.text = "비밀번호를 4자리 이상 입력해주세요.";
            return;
        }
        
        signUpCreateButton.interactable = false; // 회원가입 생성 버튼 비활성화
        signUpBackButton.interactable = false; // 회원가입 뒤로 버튼 비활성화
        
        // 회원가입
        Backend.BMember.CustomSignUp(id, password, callback =>
        {
            signUpCreateButton.interactable = true; 
            signUpBackButton.interactable = true;
            
            // 회원가입 콜백
            if (callback.IsSuccess())
            {
                ShowSignIn(); // 로그인 화면 보이기
            }
            else
            {
                signUpAlertText.text = "회원가입에 실패했습니다.";
            }
        });
    }
}
