using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetManager : NetworkManager
{
    //NetworkManager 负责管理网络连接、生成对象、消息路由等。这里把它包装一层，是为了方便扩展自己的方法。
    public static NetManager Instance { get; private set; }
 
    //UnityTransport 网络管理器 封装了IP 端口 连接管理
    public UnityTransport unityTransport { get; private set; }
    public NetMessageManager netMessageManager { get; private set; }

    //某个预制体 负责这个预制体的处理器
    private Dictionary<GameObject, NetworkPrefabInstanceHandler> prefabHandlerDic;

    public void Init(bool client)
    {
        Instance = this;
        unityTransport = GetComponent<UnityTransport>();
        netMessageManager = GetComponent<NetMessageManager>();
        if (client) InitClient();
        else InitServer();
        netMessageManager.Init();
        
        prefabHandlerDic =
            new Dictionary<GameObject, NetworkPrefabInstanceHandler>(NetworkConfig.Prefabs.Prefabs.Count);

        foreach (NetworkPrefab item in NetworkConfig.Prefabs.Prefabs)
        {
            NetworkPrefabInstanceHandler handler = new NetworkPrefabInstanceHandler(item.Prefab);
            // 保存到我们自己的字典，方便后面主动调用
            prefabHandlerDic.Add(item.Prefab, handler);
            // 登记给 Netcode，让它使用这个处理器创建和回收实例
            PrefabHandler.AddHandler(item.Prefab, handler);
        }

    }

    //启动客户端方法
    public void InitClient()
    {
        StartClient();
    }

    //启动服务端方法
    public void InitServer()
    {
        bool succeed = StartServer();

        if (succeed)
        {
            Debug.Log("服务端启动成功");
        }
        else
        {
            Debug.LogError("服务端启动失败");
        }
    }

    //生成网络对象的方法
    public NetworkObject SpawnObject(ulong clientID, GameObject prefab, Vector3 positon, Quaternion rotation)
    {
        NetworkObject networkObject = prefabHandlerDic[prefab].Instantiate(clientID, positon, rotation);
        networkObject.transform.position = positon;
        // 将实例生成为网络对象，并指定所有者
        networkObject.SpawnWithOwnership(clientID);
        // 将指定客户端加入这个对象的可见范围
        networkObject.NetworkShow(clientID);
        return networkObject;
    }

    public void DestroyObject(NetworkObject networkObject)
    {
        networkObject.Despawn();
    }
}
