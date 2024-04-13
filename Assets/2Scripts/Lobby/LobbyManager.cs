using System;
using System.Collections.Generic;
using BackEnd;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager IT;

    [SerializeField] private UserInfo user; // 유저 정보
    
    [Header("Loading")]
    [SerializeField] private CanvasGroup loadingCg; // 로딩 캔버스 그룹
    
    [Header("Nickname")]
    [SerializeField] private TextMeshProUGUI nicknameText; // 닉네임 텍스트
    [SerializeField] private Button nicknameButton; // 닉네임 버튼
    [SerializeField] private CanvasGroup nicknameCg; // 닉네임 캔버스 그룹
    [SerializeField] private TMP_InputField nicknameInputField; // 닉네임 입력 필드
    [SerializeField] private TextMeshProUGUI nicknameAlertText; // 닉네임 경고 텍스트
    [SerializeField] private Button nicknameSaveButton; // 닉네임 저장 버튼
    [SerializeField] private Button nicknameBackButton; // 닉네임 뒤로가기 버튼

    [Serializable]
    public class NicknameEvent : UnityEvent { }
    public NicknameEvent onNicknameEvent = new NicknameEvent(); // 닉네임 이벤트

    [SerializeField] TextMeshProUGUI titleText;
    
    [Header("Create Room")]
    [SerializeField] private Button createRoomButton; // 방 생성 버튼
    [SerializeField] private CanvasGroup createRoomCg; // 방 생성 캔버스 그룹
    [SerializeField] private TMP_InputField createRoomInputField; // 방 생성 입력 필드
    [SerializeField] private TextMeshProUGUI createRoomAlertText; // 방 생성 경고 텍스트
    [SerializeField] private Button createRoomCreateButton; // 방 생성 버튼
    [SerializeField] private Button isTeamButtonTrue; // 팀전 선택 버튼
    [SerializeField] private Button isTeamButtonFalse; // 팀전 선택 버튼
    public bool isTeamMode;
    [SerializeField] private Button createRoomBackButton; // 방 생성 뒤로가기 버튼
    [SerializeField] private CanvasGroup createRoomButtonCg; // 방 생성 버튼 캔버스 그룹
    
    [Header("Room List")]
    [SerializeField] private CanvasGroup roomListCg; // 방 리스트 캔버스 그룹
    [SerializeField] private Button leftButton; // 왼쪽 버튼
    [SerializeField] private Button rightButton; // 오른쪽 버튼
    [SerializeField] private TextMeshProUGUI roomPageText; // 방 페이지 텍스트
    [SerializeField] private CanvasGroup noRoomCg; // 방 없음 캔버스 그룹

    [Header("Room")]
    [SerializeField] private CanvasGroup roomCg; // 방 캔버스 그룹
    [SerializeField] private Transform roomParent; // 방 부모
    [SerializeField] private RoomHandler roomPrefab; // 방 프리팹
    private List<RoomHandler> roomList = new List<RoomHandler>(); // 방 리스트
    [SerializeField] private Button leftRoomButton; // 방 뒤로가기 버튼
    [SerializeField] private Button gameStartButton; // 게임 시작 버튼
	[SerializeField] private Button changeTeamButton; // 팀 변경 버튼

    [Header("Room Player List")] 
    [SerializeField] private List<RoomPlayerSlotHandler> roomPlayerSlotHandlers; // 방 플레이어 슬롯 핸들러들
    
    [Header("Result")]
    [SerializeField] private CanvasGroup resultCanvasGroup; // 결과 캔버스 그룹
    [SerializeField] private TextMeshProUGUI resultText; // result text
    [SerializeField] private Button goToLobbyButton; // 로비로 가는 버튼
    [SerializeField] private List<ResultSlotHandler> resultSlots; // 결과 표시 슬롯들

    private void Awake()
    {
        IT = this;
        
        // 버튼 클릭 이벤트 추가
        nicknameButton.onClick.AddListener(ShowNickname);
        nicknameSaveButton.onClick.AddListener(SaveNickname);
        nicknameBackButton.onClick.AddListener(BackNickname);
        
        createRoomCg.alpha = 0;
        createRoomButton.onClick.AddListener(ShowCreateRoom);
        createRoomCreateButton.onClick.AddListener(CreateRoom);
        isTeamMode = false;
        isTeamButtonFalse.onClick.AddListener(SetIsTeamMode);
        isTeamButtonTrue.onClick.AddListener(SetIsTeamMode);
        isTeamButtonTrue.gameObject.SetActive(false);
        createRoomBackButton.onClick.AddListener(BackCreateRoom);
        
        leftButton.onClick.AddListener(LeftRoomList);
        rightButton.onClick.AddListener(RightRoomList);
        
        leftRoomButton.onClick.AddListener(LeftRoom);
        
        gameStartButton.onClick.AddListener(GameStart);
        changeTeamButton.onClick.AddListener(ChangeTeam);
        goToLobbyButton.onClick.AddListener(GoToLobby);
        
        user.GetUserInfoFromBackend(); // 유저 정보를 가져옴
        
        GenerateRoom10(); // 방 10개 생성 (풀)
    }

    private void Start()
    {
        // 결과창을 보여줘여하는지 확인
        if (GameManager.IT.isResult)
        {
            GameManager.IT.isResult = false; // 결과창 표시 여부를 false로 변경
            
            ShowResult(GameManager.IT.result); // 결과창 표시
        }
        else
            HideResult(); // 결과창 숨기기
        
        HideNoRoom(); // 방 없음 창 숨기기
        ShowLoading(); // 로딩창 보이기
        ShowRoomList(); // 방 리스트 보이기
        HideRoom(); // 방 숨기기
    }

    public void CheckTestMode(bool isTestMode, bool testCreateRoom, bool testJoinRoom)
    {
        if (isTestMode)
        {
            if (testCreateRoom)
            {
                createRoomInputField.text = "Test Room";
                CreateRoom();
            }
            else
            {
                if (testJoinRoom)
                {
                    NetworkManager.IT.JoinRoom("Test Room");
                }
            }
        }
    }
    
    private void GenerateRoom10()
    {
        for (int i = 0; i < 10; i++)
        {
            RoomHandler room = Instantiate(roomPrefab, roomParent); // 방 생성
            roomList.Add(room); // 방 리스트에 추가
        }
    }
    
    // Loading 창 보이기
    public void ShowLoading()
    {
        loadingCg.alpha = 1;
        loadingCg.blocksRaycasts = true;
        loadingCg.interactable = true;
    }
    // Loading 창 숨기기
    public void HideLoading()
    {
        if(loadingCg == null) 
            return;
        loadingCg.alpha = 0;
        loadingCg.blocksRaycasts = false;
        loadingCg.interactable = false;
    }
    // Room List 보이기
    private void ShowRoomList()
    {
        roomListCg.alpha = 1;
        roomListCg.blocksRaycasts = true;
        roomListCg.interactable = true;
    }
    // Room List 숨기기
    public void HideRoomList()
    {
        roomListCg.alpha = 0;
        roomListCg.blocksRaycasts = false;
        roomListCg.interactable = false;
    }
    // Room 보이기
    public void ShowRoom()
    {
        roomCg.alpha = 1;
        roomCg.blocksRaycasts = true;
        roomCg.interactable = true;
        
        createRoomButtonCg.alpha = 0;
        createRoomButtonCg.blocksRaycasts = false;
        createRoomButtonCg.interactable = false;

        titleText.SetText(PhotonNetwork.CurrentRoom.Name);

        if (isTeamMode == false) 
            changeTeamButton.gameObject.SetActive(false);
        else
            changeTeamButton.gameObject.SetActive(true);
    }
    // Room 숨기기
    public void HideRoom()
    {
        roomCg.alpha = 0;
        roomCg.blocksRaycasts = false;
        roomCg.interactable = false;
        
        createRoomButtonCg.alpha = 1;
        createRoomButtonCg.blocksRaycasts = true;
        createRoomButtonCg.interactable = true;

        titleText.SetText("Lobby");
    }
    
    
    // Nickname
    public void CheckNickname()
    {
        // 닉네임 할당 여부 확인
        if (UserInfo.Data.nickname == null)
        {
            ShowNickname(); // 닉네임 보이기
        }
    }
    // 닉네임 업데이트
    public void UpdateNickname()
    {
        nicknameText.text = UserInfo.Data.nickname == null ? "" : UserInfo.Data.nickname;
    }
    // 닉네임 창 보이기
    private void ShowNickname()
    {
        if (roomCg.alpha == 1)
            return;
        
        // Init
        nicknameInputField.text = "";
        nicknameAlertText.text = "";
        
        nicknameCg.alpha = 1;
        nicknameCg.blocksRaycasts = true;
        nicknameCg.interactable = true;
    }
    // 닉네임 저장
    private void SaveNickname()
    {
        // 닉네임 입력 확인
        if (nicknameInputField.text.Trim().Equals(""))
        {
            nicknameAlertText.text = "닉네임을 입력해주세요.";
            return;
        }

        // 닉네임 길이 확인
        if (nicknameInputField.text.Trim().Length is > 10 or < 2)
        {
            nicknameAlertText.text = "닉네임은 2~10자 이내로 입력해주세요.";
            return;
        }
        
        nicknameSaveButton.interactable = false; // 닉네임 저장 버튼 비활성화
        nicknameBackButton.interactable = false; // 닉네임 뒤로가기 버튼 비활성화

        // 닉네임 업데이트
        Backend.BMember.UpdateNickname(nicknameInputField.text, callback =>
        {
            nicknameSaveButton.interactable = true; // 닉네임 저장 버튼 활성화
            nicknameBackButton.interactable = true; // 닉네임 뒤로가기 버튼 활성화
            
            // 닉네임 저장 성공 여부 확인
            if (callback.IsSuccess())
            {
                nicknameCg.alpha = 0;
                nicknameCg.blocksRaycasts = false;
                nicknameCg.interactable = false;
                
                // Set Nickname
                PhotonNetwork.NickName = nicknameInputField.text;
                
                onNicknameEvent?.Invoke();
            }
            else
            {
                nicknameAlertText.text = "닉네임을 변경하는데 실패했습니다. 다시 시도해주세요.";
            }
        });
    }
    // 닉네임 창 뒤로가기
    private void BackNickname()
    {
        if (UserInfo.Data.nickname == null)
        {
            nicknameAlertText.text = "닉네임을 입력해주세요.";
            
            return;
        }

        nicknameCg.alpha = 0;
        nicknameCg.blocksRaycasts = false;
        nicknameCg.interactable = false;
    }
    

    // Room List
    // 방 없음 창 보이기
    public void ShowNoRoom()
    {
        noRoomCg.alpha = 1;
        noRoomCg.blocksRaycasts = true;
        noRoomCg.interactable = true;

        // 방 리스트 숨기기
        foreach (var room in roomList)
        {
            room.Inactive();
        }
    }
    // 방 없음 창 숨기기
    public void HideNoRoom()
    {
        noRoomCg.alpha = 0;
        noRoomCg.blocksRaycasts = false;
        noRoomCg.interactable = false;
    }
    // 방 리스트 설정
    public void SetRoomList(List<RoomInfo> roomInfos)
    {
        // 방 리스트 숨기기
        foreach (var room in roomList)
        {
            room.Inactive();
        }
        
        // 방 리스트 설정
        foreach (RoomInfo room in roomInfos)
        {
            // 방이 비어있는 경우
            if (room.PlayerCount == 0)
                continue;
            
            // 방이 있는 경우 프로퍼티 설정
            var properties = room.CustomProperties;
            
            // AI 카운트 설정
            var aiCount = 0;
            
            if ((int)properties["Slot1"] == 99)
                aiCount++;
            if ((int)properties["Slot2"] == 99)
                aiCount++;
            if ((int)properties["Slot3"] == 99)
                aiCount++;
            if ((int)properties["Slot4"] == 99)
                aiCount++;
            if ((int)properties["Slot5"] == 99)
                aiCount++;
            if ((int)properties["Slot6"] == 99)
                aiCount++;
            if ((int)properties["Slot7"] == 99)
                aiCount++;

            // 방 리스트에서 방 찾아서 활성화
            if ((bool)properties["isTeamMode"] == true) {
                roomList[roomInfos.IndexOf(room)].transform.GetChild(0).GetComponent<Image>().color = new Color(.8f, .4f, .8f);
            } else {
                roomList[roomInfos.IndexOf(room)].transform.GetChild(0).GetComponent<Image>().color = new Color(.7f, .7f, .7f);
            }
            roomList[roomInfos.IndexOf(room)].Active(room.Name, room.PlayerCount + aiCount);
        }
    }

    // 방 생성 창 보이기
    private void ShowCreateRoom()
    {
        // Init
        createRoomInputField.text = "";
        createRoomAlertText.text = "";
        
        createRoomCg.alpha = 1;
        createRoomCg.blocksRaycasts = true;
        createRoomCg.interactable = true;
    }
    
    private void CreateRoom()
    {
        ShowLoading(); // 로딩창 보이기

        // 방 이름을 입력하지 않았으면
        if (createRoomInputField.text.Trim().Equals(""))
        {
            createRoomAlertText.text = "방 이름을 입력해주세요.";
            
            HideLoading(); // 로딩창 숨기기
            return;
        }
        
        // 방 이름 중복 확인
        if (NetworkManager.IT.CheckRoomName(createRoomInputField.text))
        {
            createRoomAlertText.text = "중복된 방 이름입니다.";
            
            HideLoading();
            return;
        }
        
        createRoomCg.alpha = 0;
        createRoomCg.blocksRaycasts = false;
        createRoomCg.interactable = false;

        NetworkManager.IT.CreateRoom(createRoomInputField.text); // 방 생성
    }

    private void SetIsTeamMode()
    {
        if (isTeamMode == false) {
            isTeamMode = true;
            isTeamButtonTrue.gameObject.SetActive(true);
            isTeamButtonFalse.gameObject.SetActive(false);
        }
        else {
            isTeamMode = false;
            isTeamButtonTrue.gameObject.SetActive(false);
            isTeamButtonFalse.gameObject.SetActive(true);
        }
    }

    public void SetRoom()
    {
        HideRoomList(); // 방 리스트 숨기기
        HideNoRoom(); // 방 없음 창 숨기기
        ShowRoom(); // 방 보이기
    }

    public void SetSlotEmpty(int slotIndex)
    {
        roomPlayerSlotHandlers[slotIndex].SetEmptySlot(); // 슬롯을 빈 슬롯으로 변경
    }
    
    public void SetSlotAI(int slotIndex)
    {
        roomPlayerSlotHandlers[slotIndex].SetSlotAI(); // 슬롯을 AI 슬롯으로 변경
    }
    
    public void SetSlot(int slotIndex, string nickname)
    {
        roomPlayerSlotHandlers[slotIndex].SetPlayerNickname(nickname); // 슬롯을 플레이어 슬롯으로 변경
    }

    public void SetTeamColor(int slotIndex, int teamNum) {
        if (isTeamMode == false) {
            roomPlayerSlotHandlers[slotIndex].GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f);
            return ;
        }
        // roomPlayerSlotHandlers[slotIndex].SetTeamColor(teamNum);
        GameObject slotHand = roomPlayerSlotHandlers[slotIndex].gameObject;
        if (teamNum == 1)
            roomPlayerSlotHandlers[slotIndex].GetComponent<Image>().color = new Color(.9f, .2f, .2f, 0.9f); // Red
        else if (teamNum == 2)
            roomPlayerSlotHandlers[slotIndex].GetComponent<Image>().color = new Color(.2f, .2f, .9f, 0.9f); // Blue
        else
            roomPlayerSlotHandlers[slotIndex].GetComponent<Image>().color = new Color(.3f, .3f, .3f); // Gray
    }

    public void SetIsPlayerColor(int slotIndex, bool isPlayer) {
        if (isPlayer == true)
            roomPlayerSlotHandlers[slotIndex].transform.GetChild(0).GetComponent<Image>().color = new Color(0.9f, 0.9f, 0.9f);
        else
            roomPlayerSlotHandlers[slotIndex].transform.GetChild(0).GetComponent<Image>().color = new Color(0.6f, 0.6f, 0.6f);
    }
    
    // 방 생성 창 숨기기
    private void BackCreateRoom()
    {
        createRoomCg.alpha = 0;
        createRoomCg.blocksRaycasts = false;
        createRoomCg.interactable = false;
    }
    
    
    // 사용 X
    // 왼쪽 방 리스트 보이기
    private void LeftRoomList()
    {
        Debug.LogWarning("Left Room List");
    }
    // 사용 X
    // 오른쪽 방 리스트 보이기
    private void RightRoomList()
    {
        Debug.LogWarning("Right Room List");
    }


    // 방에서 나가기
    public void LeftRoom()
    {
        ShowLoading(); // 로딩창 보이기
        HideRoom(); // 방 숨기기
        ShowRoomList(); // 방 리스트 보이기
        NetworkManager.IT.LeftRoom(); // 방에서 나가기
    }

    public void ChangeTeam()
    {
        NetworkManager.IT.SetChangeTeam();
    }

    // 게임 시작 버튼 보이기 (마스터 클라이언트만)
    public void SetGameStart(bool isMasterClient)
    {
        gameStartButton.gameObject.SetActive(isMasterClient); // 게임 시작 버튼 보이기
    }
    private void GameStart()
    {
        if (NetworkManager.IT.GetPlayerNum() < 1) { // 방장을 제외한 인원수
            return ;
        }

        ShowLoading(); // 로딩창 보이기
        
        NetworkManager.IT.GameStart(); // 게임 시작
    }

    
    #region Result
    public void HideResult()
    {
        resultCanvasGroup.alpha = 0; // 결과 캔버스 그룹의 알파값을 0으로 변경
        resultCanvasGroup.blocksRaycasts = false; // 결과 캔버스 그룹의 블록 레이캐스트를 false로 변경
        resultCanvasGroup.interactable = false; // 결과 캔버스 그룹의 인터렉터블을 false로 변경
    }
    
    public void ShowResult(string data)
    {
		resultCanvasGroup.alpha = 1; // 결과 캔버스 그룹의 알파값을 0으로 변경
        resultCanvasGroup.blocksRaycasts = true; // 결과 캔버스 그룹의 블록 레이캐스트를 false로 변경
        resultCanvasGroup.interactable = true; // 결과 캔버스 그룹의 인터렉터블을 false로 변경

        resultText.SetText(GameManager.IT.resultText);

        var dataList = data.Split(','); // 결과 데이터를 /로 나누어 배열로 저장
        
        for (int i = 0; i < dataList.Length; i++)
        {
            var slot = resultSlots[i]; // 결과 슬롯
            var resultData = dataList[i].Split('/'); // 결과 데이터를 /로 나누어 배열로 저장
            var nickname = resultData[0]; // 닉네임
            var damage = resultData[1]; // 데미지
            var teamNum = resultData[2]; // 팀넘버 (0,1,2)
            
            slot.SetResultSlot(nickname, damage, teamNum); // 슬롯에 닉네임과 데미지를 표시

			var c = teamNum switch
			{
				// 개인전
				"0" => new Color(0.7f, 0.7f, 0.7f, 0.8f),
				// 레드
				"1" => new Color(0.9f, 0.2f, 0.2f, 0.8f),
				// 블루
				"2" => new Color(0.2f, 0.2f, 0.9f, 0.8f),
				_ => Color.magenta,
			};
			slot.GetComponent<Image>().color = c;
        }
        
        resultCanvasGroup.alpha = 1; // 결과 캔버스 그룹의 알파값을 1로 변경
        resultCanvasGroup.blocksRaycasts = true; // 결과 캔버스 그룹의 블록 레이캐스트를 true로 변경
        resultCanvasGroup.interactable = true; // 결과 캔버스 그룹의 인터렉터블을 true로 변경
    }
    
    private void GoToLobby()
    {
        HideResult(); // 결과창 숨기기
    }
    #endregion
}
