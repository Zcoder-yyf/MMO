using JKFrame;
using MongoDB.Bson.Serialization.Attributes;
using UnityEngine;

public class PlayerData
{
    [BsonId] 
    public string name;
    public string password;
    public CharacterData characterData = new CharacterData();


    public void Init(string name, string password, Vector3 position)
    {
        // 保存传进来的账号
        this.name = name;

        // 保存传进来的密码
        this.password = password;

        // 保存传进来的角色位置
        this.characterData.position = position;
    }
}

public class CharacterData
{
    public Vector3 position;
    public float rotation_Y;
}