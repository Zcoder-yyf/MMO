using UnityEngine;
using UnityEngine.SceneManagement;


public class ServerLaunch : MonoBehaviour
{
    void Start()
    {
       //  string connstr = "mongodb://localhost:27017";
       //  MongoClient mongoClient = new MongoClient(connstr); //创建一个数据库访问对象
       //  IMongoDatabase mmoDatabase = mongoClient.GetDatabase("MMONetCode"); //创建一个数据库
       //  IMongoCollection<TestUseData> userInfoCollection = mmoDatabase.GetCollection<TestUseData>("UserInfo");//创建一个数据集合
       //  TestUseData testUseData = new TestUseData //创建一条数据
       //  {
       //      useId = 2,
       //      name = "WSD",
       //      lv = 1,
       //      dataTime = DateTime.Now
       //  };
       //  
       //  
       //  //创建查询条件
       //  FilterDefinition<TestUseData> filter = Builders<TestUseData>.Filter.Eq("_id", 2);
       //  TestUseData useData = userInfoCollection.Find(filter).FirstOrDefault();
       //  if (useData == null)
       //  {
       //      userInfoCollection.InsertOne(testUseData);//插入一条数据
       //      // 查询结果为空，显示刚插入的新对象。
       //      Debug.Log($"新增用户：{testUseData.name}，等级：{testUseData.lv}");
       //  }
       //  else
       //  {
       //      // 查询结果不为空，显示已有用户。
       //      Debug.Log($"用户已存在：{useData.name}，等级：{useData.lv}");
       //  }
       //
       //  List<TestUseData> useDatas = userInfoCollection.Find(Builders<TestUseData>.Filter.Empty).ToList();
       //  Debug.Log($"用户数量{useDatas.Count}");
       //
       //  //组合条件
       //  FilterDefinition<TestUseData> multiFilter = Builders<TestUseData>.Filter.And(
       //      Builders<TestUseData>.Filter.Eq("_id", 2), Builders<TestUseData>.Filter.Eq("lv", 1));
       //  
       //  TestUseData matchedUser = userInfoCollection
       //      .Find(multiFilter)
       //      .FirstOrDefault();
       //  
       //  if (matchedUser != null)
       //  {
       //      Debug.Log($"符合条件的用户：{matchedUser.name}");
       //  }
       //  else
       //  {
       //      Debug.Log("没有用户同时满足这两个条件");
       //  }
       //  
       //  userInfoCollection.UpdateOne(
       //      Builders<TestUseData>.Filter.Eq("_id", 2),
       //      Builders<TestUseData>.Update.Set("lv", 2)
       //  );
       //  //Replace
       //  // 先修改内存中这个对象的等级。
       //  testUseData.lv = 3;
       //
       // // 找到数据库中编号为 2 的用户，用这个对象替换它。
       //  userInfoCollection.ReplaceOne(
       //      Builders<TestUseData>.Filter.Eq("_id", 2),
       //      testUseData
       //  );
       //  
       //  userInfoCollection.DeleteOne(Builders<TestUseData>.Filter.Eq("_id", 2));
        
        Application.targetFrameRate = 30;
        InitServers();
        SceneManager.LoadScene("GameScene");
    }

    private void InitServers()
    {
        ServerResSystem.InstantiateNetworkManagerPrefab().Init(false);
        Debug.Log("InitServers Succeed");
    }
}
