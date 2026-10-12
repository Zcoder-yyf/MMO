using UnityEngine;
using JKFrame;
namespace Common.Utility
{
    // 第 14.16 节练习占位，保留脚本和 .meta，稍后逐步实现。
    public static class AOIUtility
    {
        public static float chunkSize { get; private set; } //保存格子的边长。外部可以读取，但不能直接赋值；

        public static void Init(float size) //初始化，例如传入 50，表示每个格子边长为 50 个世界单位。
        {
            chunkSize = size;
        }

        //世界坐标转格子编号
        public static Vector2Int GetCoordByWorldPosition(Vector3 worldPosition)
        {
            return new Vector2Int(
                Mathf.FloorToInt(worldPosition.x / chunkSize),
                Mathf.FloorToInt(worldPosition.z / chunkSize));
        }

        public static void AddPlayer(PlayerController player, Vector2Int coord)
        {
            //将这份数据交给 JKFrame，通知当前进程中监听这种事件的方法。
            EventSystem.TypeEventTrigger(new AOIAddPlayerEvent
            {
                player = player,
                coord = coord
            });
        }

        public static void UpdatePlayerCoord(PlayerController player, Vector2Int oldCoord, Vector2Int newCoord)
        {
            //将这份数据交给 JKFrame，通知当前进程中监听这种事件的方法。
            EventSystem.TypeEventTrigger(new AOIUpdatePlayerCoordEvent
            {
                player = player,
                oldCoord =  oldCoord,
                newCoord = newCoord
            });
        }

        public static void RemovePlayer(PlayerController player, Vector2Int coord)
        {
            //将这份数据交给 JKFrame，通知当前进程中监听这种事件的方法。
            EventSystem.TypeEventTrigger(new AOIRemovePlayerEvent
            {
                player = player, coord = coord
            });
        }
    }
}
