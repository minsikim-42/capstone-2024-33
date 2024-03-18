using BackEnd;
using UnityEngine;

public class BackendManager : MonoBehaviour
{
    public static BackendManager instance;

    public bool IsSuccess;
    
    private void Awake()
    {
        instance = this;
        
        DontDestroyOnLoad(gameObject);
        
        // Init Server
        InitServer();
    }

    private void Update()
    {
        // Polling Server
        if (Backend.IsInitialized) Backend.AsyncPoll();
    }

    private void InitServer()
    {
        // Init Server
        var bro = Backend.Initialize(true);
        
        // 서버 초기화 성공 여부 확인
        if (bro.IsSuccess())
        {
            Debug.Log("Success");
            IsSuccess = true;
        }
        else
        {
            Debug.Log($"Fail : {bro}");
            IsSuccess = false;
        }
    }
}
