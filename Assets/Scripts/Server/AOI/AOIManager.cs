using System;
using System.Collections.Generic;
using JKFrame;
using Unity.Netcode;
using UnityEngine;
using Common.Utility;

public class AOIManager : SingletonMono<AOIManager>
{
    private readonly static Vector2Int defaultCoord; //表示一个无效的初始坐标
    [SerializeField] private float chunkSize = 50; //每个格子的大小
    [SerializeField] private int visualChunkRange = 1; //视野半径 正好是一圈九宫格

    //格子坐标 和客户端ID集合
    private Dictionary<Vector2Int, HashSet<ulong>> chunkClientDic = new Dictionary<Vector2Int, HashSet<ulong>>();

    //格子坐标 网络对象集合
    private Dictionary<Vector2Int, HashSet<NetworkObject>> chunkServerObjectDic =
        new Dictionary<Vector2Int, HashSet<NetworkObject>>();

    static AOIManager()
    {
        defaultCoord = new Vector2Int(int.MinValue, int.MinValue);
    }

    // 复盘起点：坐标算法暂时保留在管理器内，后续再迁入 AOIUtility。
    

    public void Init()
    {
        AOIUtility.Init(chunkSize);
        EventSystem.AddTypeEventListener<AOIAddPlayerEvent>(OnAOIAddPlayerEvent);
        EventSystem.AddTypeEventListener<AOIUpdatePlayerCoordEvent>(OnAOIUpdatePlayerCoordEvent);
        EventSystem.AddTypeEventListener<AOIRemovePlayerEvent>(OnAOIRemovePlayerEvent);
    }
    
   

    private void OnDestroy()
    {
        EventSystem.RemoveTypeEventListener<AOIAddPlayerEvent>(
            OnAOIAddPlayerEvent);

        EventSystem.RemoveTypeEventListener<AOIUpdatePlayerCoordEvent>(
            OnAOIUpdatePlayerCoordEvent);

        EventSystem.RemoveTypeEventListener<AOIRemovePlayerEvent>(
            OnAOIRemovePlayerEvent);
    }

    private void OnAOIAddPlayerEvent(AOIAddPlayerEvent arg)
    {
        InitClient(arg.player.OwnerClientId, arg.coord);
    }
    
    private void OnAOIUpdatePlayerCoordEvent(AOIUpdatePlayerCoordEvent arg)
    {
        UpdateClientChunkCoord(arg.player.OwnerClientId, arg.oldCoord, arg.newCoord);
    }

    private void OnAOIRemovePlayerEvent(AOIRemovePlayerEvent arg)
    {
        RemoveClient(arg.player.OwnerClientId, arg.coord);
    }

    #region Client

    //初始化游戏客户端 把客户端登记进AOI系统
    public void InitClient(ulong clientID, Vector2Int chunkCoord)
    {
        UpdateClientChunkCoord(clientID, defaultCoord, chunkCoord);
    }

    //更新玩家在AOI地图块上的坐标
    public void UpdateClientChunkCoord(ulong clientID, Vector2Int oldCoord, Vector2Int newCoord)
    {
        if (oldCoord == newCoord) return;
        //从旧的地图块中移除
        RemoveClient(clientID, oldCoord);

        if (Vector2Int.Distance(oldCoord, newCoord) > 1.5f) //超过单个格子的极限距离 所以属于传送
        {
            for (int x = -visualChunkRange; x <= visualChunkRange; x++)
            {
                for (int y = -visualChunkRange; y <= visualChunkRange; y++)
                {
                    Vector2Int hideChunkCoord = new Vector2Int(oldCoord.x + x, oldCoord.y + y);
                    Vector2Int showChunkCoord = new Vector2Int(newCoord.x + x, newCoord.y + y);
                    ShowAndHideForChunk(clientID, hideChunkCoord, showChunkCoord);
                }
            }
        }
        else //正常一个格子
        {
            //y轴方向判断
            if (newCoord.y > oldCoord.y) //向上移动
            {
                int hideY = oldCoord.y - visualChunkRange;
                int showY = newCoord.y + visualChunkRange;
                for (int x = -visualChunkRange; x <= visualChunkRange; x++)
                {
                    Vector2Int hideChunk = new Vector2Int(oldCoord.x + x, hideY);
                    Vector2Int showChunk = new Vector2Int(newCoord.x + x, showY);
                    ShowAndHideForChunk(clientID, hideChunk, showChunk);
                }
            }
            else if (newCoord.y < oldCoord.y) //向下移动
            {
                int hideY = oldCoord.y + visualChunkRange;
                int showY = newCoord.y - visualChunkRange;
                for (int x = -visualChunkRange; x <= visualChunkRange; x++)
                {
                    Vector2Int hideChunk = new Vector2Int(oldCoord.x + x, hideY);
                    Vector2Int showChunk = new Vector2Int(newCoord.x + x, showY);
                    ShowAndHideForChunk(clientID, hideChunk, showChunk);
                }
            }

            //x轴方向判断（独立于y轴，处理斜向移动）
            if (newCoord.x < oldCoord.x) //向左移动
            {
                int hideX = oldCoord.x + visualChunkRange;
                int showX = newCoord.x - visualChunkRange;
                for (int y = -visualChunkRange; y <= visualChunkRange; y++)
                {
                    Vector2Int hideChunk = new Vector2Int(hideX, oldCoord.y + y);
                    Vector2Int showChunk = new Vector2Int(showX, newCoord.y + y);
                    ShowAndHideForChunk(clientID, hideChunk, showChunk);
                }
            }
            else if (newCoord.x > oldCoord.x) //向右移动
            {
                int hideX = oldCoord.x - visualChunkRange;
                int showX = newCoord.x + visualChunkRange;
                for (int y = -visualChunkRange; y <= visualChunkRange; y++)
                {
                    Vector2Int hideChunk = new Vector2Int(hideX, oldCoord.y + y);
                    Vector2Int showChunk = new Vector2Int(showX, newCoord.y + y);
                    ShowAndHideForChunk(clientID, hideChunk, showChunk);
                }
            }
        }

        //添加到新地图块
        if (!chunkClientDic.TryGetValue(newCoord, out HashSet<ulong> newCoordClientIDs))
        {
            newCoordClientIDs = ResSystem.GetOrNew<HashSet<ulong>>();
            chunkClientDic.Add(newCoord, newCoordClientIDs);
        }
        newCoordClientIDs.Add(clientID);
    }

    //视野刷新 当判定玩家发生网格跨越时候 加载新区域 卸载旧区域
    private void ShowAndHideForChunk(ulong clientID, Vector2Int hideChunkCoord, Vector2Int showChunkCoord)
    {
        ShowClientForChunkClients(clientID, showChunkCoord);
        HideClientForChunkClients(clientID, hideChunkCoord);
        ShowChunkServerObjectForClient(clientID, showChunkCoord);
        HideChunkServerObjectForClient(clientID, hideChunkCoord);
    }

    //拿着新网格的坐标去查字典，取出里面的玩家名单并遍历，让刚进来的玩家和名单里的每一个老玩家执行"互相显示"
    private void ShowClientForChunkClients(ulong clientID, Vector2Int showChunkCoord)
    {
        if (chunkClientDic.TryGetValue(showChunkCoord, out HashSet<ulong> clientIDs))
        {
            var showClientEnumerator = clientIDs.GetEnumerator();
            while (showClientEnumerator.MoveNext())
            {
                ulong otherClientID = showClientEnumerator.Current;
                ClientMutualShow(clientID, otherClientID);
            }
        }
    }

    //拿着旧网格的坐标查字典并遍历，让离开的玩家和仍然留在旧网格的玩家互相执行"视野剔除"
    private void HideClientForChunkClients(ulong clientID, Vector2Int hideChunkCoord)
    {
        if (chunkClientDic.TryGetValue(hideChunkCoord, out HashSet<ulong> clientIDs))
        {
            var hideClientEnumerator = clientIDs.GetEnumerator();
            while (hideClientEnumerator.MoveNext())
            {
                ulong otherClientID = hideClientEnumerator.Current;
                ClientMutualHide(clientID, otherClientID);
            }
        }
    }

    //查找新网格里的纯服务端对象（比如野怪、NPC、掉落物）名单并遍历，通知底层框架把这些物体同步给当前玩家
    private void ShowChunkServerObjectForClient(ulong clientID, Vector2Int showChunkCoord)
    {
        if (chunkServerObjectDic.TryGetValue(showChunkCoord, out HashSet<NetworkObject> serverObjects))
        {
            var showServerObjectEnumerator = serverObjects.GetEnumerator();
            while (showServerObjectEnumerator.MoveNext())
            {
                NetworkObject serverObject = showServerObjectEnumerator.Current;
                if (!serverObject.IsNetworkVisibleTo(clientID))
                {
                    serverObject.NetworkShow(clientID);
                }
            }
        }
    }

    //查找旧网格的服务端对象名单并遍历，通知客户端销毁这些远去的事物
    private void HideChunkServerObjectForClient(ulong clientID, Vector2Int hideChunkCoord)
    {
        if (chunkServerObjectDic.TryGetValue(hideChunkCoord, out HashSet<NetworkObject> serverObjects))
        {
            var hideServerObjectEnumerator = serverObjects.GetEnumerator();
            while (hideServerObjectEnumerator.MoveNext())
            {
                NetworkObject serverObject = hideServerObjectEnumerator.Current;
                if (serverObject.IsNetworkVisibleTo(clientID))
                {
                    serverObject.NetworkHide(clientID);
                }
            }
        }
    }

    //这是玩家间双向视野的核心。它会提取玩家 A 和 B 名下的所有网络实体，互相调用 NetworkShow 强行将对象数据下发给对方客户端进行渲染。
    private void ClientMutualShow(ulong clientA, ulong clientB)
    {
        if (clientA == clientB) return;
        if (NetManager.Instance.SpawnManager.OwnershipToObjectsTable.TryGetValue(clientA, out Dictionary<ulong, NetworkObject> aNetWorkObjectDic) &&
            NetManager.Instance.SpawnManager.OwnershipToObjectsTable.TryGetValue(clientB, out Dictionary<ulong, NetworkObject> bNetWorkObjectDic))
        {
            foreach (NetworkObject aItem in aNetWorkObjectDic.Values)
            {
                if (!aItem.IsNetworkVisibleTo(clientB)) aItem.NetworkShow(clientB);
            }

            foreach (NetworkObject bItem in bNetWorkObjectDic.Values)
            {
                if (!bItem.IsNetworkVisibleTo(clientA)) bItem.NetworkShow(clientA);
            }
        }
    }

    //逻辑相对应，通过相互调用 NetworkHide 截断数据同步，并在对方客户端触发销毁流程，达到节省两端渲染和网络性能的目的。
    private void ClientMutualHide(ulong clientA, ulong clientB)
    {
        if (clientA == clientB) return;
        if (NetManager.Instance.SpawnManager.OwnershipToObjectsTable.TryGetValue(clientA, out Dictionary<ulong, NetworkObject> aNetWorkObjectDic) &&
            NetManager.Instance.SpawnManager.OwnershipToObjectsTable.TryGetValue(clientB, out Dictionary<ulong, NetworkObject> bNetWorkObjectDic))
        {
            foreach (NetworkObject aItem in aNetWorkObjectDic.Values)
            {
                if (aItem.IsNetworkVisibleTo(clientB)) aItem.NetworkHide(clientB);
            }

            foreach (NetworkObject bItem in bNetWorkObjectDic.Values)
            {
                if (bItem.IsNetworkVisibleTo(clientA)) bItem.NetworkHide(clientA);
            }
        }
    }

    //利用哈希表高效的删除特性，将玩家迅速移出旧网格的名单。当网格人数归零时，立即将清空后的集合推回对象池
    private void RemoveClient(ulong clientID, Vector2Int coord)
    {
        if (chunkClientDic.TryGetValue(coord, out HashSet<ulong> clientIDs))
        {
            if (clientIDs.Remove(clientID) && clientIDs.Count == 0)
            {
                clientIDs.ObjectPushPool();
                chunkClientDic.Remove(coord);
            }
        }
    }

    #endregion

    #region ServerObject

    public void InitServerObject(NetworkObject serverObject, Vector2Int chunkCoord)
    {
        UpdateServerObjectChunkCoord(serverObject, defaultCoord, chunkCoord);
    }

    public void UpdateServerObjectChunkCoord(NetworkObject serverObject, Vector2Int oldCoord, Vector2Int newCoord)
    {
        if (oldCoord == newCoord) return;
        RemoveServerObject(serverObject, oldCoord);

        if (Vector2Int.Distance(oldCoord, newCoord) > 1.5f) //超过单个格子移动的极限距离 所以是传送性质的位移
        {
            for (int x = -visualChunkRange; x <= visualChunkRange; x++)
            {
                for (int y = -visualChunkRange; y <= visualChunkRange; y++)
                {
                    Vector2Int hideChunkCoord = new Vector2Int(oldCoord.x + x, oldCoord.y + y);
                    Vector2Int showChunkCoord = new Vector2Int(newCoord.x + x, newCoord.y + y);
                    ShowAndHideServerObjectForChunk(serverObject, hideChunkCoord, showChunkCoord);
                }
            }
        }
        else
        {
            //y轴方向判断
            if (newCoord.y > oldCoord.y) //向上移动
            {
                int hideY = oldCoord.y - visualChunkRange;
                int showY = newCoord.y + visualChunkRange;
                for (int x = -visualChunkRange; x <= visualChunkRange; x++)
                {
                    Vector2Int hideChunk = new Vector2Int(oldCoord.x + x, hideY);
                    Vector2Int showChunk = new Vector2Int(newCoord.x + x, showY);
                    ShowAndHideServerObjectForChunk(serverObject, hideChunk, showChunk);
                }
            }
            else if (newCoord.y < oldCoord.y) //向下移动
            {
                int hideY = oldCoord.y + visualChunkRange;
                int showY = newCoord.y - visualChunkRange;
                for (int x = -visualChunkRange; x <= visualChunkRange; x++)
                {
                    Vector2Int hideChunk = new Vector2Int(oldCoord.x + x, hideY);
                    Vector2Int showChunk = new Vector2Int(newCoord.x + x, showY);
                    ShowAndHideServerObjectForChunk(serverObject, hideChunk, showChunk);
                }
            }

            //x轴方向判断（独立于y轴，处理斜向移动）
            if (newCoord.x < oldCoord.x) //向左移动
            {
                int hideX = oldCoord.x + visualChunkRange;
                int showX = newCoord.x - visualChunkRange;
                for (int y = -visualChunkRange; y <= visualChunkRange; y++)
                {
                    Vector2Int hideChunk = new Vector2Int(hideX, oldCoord.y + y);
                    Vector2Int showChunk = new Vector2Int(showX, newCoord.y + y);
                    ShowAndHideServerObjectForChunk(serverObject, hideChunk, showChunk);
                }
            }
            else if (newCoord.x > oldCoord.x) //向右移动
            {
                int hideX = oldCoord.x - visualChunkRange;
                int showX = newCoord.x + visualChunkRange;
                for (int y = -visualChunkRange; y <= visualChunkRange; y++)
                {
                    Vector2Int hideChunk = new Vector2Int(hideX, oldCoord.y + y);
                    Vector2Int showChunk = new Vector2Int(showX, newCoord.y + y);
                    ShowAndHideServerObjectForChunk(serverObject, hideChunk, showChunk);
                }
            }
        }

        //添加到新地图块
        if (!chunkServerObjectDic.TryGetValue(newCoord, out HashSet<NetworkObject> newCoordServerObjects))
        {
            newCoordServerObjects = ResSystem.GetOrNew<HashSet<NetworkObject>>();
            chunkServerObjectDic.Add(newCoord, newCoordServerObjects);
        }
        newCoordServerObjects.Add(serverObject);
    }

    private void ShowAndHideServerObjectForChunk(NetworkObject serverObject, Vector2Int hideChunkCoord, Vector2Int showChunkCoord)
    {
        ShowServerObjectForChunkClients(serverObject, showChunkCoord);
        HideServerObjectForChunkClients(serverObject, hideChunkCoord);
    }

    private void ShowServerObjectForChunkClients(NetworkObject serverObject, Vector2Int showChunkCoord)
    {
        if (chunkClientDic.TryGetValue(showChunkCoord, out HashSet<ulong> clientIDs))
        {
            var clientEnumerator = clientIDs.GetEnumerator();
            while (clientEnumerator.MoveNext())
            {
                ulong clientID = clientEnumerator.Current;
                if (!serverObject.IsNetworkVisibleTo(clientID))
                {
                    serverObject.NetworkShow(clientID);
                }
            }
        }
    }

    private void HideServerObjectForChunkClients(NetworkObject serverObject, Vector2Int hideChunkCoord)
    {
        if (chunkClientDic.TryGetValue(hideChunkCoord, out HashSet<ulong> clientIDs))
        {
            var clientEnumerator = clientIDs.GetEnumerator();
            while (clientEnumerator.MoveNext())
            {
                ulong clientID = clientEnumerator.Current;
                if (serverObject.IsNetworkVisibleTo(clientID))
                {
                    serverObject.NetworkHide(clientID);
                }
            }
        }
    }

    private void RemoveServerObject(NetworkObject serverObject, Vector2Int chunkCoord)
    {
        if (chunkServerObjectDic.TryGetValue(chunkCoord, out HashSet<NetworkObject> serverObjects))
        {
            if (serverObjects.Remove(serverObject) && serverObjects.Count == 0)
            {
                serverObjects.ObjectPushPool();
                chunkServerObjectDic.Remove(chunkCoord);
            }
        }
    }

    #endregion

   
}
