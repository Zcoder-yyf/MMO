using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class ServerResSystem
{
    public static ServerConfig serverConfig;

    static ServerResSystem()
    {
        serverConfig = ServerGlobal.Instance.ServerConfig;
    }

    public static NetManager InstantiateNetworkManagerPrefab()
    {
        GameObject prefab = serverConfig.NetworkManagerPrefab;
        GameObject instance = GameObject.Instantiate(prefab);
        return instance.GetComponent<NetManager>();
    }

    public static GameObject InstantiateServerOnGameScene()
    {
        GameObject prefab = serverConfig.ServerOnGameScenePrefab;
        GameObject instance = GameObject.Instantiate(prefab);
        return instance;
    }
    
    public static GameObject InstantiateTerrain(string resKey, Transform parent, Vector3 position)
    {
        GameObject prefab = serverConfig.terrainDic[resKey];
        GameObject instance = GameObject.Instantiate(prefab, position, Quaternion.identity, parent);
        instance.GetComponent<Terrain>().enabled = false;
        return instance;
    }
}