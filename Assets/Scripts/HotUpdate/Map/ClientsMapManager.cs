using System;
using System.Collections;
using System.Collections.Generic;
using JKFrame;
using UnityEngine;


public class ClientsMapManager : SingletonMono<ClientsMapManager>
{
    public enum TerrainState
    {
        ReQuest,Enable,Disable
    }

    public class TerrainController
    {
        
        public Terrain terrain;
        public TerrainState state;
        public float destoryTimer;

        public void Enable()
        {
            if (state != TerrainState.Enable)
            {
                destoryTimer = 0;
                state = TerrainState.Enable;
                if (terrain != null)
                {
                    terrain.gameObject.SetActive(true);
                }
            }
        }
        public void Disable()
        {
            if (state != TerrainState.Disable)
            {
                state = TerrainState.Disable;
                if (terrain != null)
                {
                    terrain.gameObject.SetActive(false);
                }
            }
        }

        public void Load(Vector2Int coord)
        {
#if ENABLE_ADDRESSABLES
            // 客户端通过 Addressables 加载；服务端的地形由 ServerResSystem 创建。
            Vector2Int resCoord = coord + Instance.mapConfig.terrainResKeyOffset;
            string resKey = $"{resCoord.x}_{resCoord.y}";
            state = TerrainState.ReQuest;
            ResSystem.InstantiateGameObjectAsync<Terrain>(resKey, (terrain) =>
            {
                this.terrain = terrain;
                terrain.basemapDistance = 100;
                terrain.heightmapPixelError = 50;
                terrain.heightmapMaximumLOD = 1;
                terrain.detailObjectDensity = 0.9f;
                terrain.treeDistance = 10;
                terrain.treeCrossFadeLength = 10;
                terrain.treeMaximumFullLODCount = 10;
                terrain.transform.position = Instance.GetWorldPositionByTerrainCoord(coord);
                if (state == TerrainState.Disable)
                {
                    terrain.gameObject.SetActive(false);
                }
                
            }, Instance.transform, null, false);
#endif
        }

        public bool CheckAndDestroy()
        {
            if (state == TerrainState.Disable)
            {
                destoryTimer += Time.deltaTime;
                if (destoryTimer >= Instance.destroyTerrainTime)
                {
                    Destroy();
                    return true;
                }
            }

            return false;
        }
        
        
        

        

        public void Destroy()
        {
            if (terrain != null)
            {
#if ENABLE_ADDRESSABLES
                ResSystem.UnloadInstance(terrain.gameObject);
#endif
            }
            
            destoryTimer = 0;
            terrain = null;
            this.ObjectPushPool();
        }
    }
    private QuadTree quadTree;
    [SerializeField] private MapConfig mapConfig;
    [UnityEngine.Serialization.FormerlySerializedAs("mapCamera")]
    [SerializeField] private Camera camera;
    public float destroyTerrainTime;

    private Dictionary<Vector2Int, TerrainController> terrrainControllerDic =
        new Dictionary<Vector2Int, TerrainController>();
    private List<Vector2Int> destroyTerrainCoords = new List<Vector2Int>(100);
    
    private Plane[] cameraPlanes = new Plane[6];
    private Vector2Int playerTerrainCoord;
    private bool hasCenterTerrain;
    public MapConfig MapConfig
    {
        get => mapConfig;
    }

    public void Init()
    {
        // 场景村庄占据中心块 [-size/2, size/2)，不能再加载森林中心块覆盖它。
        hasCenterTerrain = transform.parent != null &&
            transform.parent.Find("CenterTerrain") != null;
        quadTree = new QuadTree(mapConfig,EnableTerrain,DisableTerrain, CheckVisibility);
        
        
    }

    private void Update()
    {
        //确保加载玩家的当前所在块
        if (camera == null) return;
        
        GeometryUtility.CalculateFrustumPlanes(camera,cameraPlanes);
        quadTree.CheckVisibility();
        playerTerrainCoord = GetTerrainCoordByWorldPosition(camera.transform.position);
        EnableTerrain(playerTerrainCoord);
        
        
        foreach (KeyValuePair<Vector2Int, TerrainController> item in terrrainControllerDic)
        {
            if (item.Value.CheckAndDestroy())
            {
                destroyTerrainCoords.Add(item.Key);
            }
        }

        foreach (Vector2Int item in destroyTerrainCoords)
        {
            terrrainControllerDic.Remove(item);
        }
        destroyTerrainCoords.Clear();
    }

    private void DisableTerrain(Vector2Int coord)
    {
        if (terrrainControllerDic.TryGetValue(coord, out TerrainController controller))
        {
            controller.Disable();
        }
    }

    private void EnableTerrain(Vector2Int coord)
    {
        if (hasCenterTerrain && coord == Vector2Int.zero) return;
        if (!terrrainControllerDic.TryGetValue(coord, out TerrainController controller))
        {
            controller = ResSystem.GetOrNew<TerrainController>();
            controller.Load(coord);
            terrrainControllerDic.Add(coord, controller);
        }
        controller.Enable();
    }

    public bool IsLoadingCompleted()
    {
        if (terrrainControllerDic.Count == 0) return false;
        foreach (TerrainController item in terrrainControllerDic.Values)
        {
            if (item.terrain == null)
            {
                return false;
            }
        }

        return true;
    }

    private Vector2Int GetTerrainCoordByWorldPosition(Vector3 worldPos)
    {
        float halfSize = mapConfig.terrainSize / 2f;
        return new Vector2Int(Mathf.FloorToInt((worldPos.x + halfSize) / mapConfig.terrainSize),
            Mathf.FloorToInt((worldPos.z + halfSize) / mapConfig.terrainSize));
    }

    private Vector3 GetWorldPositionByTerrainCoord(Vector2Int coord)
    {
        float halfSize = mapConfig.terrainSize / 2f;
        return new Vector3(coord.x * mapConfig.terrainSize - halfSize, 0,
            coord.y * mapConfig.terrainSize - halfSize);
    }

    private bool CheckVisibility(Bounds bounds)
    {
        //希望实际的可见范围大一点
        bounds.size *= 2;
        if (GeometryUtility.TestPlanesAABB(cameraPlanes, bounds)) return true;
        // 玩家当前地块附近的地块也要显示
        Vector3 boundsCenter = new Vector3(playerTerrainCoord.x * mapConfig.terrainSize,
            mapConfig.terrainMaxHeight / 2f, playerTerrainCoord.y * mapConfig.terrainSize);
        Bounds playerTerrainBounds = new Bounds(boundsCenter,
            new Vector3(mapConfig.terrainSize, mapConfig.terrainMaxHeight, mapConfig.terrainSize) * 3);
        return bounds.Intersects(playerTerrainBounds);

    }
    
    

#if UNITY_EDITOR
    public bool drawGizmos;

    private void OnDrawGizmos()
    {
        if (drawGizmos)
        {
            quadTree?.Draw();
        }
    }
#endif

    
}
