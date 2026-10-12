using System;
using JKFrame;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/MaoConfig")]
public class MapConfig : ConfigBase
{
    public float quadTreeSize = 19200; //四叉树的尺寸 300 * 4 * 4 * 4 = 19200
    public Vector2 mapSize = new Vector2(12000, 12000);
    public Vector2Int terrainResKeyOffset; //地形坐标的偏移量 用来把真实的世界坐标转换为数组格子的索引
    public float terrainSize = 300f; //最小地形块的尺寸
    public float minQuadTreeNodeSize = 300; //四叉树节点允许分割的最小尺寸
    public float terrainMaxHeight = 200f; //地形的最高海拔高度 用来生成3d包围盒的y轴高度

    //当这个脚本第一次挂载 或者在面板上点击reset的时候调用
    private void Reset()
    {
        OnValidate();
    }
    

    private void OnValidate()
    {
        //计算地图原点(0,0)相对于整个网格的偏移量
        //例如12000 / 300 / 2 = 20 所以偏移量是 (-20, 20)
        //这昂就把以地图为中心的(0,0)的坐标系完美转换成了从左下角开始的(0, 0)二维数组索引
        terrainResKeyOffset = new Vector2Int((int)(mapSize.x / terrainSize / 2), (int)(mapSize.y / terrainSize / 2));
        
    }
}
