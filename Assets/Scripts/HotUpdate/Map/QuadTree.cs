using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTree
{
    private static MapConfig mapConfig; //地图配置 由外部传进来
    private static Action<Vector2Int> onTerrainEnable;
    private static Action<Vector2Int> onTerrainDisable;
    private static Func<Bounds, bool> OnCheckVisibility;
    private class Node
    {
        public Bounds bounds; //节点在三维空间中的范围
        private Node leftAndTop; //左上
        private Node rightAndTop; //右上
        private Node leftAndBottom; //左下
        private Node rightAndBottom; //右下
        private bool isTerrain; //标记自己是否为叶子结点
        public Vector2Int terrainCoord; //如果是叶子 记录自己的二维地形坐标

        public Node(Bounds bounds, bool divide)
        {
            this.bounds = bounds;
            isTerrain = CheckTerrain(out terrainCoord);
            if (divide && bounds.size.x > mapConfig.minQuadTreeNodeSize)
            {
                Divide();
            }
        }

        private void Divide()
        {
            float halfSize = bounds.size.x / 2;
            float halfHeight = mapConfig.terrainMaxHeight / 2;
            float positionOffset = halfSize / 2;
            Vector3 childSize = new Vector3(halfSize, mapConfig.terrainMaxHeight, halfSize);
            leftAndTop = new Node(new Bounds(new Vector3(bounds.center.x - positionOffset, halfHeight, bounds.center.z + positionOffset), childSize), true);
            rightAndTop = new Node(new Bounds(new Vector3(bounds.center.x + positionOffset, halfHeight, bounds.center.z + positionOffset), childSize), true);
            leftAndBottom = new Node(new Bounds(new Vector3(bounds.center.x - positionOffset, halfHeight, bounds.center.z - positionOffset), childSize), true);
            rightAndBottom = new Node(new Bounds(new Vector3(bounds.center.x + positionOffset, halfHeight, bounds.center.z - positionOffset), childSize), true);
        }
        
        private bool CheckTerrain(out Vector2Int coord) //判断当前节点是否是一个最小单元
        {
            Vector3 size = bounds.size;
            bool isTerrain = size.x == mapConfig.terrainSize && size.z == mapConfig.terrainSize;
            coord = Vector2Int.zero;
            if (isTerrain)
            {
                coord.x = Mathf.FloorToInt((bounds.center.x + mapConfig.terrainSize / 2f) / mapConfig.terrainSize);
                coord.y = Mathf.FloorToInt((bounds.center.z + mapConfig.terrainSize / 2f) / mapConfig.terrainSize);
                //剔除mapsize以外的的符合尺寸的terrain
                int maxCoordAbsX = (int)(mapConfig.mapSize.x / mapConfig.terrainSize)/ 2;
                int maxCoordAbsY = (int)(mapConfig.mapSize.y / mapConfig.terrainSize) / 2;
                isTerrain = coord.x >= -maxCoordAbsX && coord.x < maxCoordAbsX &&
                    coord.y >= -maxCoordAbsY && coord.y < maxCoordAbsY;
            }

            return isTerrain;
        }

        private bool active = false;

        public void CheckVisibility()
        {
            bool newActiveState = OnCheckVisibility(bounds);
            //原本可见 现在可见
            //原本不可见 现在可见
            //if ((active && newActiveState) || (!active && newActiveState))
            if (newActiveState)
            {
                if (isTerrain)
                {
                    onTerrainEnable.Invoke(terrainCoord);
                }
                else
                {
                    leftAndTop?.CheckVisibility();
                    leftAndBottom?.CheckVisibility();
                    rightAndTop?.CheckVisibility();
                    rightAndBottom?.CheckVisibility();
                }
            }
            //原本可见 现在不可见
            else if (active && !newActiveState)
            {
                Disable();
            }

            active = newActiveState;
        }

        public void Disable()
        {
            active = false;
            leftAndTop?.Disable();
            leftAndBottom?.Disable();
            rightAndTop?.Disable();
            rightAndBottom?.Disable();
            if (isTerrain)
            {
                onTerrainDisable.Invoke(terrainCoord);
            }
        }
        

#if UNITY_EDITOR
        public void Draw()
        {
            if (isTerrain && active)
            {
                Gizmos.color = new Color(0, 1, 0, 0.4f);
                Gizmos.DrawCube(bounds.center, bounds.size * 1);
            }
            
            Gizmos.color = isTerrain ? Color.green : Color.white;
            Gizmos.DrawWireCube(bounds.center, bounds.size * 1);
            
            
            Gizmos.color = Color.white;
            leftAndTop?.Draw();
            rightAndTop?.Draw();
            leftAndBottom?.Draw();
            rightAndBottom?.Draw();
        }
#endif
        
        
    }

    private Node rootNode;
    

    public QuadTree(MapConfig config, Action<Vector2Int> terrainEnable, Action<Vector2Int> terrainDisable, Func<Bounds, bool> checkVisibility)
    {
        mapConfig = config;
        onTerrainEnable = terrainEnable;
        onTerrainDisable = terrainDisable;
        OnCheckVisibility = checkVisibility;
        Bounds rootBounds = new Bounds(new Vector3(-mapConfig.terrainSize / 2f, mapConfig.terrainMaxHeight / 2, -mapConfig.terrainSize / 2f),
            new Vector3(mapConfig.quadTreeSize, mapConfig.terrainMaxHeight, mapConfig.quadTreeSize));
        rootNode = new Node(rootBounds, true);
    }

    public void CheckVisibility()
    {
        rootNode.CheckVisibility();
    }

#if UNITY_EDITOR
    public void Draw()
    {
        rootNode?.Draw();
    }
#endif
    

    
}
