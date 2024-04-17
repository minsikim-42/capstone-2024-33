// using UnityEngine;
// using System;
// using System.Collections.Generic;
// using Photon.Pun;
// using Unity.Properties;

// public class DataManager : MonoBehaviour
// {
//     public static DataManager IT;
//     private PhotonView PV;
//     public List<Slot> roomSlots;
    
//     private void Awake()
//     {
//         if (IT == null) {
//             IT = this;
//             if (PV == null)
//             {
//                 // PhotonView 컴포넌트 추가
//                 PV = gameObject.AddComponent<PhotonView>();
//             }
//         }
//         else
//             Destroy(gameObject);
        
//         DontDestroyOnLoad(gameObject);
//     }

//     private void Start()
//     {
//     }

//     public void SetSlot() {
        
//     }

//     [PunRPC]
//     public void RPC_SetSlot() {
//         roomSlots.Clear();
//         roomSlots = new List<Slot>();
//         foreach (var slot in LobbyManager.IT.roomPlayerSlots) {
//             if (slot.actorNumber > 0)
//                 roomSlots.Add(new Slot(slot.slotName, slot.actorNumber, slot.nickName, slot.teamNumber));
//         }
//     }

// }
// [Serializable]
// public class Slot
// {
//     public string slotName;
//     public int actorNumber;
//     public string nickName;
//     public int teamNumber;

//     public Slot(string slotName, int actorNumber, string nickName, int teamNumber = 0)
//     {
//         this.slotName = slotName;
//         this.nickName = nickName;
//         this.actorNumber = actorNumber;
//         this.teamNumber = teamNumber;
//     }
// }