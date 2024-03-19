using System;
using UnityEngine;
using BackEnd;
using UnityEngine.Events;

public class UserInfo : MonoBehaviour
{
    [Serializable]
    public class UserInfoEvent : UnityEvent { }
    public UserInfoEvent onUserInfoEvent = new UserInfoEvent(); // 유저 정보 이벤트
    
    private static UserInfoData data = new UserInfoData(); // 유저 정보 데이터
    public static UserInfoData Data => data; // 유저 정보 데이터 Getter

    public void GetUserInfoFromBackend()
    {
        // 유저 정보 가져오기
        Backend.BMember.GetUserInfo(callback =>
        {
            // 콜백이 성공했을 경우
            if (callback.IsSuccess())
            {
                try
                {
                    // 콜백 json 데이터를 UserInfoData에 저장
                    
                    var json = callback.GetReturnValuetoJSON()["row"];
                
                    data.gamerId = json["gamerId"].ToString();
                    data.countryCode = json["countryCode"]?.ToString();
                    data.nickname = json["nickname"]?.ToString();
                    data.inDate = json["inDate"].ToString();
                    data.emailForFindPassword = json["emailForFindPassword"]?.ToString();
                    data.subscriptionType = json["subscriptionType"].ToString();
                    data.federationId = json["federationId"]?.ToString();
                }
                catch (Exception e)
                {
                    // 예외 발생 시 데이터 초기화
                    
                    data.Reset();
                    
                    Debug.LogError(e);
                }
            }
            // 콜백이 실패했을 경우
            else
            {
                data.Reset(); // 데이터 초기화
                
                Debug.LogError(callback.GetMessage());
            }
            
            onUserInfoEvent?.Invoke(); // 유저 정보 이벤트 호출
        });
    }
}

public class UserInfoData
{
    public string gamerId;
    public string countryCode;
    public string nickname;
    public string inDate;
    public string emailForFindPassword;
    public string subscriptionType;
    public string federationId;

    public void Reset()
    {
        gamerId = "Offline";
        countryCode = "Unknown";
        nickname = "Noname";
        inDate = string.Empty;
        emailForFindPassword = string.Empty;
        subscriptionType = string.Empty;
        federationId = string.Empty;
    }
}
