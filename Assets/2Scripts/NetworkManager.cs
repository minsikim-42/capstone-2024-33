using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using TMPro;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager IT;
    
    private PhotonView PV; // PhotonView
    private readonly string gameVersion = "1"; // 게임 버전
    
    private List<RoomInfo> allRoomList; // 모든 방 리스트

    public bool isStarted; // PhotonNetwork.CurrentRoom.CustomProperties
    
    private void Awake()
    {
        if (IT == null)
        {
            IT = this;
        
            DontDestroyOnLoad(gameObject);

            // PhotonView 할당
            if (PV == null)
            {
                // PhotonView 컴포넌트 추가, 옵션 설정 
                PV = gameObject.AddComponent<PhotonView>();
                PV.OwnershipTransfer = OwnershipOption.Fixed;
                PV.Synchronization = ViewSynchronization.UnreliableOnChange;
                PV.observableSearch = PhotonView.ObservableSearch.AutoFindAll;
            
                PhotonNetwork.GameVersion = gameVersion;
            
                Connect(); // 서버 연결
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        allRoomList = new List<RoomInfo>(); // 모든 방 리스트 초기화

        isStarted = false;
    }

    public void SendViewId(int viewId)
    {
        PV.RPC(nameof(RPC_SendViewId), RpcTarget.MasterClient, PhotonNetwork.NickName, viewId);
    }
    
    [PunRPC]
    private void RPC_SendViewId(string nickname, int viewId)
    {
        Debug.Log($"Nickname : {nickname}, ViewId : {viewId}");
    }
    
    private void Connect()
    {
        PhotonNetwork.ConnectUsingSettings(); // 서버 연결
    }
    
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        
        // Set Nickname
        PhotonNetwork.NickName = UserInfo.Data.nickname;
        
        // Join Lobby
        JoinLobby();
    }
    
    public override void OnDisconnected(DisconnectCause cause)
    {
        Connect(); // 서버 연결
    }

    
    private void JoinLobby()
    {
        PhotonNetwork.JoinLobby(); // 로비 입장
    }
    
    public override void OnJoinedLobby()
    {
        GameManager.IT.CheckTestMode(); // 테스트 모드 확인
    }
    
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // roomList가 null이면
        if (roomList == null)
        {
            print("roomList is null");
        }
        else
        {
            var startedRoomList = new List<RoomInfo>(); // 시작된 방 리스트
            
            foreach (var room in roomList)
            {
                // 시작된 방 리스트에 추가
                if (room.CustomProperties != null && room.CustomProperties.ContainsKey(IsStarted) && room.CustomProperties[IsStarted] is bool && (bool)room.CustomProperties[IsStarted])
                {
                    startedRoomList.Add(room);
                }
            }

            // room info
            foreach (var room in roomList)
            {
                // allRoomList에서 room.Name과 같은 이름의 RoomInfo가 있는지 확인
                var index = allRoomList.FindIndex(x => x.Name == room.Name);
            
                // 없으면 추가
                if (index == -1)
                {
                    // PlayerCount가 0이 아닌 방만 추가
                    if (room.PlayerCount != 0)
                        allRoomList.Add(room);
                }
                // 있으면 업데이트
                else
                {
                    allRoomList[index] = room; 
                }
            }
        
            // started room은 제외
            foreach (var room in startedRoomList)
            {
                allRoomList.Remove(room);
            }
        
        
            if (allRoomList.Count == 0)
            {
                LobbyManager.IT.ShowNoRoom(); // 방이 없을 때 No room UI 표시
            }
            else
            {
                LobbyManager.IT.HideNoRoom(); // No room UI 숨기기
            }
        
            LobbyManager.IT.SetRoomList(allRoomList); // 방 리스트 설정
        
            Invoke(nameof(DelayHideLoading), 1f); // 로딩 숨기기
        }
        
    }
    
    private void DelayHideLoading()
    {
        LobbyManager.IT.HideLoading();
    }

    public bool CheckRoomName(string roomName)
    {
        return allRoomList.Any(x => x.Name == roomName && x.PlayerCount != 0); // 방 이름 확인
    }
    
    // 프로퍼티 용 string 변수 
    const string IsStarted = "IsStarted";
    const string IsTeamMode = "isTeamMode";
    const string Slot0 = "Slot0";
    const string Slot1 = "Slot1";
    const string Slot2 = "Slot2";
    const string Slot3 = "Slot3";
    const string Slot4 = "Slot4";
    const string Slot5 = "Slot5";
    const string Slot6 = "Slot6";
    const string Slot7 = "Slot7";
    
    [SerializeField] public List<Slot> gameForSlots; // 슬롯 리스트
    public void CreateRoom(string roomName)
    {
        var player = PhotonNetwork.LocalPlayer;
        // 방 설정 setRoomProperties
        if (LobbyManager.IT.isTeamMode)
            LobbyManager.IT.InitSlot(1);
        else
            LobbyManager.IT.InitSlot(0);

        var s = LobbyManager.IT.roomPlayerSlots[0];
        Debug.Log("NetM: actNum: " + s.actorNumber + "actName: " + s.nickName);
        
        var roomOptions = new RoomOptions
        {
            CustomRoomPropertiesForLobby = new[] {IsStarted, IsTeamMode, "GameMode", Slot0, Slot1, Slot2, Slot3, Slot4, Slot5, Slot6, Slot7, "team"+Slot1, "team"+Slot2, "team"+Slot3, "team"+Slot4, "team"+Slot5, "team"+Slot6, "team"+Slot7,"name"+Slot1, "name"+Slot2, "name"+Slot3, "name"+Slot4, "name"+Slot5, "name"+Slot6, "name"+Slot7},
            CustomRoomProperties = new Hashtable
            {
                {IsStarted, false}, {IsTeamMode, LobbyManager.IT.isTeamMode}, {"GameMode", 1},
                {Slot0, 1}, {Slot1, -1}, {Slot2, -1}, {Slot3, -1}, {Slot4, -1}, {Slot5, -1}, {Slot6, -1}, {Slot7, -1},
                {"team"+Slot0, LobbyManager.IT.roomPlayerSlots[0].teamNumber}, {"team"+Slot1, 0}, {"team"+Slot2, 0}, {"team"+Slot3, 0}, {"team"+Slot4, 0}, {"team"+Slot5, 0}, {"team"+Slot6, 0}, {"team"+Slot7, 0},
                {"name"+Slot0, player.NickName}, {"name"+Slot1, string.Empty}, {"name"+Slot2, string.Empty}, {"name"+Slot3, string.Empty}, {"name"+Slot4, string.Empty}, {"name"+Slot5, string.Empty}, {"name"+Slot6, string.Empty}, {"name"+Slot7, string.Empty}
            },
            MaxPlayers = 8
        };
        gameForSlots = new List<Slot>
        {
            new(Slot0, 1, player.NickName, LobbyManager.IT.roomPlayerSlots[0].teamNumber),
            new(Slot1, -1, string.Empty, 0),
            new(Slot2, -1, string.Empty, 0),
            new(Slot3, -1, string.Empty, 0),
            new(Slot4, -1, string.Empty, 0),
            new(Slot5, -1, string.Empty, 0),
            new(Slot6, -1, string.Empty, 0),
            new(Slot7, -1, string.Empty, 0)
        };

        PhotonNetwork.CreateRoom($"{roomName}", roomOptions); // 방 생성
    }
    
    
    public override void OnCreatedRoom()
    {
        LobbyManager.IT.SetRoom(); // 방 설정
        LobbyManager.IT.SetGameStart(PhotonNetwork.IsMasterClient); // 게임 시작 버튼 설정

        List<RoomPlayerSlotHandler> slotList = LobbyManager.IT.GetAllSlots();


        List<int> slotLog = new ();
        for (var i=0; i<8; i++)
        {
            slotLog.Add(slotList[i].actorNumber);
            if (slotList[i].actorNumber == -1) {
                // LobbyManager.IT.SetSlotEmpty(i); // 빈 슬롯
                LobbyManager.IT.SetIsPlayerColor(i, false);
            }
            else if (slotList[i].actorNumber == 99) {
                // LobbyManager.IT.SetSlotAI(i); // AI 슬롯
                LobbyManager.IT.SetIsPlayerColor(i, true);
            }
            else {
                // var p = PhotonNetwork.PlayerList.FirstOrDefault(a => a.ActorNumber == slotList[i].actorNumber);
                // LobbyManager.IT.SetSlotPlayer(i, p.NickName); // 플레이어 슬롯
                LobbyManager.IT.SetIsPlayerColor(i, true);
            }

            LobbyManager.IT.SetTeamColor(i, slotList[i].teamNumber);
        }
        LobbyManager.IT.SetSlotPlayer(0, PhotonNetwork.MasterClient.NickName); // 방장 슬롯
        Debug.Log(slotLog);

        var modeText = LobbyManager.IT.esayHardButton.GetComponentInChildren<TextMeshProUGUI>();
        modeText.SetText("Easy");
        LobbyManager.IT.HideLoading(); // 로딩 숨기기
    }

    public int GetPlayerNum() {
        int num=0;
        for (int i=1; i<8; i++) { // 방장을 제외한 인원 수
            if (LobbyManager.IT.roomPlayerSlots[i].actorNumber != -1)
                num++;
        }

        return num;
    }
    
    public void JoinRoom(string roomName)
    {
        LobbyManager.IT.ShowLoading(); // 로딩 표시
        
        PhotonNetwork.JoinRoom(roomName); // 방 입장
    }
    
    public override void OnJoinedRoom()
    {
        // 방에 입장한 상태
        if (PhotonNetwork.InRoom)
        {
            Debug.Log("joined room~");
            // hashtable 받아오기
            Hashtable CustomRoomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
            
            LobbyManager.IT.SetSlot(0, Slot0, (int)CustomRoomProperties[Slot0], (string)CustomRoomProperties["name"+Slot0], (int)CustomRoomProperties["team"+Slot0]);
            LobbyManager.IT.SetSlot(1, Slot1, (int)CustomRoomProperties[Slot1], (string)CustomRoomProperties["name"+Slot1], (int)CustomRoomProperties["team"+Slot1]);
            LobbyManager.IT.SetSlot(2, Slot2, (int)CustomRoomProperties[Slot2], (string)CustomRoomProperties["name"+Slot2], (int)CustomRoomProperties["team"+Slot2]);
            LobbyManager.IT.SetSlot(3, Slot3, (int)CustomRoomProperties[Slot3], (string)CustomRoomProperties["name"+Slot3], (int)CustomRoomProperties["team"+Slot3]);
            LobbyManager.IT.SetSlot(4, Slot4, (int)CustomRoomProperties[Slot4], (string)CustomRoomProperties["name"+Slot4], (int)CustomRoomProperties["team"+Slot4]);
            LobbyManager.IT.SetSlot(5, Slot5, (int)CustomRoomProperties[Slot5], (string)CustomRoomProperties["name"+Slot5], (int)CustomRoomProperties["team"+Slot5]);
            LobbyManager.IT.SetSlot(6, Slot6, (int)CustomRoomProperties[Slot6], (string)CustomRoomProperties["name"+Slot6], (int)CustomRoomProperties["team"+Slot6]);
            LobbyManager.IT.SetSlot(7, Slot7, (int)CustomRoomProperties[Slot7], (string)CustomRoomProperties["name"+Slot7], (int)CustomRoomProperties["team"+Slot7]);
            
            LobbyManager.IT.isTeamMode = (bool)CustomRoomProperties[IsTeamMode];
        }

        LobbyManager.IT.SetGameStart(PhotonNetwork.IsMasterClient); // 게임 시작 버튼 설정
        LobbyManager.IT.SetRoom(); // 방 설정
        LobbyManager.IT.HideLoading(); // 로딩 숨기기
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // 방장인 경우
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (var slot in LobbyManager.IT.roomPlayerSlots.Where(slot => slot.actorNumber == -1))
            {
                slot.actorNumber = newPlayer.ActorNumber; // 슬롯에 플레이어 번호 할당 // change?
                    
                break;
            }
            
            var slotName = LobbyManager.IT.roomPlayerSlots.Find(a => a.actorNumber == newPlayer.ActorNumber).slotName; // 슬롯 이름
            
            if (LobbyManager.IT.isTeamMode)
                SetSlotPp(slotName, newPlayer.ActorNumber, newPlayer.NickName, 1); // 슬롯 설정
            else
                SetSlotPp(slotName, newPlayer.ActorNumber, newPlayer.NickName, 0); // 슬롯 설정
        }
    }

    public void SetSlotToAI(int slotNumber)
    {
        var slot = LobbyManager.IT.roomPlayerSlots[slotNumber];
        LobbyManager.IT.SetSlotAI(slotNumber); // 슬롯
        // slot.actorNumber = 99; // AI 번호인 99로 설정
        
        if (LobbyManager.IT.isTeamMode == true)
            SetSlotPp(slot.slotName, 99, "AI_"+slotNumber, 1); // 슬롯 설정
        else
            SetSlotPp(slot.slotName, 99, "AI_"+slotNumber, 0); // 슬롯 설정

    }
    
    public void SetSlotToEmpty(int slotNumber)
    {
        var slot = LobbyManager.IT.roomPlayerSlots[slotNumber];
        LobbyManager.IT.SetSlotEmpty(slotNumber); // 빈 슬롯인 -1로 설정
        
        SetSlotPp(slot.slotName, -1, null, 0); // 슬롯 설정
    }
    
    private void SetSlotPp(string slotName, int actorNumber, string nickName, int teamNum)
    {
        Hashtable CustomRoomProperties = PhotonNetwork.CurrentRoom.CustomProperties; // 방의 프로퍼티
        
        CustomRoomProperties[slotName] = actorNumber; // 슬롯 설정
        CustomRoomProperties["team"+slotName] = teamNum; // 슬롯 팀 설정
        CustomRoomProperties["name"+slotName] = nickName;
        
        PhotonNetwork.CurrentRoom.SetCustomProperties(CustomRoomProperties); // 방의 프로퍼티 설정
    }

    public void SetChangeTeam() {
        Player player = PhotonNetwork.LocalPlayer;
        var slot = LobbyManager.IT.roomPlayerSlots.Find(a => a.actorNumber == player.ActorNumber); // 슬롯
        var cProperties = PhotonNetwork.CurrentRoom.CustomProperties;

        // Debug.Log(LobbyManager.IT.roomPlayerSlots[0].actorNumber);
        Debug.Log("Change Team To " + (int)cProperties["team"+slot.slotName] + ", Slot: " + slot.slotName);
        if ((int)cProperties["team"+slot.slotName] == 1)
        {
            cProperties["team"+slot.slotName] = 2;
        }
        else
        {
            cProperties["team"+slot.slotName] = 1;
        }
        PhotonNetwork.CurrentRoom.SetCustomProperties(cProperties);
    }

    public void SetChangeMode() {
        if (PhotonNetwork.LocalPlayer.IsMasterClient == false)
            return;
        var cProperties = PhotonNetwork.CurrentRoom.CustomProperties;

        if ((int)cProperties["GameMode"] == 1)
        {
            cProperties["GameMode"] = 2;
        }
        else
        {
            cProperties["GameMode"] = 1;
        }
        PhotonNetwork.CurrentRoom.SetCustomProperties(cProperties);
    }

	public int GetGameMode()
	{
		var cProperties = PhotonNetwork.CurrentRoom.CustomProperties;
		return (int)cProperties["GameMode"];
	}
    
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        // 방장이 나가면 방 삭제
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            LobbyManager.IT.LeftRoom(); 
        }
        else
        {
            InGameManager.IT.ExitInGame(); // 인게임에서 나가기
        }
    }

    public void LeftRoom()
    {
        PhotonNetwork.LeaveRoom(); // 방 나가기
    }
    public override void OnLeftRoom()
    {
        if (PhotonNetwork.IsMasterClient)
            InGameManager.IT.ExitInGame(); // 인게임에서 나가기
    }

    public override void OnPlayerLeftRoom(Player player) // 방안의 모든 플레이어에게 호출
    {
        Debug.Log("Net::OnPlayerLeftRoom" + player);
        // 로비에서
        if (SceneManager.GetActiveScene().name == "Lobby")
        {    
            var slot = LobbyManager.IT.roomPlayerSlots.Find(a => a.actorNumber == player.ActorNumber); // 슬롯

            if (slot == null)
                return;
        
            slot.actorNumber = -1; 

            if (PhotonNetwork.IsMasterClient)
                SetSlotPp(slot.slotName, -1, null, 0); // 빈슬롯 설정
        }
        else if (SceneManager.GetActiveScene().name == "Game")
        {
            // if (InGameManager.IT.GetIsTurn(player.NickName) && PhotonNetwork.IsMasterClient)
            //     InGameManager.IT.NextTurn(); // 다음 턴
            InGameManager.IT.RemoveTurn(player.NickName);
        }
    }

    private bool roomStartState; // 방 시작 상태
    
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        // 로비에서
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            // IsStarted가 true로 변경되면 게임 시작
            if ((bool)propertiesThatChanged[IsStarted])
            {
                PhotonNetwork.LoadLevel("Game"); // Game 씬으로 이동
                // 마스터 클라이언트가 방에 참가한 모든 플레이어들을 특정 Scene을 로드해줍니다
            
                return;
            }

			var modeText = LobbyManager.IT.esayHardButton.GetComponentInChildren<TextMeshProUGUI>();
			if ((int)propertiesThatChanged["GameMode"] == 1)
				modeText.SetText("Easy");
			else if ((int)propertiesThatChanged["GameMode"] == 2)
				modeText.SetText("Hard");

            LobbyManager.IT.SetSlot(0, Slot0, (int)propertiesThatChanged[Slot0], (string)propertiesThatChanged["name"+Slot0], (int)propertiesThatChanged["team"+Slot0]);
            LobbyManager.IT.SetSlot(1, Slot1, (int)propertiesThatChanged[Slot1], (string)propertiesThatChanged["name"+Slot1], (int)propertiesThatChanged["team"+Slot1]);
            LobbyManager.IT.SetSlot(2, Slot2, (int)propertiesThatChanged[Slot2], (string)propertiesThatChanged["name"+Slot2], (int)propertiesThatChanged["team"+Slot2]);
            LobbyManager.IT.SetSlot(3, Slot3, (int)propertiesThatChanged[Slot3], (string)propertiesThatChanged["name"+Slot3], (int)propertiesThatChanged["team"+Slot3]);
            LobbyManager.IT.SetSlot(4, Slot4, (int)propertiesThatChanged[Slot4], (string)propertiesThatChanged["name"+Slot4], (int)propertiesThatChanged["team"+Slot4]);
            LobbyManager.IT.SetSlot(5, Slot5, (int)propertiesThatChanged[Slot5], (string)propertiesThatChanged["name"+Slot5], (int)propertiesThatChanged["team"+Slot5]);
            LobbyManager.IT.SetSlot(6, Slot6, (int)propertiesThatChanged[Slot6], (string)propertiesThatChanged["name"+Slot6], (int)propertiesThatChanged["team"+Slot6]);
            LobbyManager.IT.SetSlot(7, Slot7, (int)propertiesThatChanged[Slot7], (string)propertiesThatChanged["name"+Slot7], (int)propertiesThatChanged["team"+Slot7]);
            
            gameForSlots = new List<Slot>
            {
                new(Slot0, (int)propertiesThatChanged[Slot0], (string)propertiesThatChanged["name"+Slot0], (int)propertiesThatChanged["team"+Slot0]),
                new(Slot1, (int)propertiesThatChanged[Slot1], (string)propertiesThatChanged["name"+Slot1], (int)propertiesThatChanged["team"+Slot1]),
                new(Slot2, (int)propertiesThatChanged[Slot2], (string)propertiesThatChanged["name"+Slot2], (int)propertiesThatChanged["team"+Slot2]),
                new(Slot3, (int)propertiesThatChanged[Slot3], (string)propertiesThatChanged["name"+Slot3], (int)propertiesThatChanged["team"+Slot3]),
                new(Slot4, (int)propertiesThatChanged[Slot4], (string)propertiesThatChanged["name"+Slot4], (int)propertiesThatChanged["team"+Slot4]),
                new(Slot5, (int)propertiesThatChanged[Slot5], (string)propertiesThatChanged["name"+Slot5], (int)propertiesThatChanged["team"+Slot5]),
                new(Slot6, (int)propertiesThatChanged[Slot6], (string)propertiesThatChanged["name"+Slot6], (int)propertiesThatChanged["team"+Slot6]),
                new(Slot7, (int)propertiesThatChanged[Slot7], (string)propertiesThatChanged["name"+Slot7], (int)propertiesThatChanged["team"+Slot7])
            };

            // LobbyManager.IT.SetSlot(0, PhotonNetwork.MasterClient.NickName); // 방장
        
            // 나머지 슬롯 설정
            for (var i=0; i<8; i++)
            {
                if (LobbyManager.IT.roomPlayerSlots[i].actorNumber == -1) {
                    LobbyManager.IT.SetSlotEmpty(i); // 빈 슬롯
                    LobbyManager.IT.SetIsPlayerColor(i, false);
                }
                else if (LobbyManager.IT.roomPlayerSlots[i].actorNumber == 99) {
                    LobbyManager.IT.SetSlotAI(i); // AI 슬롯
                    LobbyManager.IT.SetIsPlayerColor(i, true);
                }
                else {
                    var p = PhotonNetwork.PlayerList.FirstOrDefault(a => a.ActorNumber == LobbyManager.IT.roomPlayerSlots[i].actorNumber);
                    LobbyManager.IT.SetSlotPlayer(i, p.NickName); // 플레이어 슬롯
                    LobbyManager.IT.SetIsPlayerColor(i, true);
                }
                
                // 팀 설정
                LobbyManager.IT.SetTeamColor(i, LobbyManager.IT.roomPlayerSlots[i].teamNumber);
            }
            LobbyManager.IT.SetTeamColor(0, LobbyManager.IT.roomPlayerSlots[0].teamNumber);
        }
        else if (SceneManager.GetActiveScene().name == "Game")
        {
            gameForSlots = new List<Slot>
            {
                new(Slot0, (int)propertiesThatChanged[Slot0], (string)propertiesThatChanged["name"+Slot0], (int)propertiesThatChanged["team"+Slot0]),
                new(Slot1, (int)propertiesThatChanged[Slot1], (string)propertiesThatChanged["name"+Slot1], (int)propertiesThatChanged["team"+Slot1]),
                new(Slot2, (int)propertiesThatChanged[Slot2], (string)propertiesThatChanged["name"+Slot2], (int)propertiesThatChanged["team"+Slot2]),
                new(Slot3, (int)propertiesThatChanged[Slot3], (string)propertiesThatChanged["name"+Slot3], (int)propertiesThatChanged["team"+Slot3]),
                new(Slot4, (int)propertiesThatChanged[Slot4], (string)propertiesThatChanged["name"+Slot4], (int)propertiesThatChanged["team"+Slot4]),
                new(Slot5, (int)propertiesThatChanged[Slot5], (string)propertiesThatChanged["name"+Slot5], (int)propertiesThatChanged["team"+Slot5]),
                new(Slot6, (int)propertiesThatChanged[Slot6], (string)propertiesThatChanged["name"+Slot6], (int)propertiesThatChanged["team"+Slot6]),
                new(Slot7, (int)propertiesThatChanged[Slot7], (string)propertiesThatChanged["name"+Slot7], (int)propertiesThatChanged["team"+Slot7])
            };
        }
    }

    public int GetTeamNum(int t) // 플레이어 수
    {
        int n=0;
        foreach (var slot in LobbyManager.IT.roomPlayerSlots)
        {
            if (slot.actorNumber != -1 && slot.teamNumber == t)
            {
                n++;
            }
        }

        return n;
    }

    public int GetTankTeamNum(string name)
    {
        foreach (var slot in LobbyManager.IT.roomPlayerSlots)
        {
            if (slot.nickName == name)
                return slot.teamNumber;
        }

        return -1;
    }

    public int GetSlotTeamNum(string name)
    {
        Debug.Log("acNum: " + name);
        foreach (var slot in LobbyManager.IT.roomPlayerSlots)
        {
            if (slot.slotName == name)
            {
                Debug.Log("acNum: " +slot.slotName + ", teamNum: " + slot.teamNumber);
                return slot.teamNumber;
            }
        }

        Debug.Log("Can Not Find slot as actorNumber");
        return -1;
    }
    
    public int aiCount; // AI 수
    
    // Game Start
    public void GameStart()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        
        Hashtable CustomRoomProperties = PhotonNetwork.CurrentRoom.CustomProperties; // 방의 프로퍼티

        // AI Slot Check
        if (LobbyManager.IT.roomPlayerSlots.Any(a => a.actorNumber == 99))
            aiCount = LobbyManager.IT.roomPlayerSlots.Count(a => a.actorNumber == 99);
        else
            aiCount = 0;
        
        CustomRoomProperties[IsStarted] = true; // 게임 시작
        isStarted = true; // 게임 시작
        
        PhotonNetwork.CurrentRoom.SetCustomProperties(CustomRoomProperties); // 방의 프로퍼티 설정
    }

    public List<Turn> ShuffleTurn(List<Turn> turnList)
    {
        // Shuffle
        List<Turn> res = new List<Turn>();
        for (var i = 0; i < turnList.Count; i++)
        {
            var temp = turnList[i]; // 임시 저장
            var randomIndex = Random.Range(i, turnList.Count); // 랜덤 인덱스
            turnList[i] = turnList[randomIndex]; // 랜덤 인덱스의 값을 현재 인덱스에 저장
            turnList[randomIndex] = temp; // 현재 인덱스의 값을 랜덤 인덱스에 저장
        }
        for (var i = 0; i < turnList.Count; i++)
        {
            res.Add(turnList[i]);
        }

        Hashtable CustomRoomProperties = PhotonNetwork.CurrentRoom.CustomProperties; // 방의 프로퍼티
        int slotIdx=0;
        foreach (var turn in turnList)
        {
            CustomRoomProperties["Slot" + slotIdx] = turn.actorNumber; // 슬롯 설정
            CustomRoomProperties["team" + "Slot" + slotIdx] = turn.teamNumber; // 슬롯 팀 설정
            CustomRoomProperties["name" + "Slot" + slotIdx] = turn.nickName; // 슬롯 이름 설정
            slotIdx++;
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(CustomRoomProperties); // 방의 프로퍼티 설정

        return res;
    }
}

[Serializable]
public class Slot
{
    public string slotName;
    public int actorNumber;
    public string nickName;
    public int teamNumber;

    public Slot(string slotName, int actorNumber, string nickName, int teamNumber = 0)
    {
        this.slotName = slotName;
        this.nickName = nickName;
        this.actorNumber = actorNumber;
        this.teamNumber = teamNumber;
    }
}