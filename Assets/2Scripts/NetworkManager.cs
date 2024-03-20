using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager IT;
    
    private PhotonView PV; // PhotonView
    private readonly string gameVersion = "1"; // 게임 버전
    
    private List<RoomInfo> allRoomList; // 모든 방 리스트
    
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
    const string Slot1 = "Slot1";
    const string Slot2 = "Slot2";
    const string Slot3 = "Slot3";
    const string Slot4 = "Slot4";
    const string Slot5 = "Slot5";
    const string Slot6 = "Slot6";
    const string Slot7 = "Slot7";
    
    [SerializeField] private List<Slot> slotList; // 슬롯 리스트
    public void CreateRoom(string roomName)
    {
        // 방 설정
        var roomOptions = new RoomOptions
        {
            CustomRoomPropertiesForLobby = new[] {IsStarted, Slot1, Slot2, Slot3, Slot4, Slot5, Slot6, Slot7},
            CustomRoomProperties = new Hashtable {{IsStarted, false}, {Slot1, -1}, {Slot2, -1}, {Slot3, -1}, {Slot4, -1}, {Slot5, -1}, {Slot6, -1}, {Slot7, -1}},
            MaxPlayers = 8
        };
        
        slotList = new List<Slot>
        {
            new(Slot1, -1),
            new(Slot2, -1),
            new(Slot3, -1),
            new(Slot4, -1),
            new(Slot5, -1),
            new(Slot6, -1),
            new(Slot7, -1)
        };


        PhotonNetwork.CreateRoom($"{roomName}", roomOptions); // 방 생성
    }
    
    
    public override void OnCreatedRoom()
    {
        LobbyManager.IT.SetRoom(); // 방 설정
        LobbyManager.IT.SetGameStart(PhotonNetwork.IsMasterClient); // 게임 시작 버튼 설정
        
        LobbyManager.IT.SetSlot(0, PhotonNetwork.MasterClient.NickName); // 방장 슬롯
            
        for (var i = 0; i < slotList.Count; i++)
        {
            if (slotList[i].actorNumber == -1)
                LobbyManager.IT.SetSlotEmpty(i + 1); // 빈 슬롯
            else if (slotList[i].actorNumber == 99)
                LobbyManager.IT.SetSlotAI(i + 1); // AI 슬롯
            else
                LobbyManager.IT.SetSlot(i + 1, PhotonNetwork.PlayerList.FirstOrDefault(a => a.ActorNumber == slotList[i].actorNumber)?.NickName); // 플레이어 슬롯
        }
        
        LobbyManager.IT.HideLoading(); // 로딩 숨기기
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
            // hashtable 받아오기
            Hashtable CustomRoomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
            
            // Slot 설정
            slotList = new List<Slot>
            {
                new(Slot1, (int)CustomRoomProperties[Slot1]),
                new(Slot2, (int)CustomRoomProperties[Slot2]),
                new(Slot3, (int)CustomRoomProperties[Slot3]),
                new(Slot4, (int)CustomRoomProperties[Slot4]),
                new(Slot5, (int)CustomRoomProperties[Slot5]),
                new(Slot6, (int)CustomRoomProperties[Slot6]),
                new(Slot7, (int)CustomRoomProperties[Slot7])
            };
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
            foreach (var slot in slotList.Where(slot => slot.actorNumber == -1))
            {
                slot.actorNumber = newPlayer.ActorNumber; // 슬롯에 플레이어 번호 할당
                    
                break;
            }
            
            var slotName = slotList.Find(a => a.actorNumber == newPlayer.ActorNumber).slotName; // 슬롯 이름
            
            SetSlot(slotName, newPlayer.ActorNumber); // 슬롯 설정
        }
    }

    public void SetSlotToAI(int slotNumber)
    {
        var slot = slotList[slotNumber - 1]; // 슬롯
            slot.actorNumber = 99; // AI 번호인 99로 설정
        
        SetSlot(slot.slotName, 99); // 슬롯 설정
    }
    
    public void SetSlotToEmpty(int slotNumber)
    {
        var slot = slotList[slotNumber - 1]; // 슬롯
            slot.actorNumber = -1; // 빈 슬롯인 -1로 설정
        
        SetSlot(slot.slotName, -1); // 슬롯 설정
    }
    
    private void SetSlot(string slotName, int actorNumber)
    {
        Hashtable CustomRoomProperties = PhotonNetwork.CurrentRoom.CustomProperties; // 방의 프로퍼티
        
        CustomRoomProperties[slotName] = actorNumber; // 슬롯 설정
        
        PhotonNetwork.CurrentRoom.SetCustomProperties(CustomRoomProperties); // 방의 프로퍼티 설정
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

    public override void OnPlayerLeftRoom(Player player)
    {
        // 로비에서
        if (SceneManager.GetActiveScene().name == "Lobby")
        {    
            var slot = slotList.Find(a => a.actorNumber == player.ActorNumber); // 슬롯

            if (slot == null)
                return;
        
            slot.actorNumber = -1; 

            if (PhotonNetwork.IsMasterClient)
                SetSlot(slot.slotName, -1); // 슬롯 설정
        }
        else if (SceneManager.GetActiveScene().name == "Game")
        {
            if (PhotonNetwork.IsMasterClient)
                InGameManager.IT.NextTurn(); // 다음 턴
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
            
                return;
            }

            // Slot 변경
            slotList = new List<Slot>
            {
                new(Slot1, (int)propertiesThatChanged[Slot1]),
                new(Slot2, (int)propertiesThatChanged[Slot2]),
                new(Slot3, (int)propertiesThatChanged[Slot3]),
                new(Slot4, (int)propertiesThatChanged[Slot4]),
                new(Slot5, (int)propertiesThatChanged[Slot5]),
                new(Slot6, (int)propertiesThatChanged[Slot6]),
                new(Slot7, (int)propertiesThatChanged[Slot7])
            };

            LobbyManager.IT.SetSlot(0, PhotonNetwork.MasterClient.NickName); // 방장
        
            // 나머지 슬롯 설정
            for (var i = 0; i < slotList.Count; i++)
            {
                if (slotList[i].actorNumber == -1)
                    LobbyManager.IT.SetSlotEmpty(i + 1); // 빈 슬롯
                else if (slotList[i].actorNumber == 99)
                    LobbyManager.IT.SetSlotAI(i + 1); // AI 슬롯
                else
                    LobbyManager.IT.SetSlot(i + 1, PhotonNetwork.PlayerList.FirstOrDefault(a => a.ActorNumber == slotList[i].actorNumber)?.NickName); // 플레이어 슬롯
            }
        }
    }


    public int aiCount; // AI 수
    
    // Game Start
    public void GameStart()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        
        Hashtable CustomRoomProperties = PhotonNetwork.CurrentRoom.CustomProperties; // 방의 프로퍼티

        // AI Slot Check
        if (slotList.Any(a => a.actorNumber == 99))
            aiCount = slotList.Count(a => a.actorNumber == 99);
        else
            aiCount = 0;
        
        CustomRoomProperties[IsStarted] = true; // 게임 시작
        
        PhotonNetwork.CurrentRoom.SetCustomProperties(CustomRoomProperties); // 방의 프로퍼티 설정
    }
}

[Serializable]
public class Slot
{
    public string slotName;
    public int actorNumber;

    public Slot(string slotName, int actorNumber)
    {
        this.slotName = slotName;
        this.actorNumber = actorNumber;
    }
}