using System.Collections.Generic;
using JKFrame;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif
[CreateAssetMenu(fileName = "ServerConfig", menuName = "MMORPG/ServerConfig")]
public class ServerConfig : ConfigBase
{
    [Header("通用")] 
    public GameObject NetworkManagerPrefab;

    public GameObject ServerOnGameScenePrefab;
    
    
    [Header("玩家")] 
    public Vector3 playerDefaultPosition;
    public GameObject playerPrefab;


    [Header("地形")] 
    public Dictionary<string, GameObject> terrainDic;
#if UNITY_EDITOR
    [FolderPath] public string terrainFolderPath;
    [Button] 
    public void SetTerrainDic()
    {
        if (terrainDic == null) terrainDic = new Dictionary<string, GameObject>();
        terrainDic.Clear();

        string[] files = Directory.GetFiles(terrainFolderPath); //包含.meta
        for (int i = 0; i < files.Length; i++)
        {
            if (!files[i].EndsWith(".meta"))
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(files[i]);
                terrainDic.Add(prefab.name, prefab);
            }
        }
    }
#endif

}
