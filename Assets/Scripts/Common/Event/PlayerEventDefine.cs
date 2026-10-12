using UnityEngine;

//本地玩家初始化
public struct InitLocalPlayerEvent
{
    public PlayerController localPlayer;
}


//玩家加入AOI(加入时，需要知道“谁，进入哪个格子”)
public struct AOIAddPlayerEvent
{
    public PlayerController player;
    public Vector2Int coord;
}

//玩家跨格(跨格时，需要知道“谁，从哪里，到哪里”)
public struct AOIUpdatePlayerCoordEvent
{
    public PlayerController player;
    public Vector2Int oldCoord;
    public Vector2Int newCoord;
}

//玩家离开AOI(需要知道“谁，从哪个格子移除”)
public struct AOIRemovePlayerEvent
{
    public PlayerController player;
    public Vector2Int coord;
}


