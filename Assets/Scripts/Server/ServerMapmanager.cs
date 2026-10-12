using JKFrame;
using UnityEngine;

public class ServerMapmanager : SingletonMono<ServerMapmanager>
{
    [SerializeField] private MapConfig mapConfig;
    
    int testRange = 36; //过滤掉外围几圈的地图快
    public void Init()
    {
        //一次性加载全部地图
        int width = (int)(mapConfig.mapSize.x / mapConfig.terrainSize);
        int height = (int)(mapConfig.mapSize.y / mapConfig.terrainSize);

        for (int x = testRange / 2; x < width - testRange / 2; x++)
        {
            for (int y = testRange / 2; y < height - testRange / 2; y++)
            {
                Vector2Int resCoord = new Vector2Int(x, y);
                
                // 将 terrainCoord 的计算提前，用于判断
                Vector2Int terrainCoord = resCoord - mapConfig.terrainResKeyOffset;
                
                // 跳过中心块
                // if (terrainCoord == Vector2Int.zero)
                // {
                //     continue;
                // }

                string resKey = $"{resCoord.x}_{resCoord.y}";

// 地块中心坐标转换为 Terrain 左下角位置。
                float halfSize = mapConfig.terrainSize / 2f;
                Vector3 position = new Vector3(
                    terrainCoord.x * mapConfig.terrainSize - halfSize,
                    0,
                    terrainCoord.y * mapConfig.terrainSize - halfSize);

// 从服务端配置获取预制体，创建地形并关闭渲染。
                ServerResSystem.InstantiateTerrain(resKey, transform, position);

            }
        }
    }
}