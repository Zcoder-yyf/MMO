using JKFrame;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 网络对象退场时，把它的 GameObject 留在对象池里；下次需要同类对象时，取出来重新参与网络同步。
/// 
/// </summary>
public class NetworkPrefabInstanceHandler : INetworkPrefabInstanceHandler
{

    private GameObject prefab;

    public NetworkPrefabInstanceHandler(GameObject prefab)
    {
        //保存这个处理器负责的预制体
        this.prefab = prefab;
    }

    /// <summary>
    /// 加载方法 从对象池取出一个实例 如果没有就创建一个
    /// </summary>
    /// <param name="ownerClientId"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <returns></returns>
    public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
    {
        NetworkObject networkObject = PoolSystem.GetGameObject<NetworkObject>(prefab.name);

        if (networkObject == null)
        {
            networkObject = GameObject.Instantiate(prefab).GetComponent<NetworkObject>();
            // 统一名称，让取出和归还使用同一个池名称
            networkObject.name = prefab.name;
        }
        
        //无论是复用旧的实例 还是刚创建的新实例 都设置本次的位置和角度
        networkObject.transform.position = position;
        networkObject.transform.rotation = rotation;

        return networkObject;
    }

    public void Destroy(NetworkObject networkObject)
    {
        //销毁就是放回对象池
        networkObject.GameObjectPushPool();
    }
}
