using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class TestManager : MonoBehaviour
{
   public GameObject serverTestObjectPrefab;
   public NetworkObject testInstance;
   

   private void OnGUI()
   {
#if UNITY_SERVER || UNITY_EDITOR
      if (NetManager.Instance.IsServer)
      {
         if (ServerTestObject.Instance != null)
         {
            GUILayout.Label("Positon" + ServerTestObject.Instance.transform.position);
         }

         if (testInstance == null && Input.GetKeyDown(KeyCode.E))
         {
            testInstance = NetManager.Instance.SpawnObject(NetManager.ServerClientId, serverTestObjectPrefab,
               Vector3.zero, Quaternion.identity);
         }

         if (testInstance != null && Input.GetKeyDown(KeyCode.F))
         {
            NetManager.Instance.DestroyObject(testInstance);
            testInstance = null;
         }
            

        
      }
      
#endif
#if !UNITY_SERVER || UNITY_EDITOR
      if (PlayerManager.localPlayer != null)
      {
         //延迟
         GUILayout.Label("Delay" + ClientRTTInfo.Instance.rttMs);
         //当前坐标
         GUILayout.Label("Positon" + PlayerManager.localPlayer.transform.position);
         if (NetManager.Instance.SpawnManager.OwnershipToObjectsTable.TryGetValue(NetManager.ServerClientId, out Dictionary<ulong, NetworkObject> temp))
         {
            //服务端对象数量
            GUILayout.Label("ServerObjects" + temp.Count);
         }
         
         //其他客户端对象数量
         int clientObjects = 0;
         foreach (KeyValuePair<ulong, Dictionary<ulong, NetworkObject>> item in NetManager.Instance.SpawnManager.OwnershipToObjectsTable)
         {
            if (item.Key != NetManager.ServerClientId && item.Key != NetManager.Instance.LocalClientId)
            {
               clientObjects += item.Value.Count;
            }
         }
         GUILayout.Label("OtherClientObjects" + clientObjects);
         
      }
      
#endif
   }
}
