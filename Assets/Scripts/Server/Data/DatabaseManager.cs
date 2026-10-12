using JKFrame;
using MongoDB.Driver;
using UnityEngine;
public class DatabaseManager : SingletonMono<DatabaseManager>
{
    [SerializeField] private string connstr = "mongodb://localhost:27017";
    private IMongoDatabase mmoDatabase;
    private IMongoCollection<PlayerData> playerDataCollection;
    public void Init()
    {
        MongoClient mongoClient = new MongoClient(connstr); //创建数据库访问对象
        mmoDatabase = mongoClient.GetDatabase("MMONetCode"); //取得数据库
        playerDataCollection = mmoDatabase.GetCollection<PlayerData>("PlayerData"); //取得存放玩家数据的集合
    }

    /// <summary>
    /// 传进来查询的账号 返回用户数据
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public PlayerData GetPlayerData(string name)
    {
        PlayerData playerData = playerDataCollection.Find(Builders<PlayerData>.Filter.Eq(nameof(PlayerData.name), name))
            .FirstOrDefault();

        return playerData;
    }

    public void CreatePlayerData(PlayerData playerData)
    {
        playerDataCollection.InsertOne(playerData);
    }

    public void SavePlayerData(PlayerData newPlayerData)
    {
        playerDataCollection.ReplaceOne(Builders<PlayerData>.Filter.Eq(nameof(PlayerData.name), name), newPlayerData);
    }
}
