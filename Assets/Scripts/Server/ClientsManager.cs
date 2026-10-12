using System.Collections.Generic;
using JKFrame;
using Unity.Netcode;
using UnityEngine;

public class ClientsManager : SingletonMono<ClientsManager>
{
    public GameObject playerPrefab;
    public Dictionary<ClientState, HashSet<Client>> clientStateDic;
    //key clientId
    public Dictionary<ulong, Client> clientIdDic;
    //Key 账号 Value ClientId
    private Dictionary<string, ulong> accountDic;

    public void Init()
    {
        
        clientStateDic = new Dictionary<ClientState, HashSet<Client>>()
        {
            { ClientState.Connected, new HashSet<Client>(100) },
            { ClientState.Logined, new HashSet<Client>(100) },
            { ClientState.Gaming, new HashSet<Client>(100) },
        };
        clientIdDic = new Dictionary<ulong, Client>(100);
        accountDic = new Dictionary<string, ulong>(100);
        NetManager.Instance.OnClientConnectedCallback += OnClientConneted;
        NetManager.Instance.OnClientDisconnectCallback += OnClientNetCodeDisconneted;
        // 登记：收到注册申请时，调用 OnClientRegister
        NetMessageManager.Instance.RegisterMessageCallback(
            MessageType.C_S_Register, OnClientRegister);
        
        // 登记：收到登录申请时，调用 OnClientLogin
        NetMessageManager.Instance.RegisterMessageCallback(
            MessageType.C_S_Login, OnClientLogin);
        
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C_S_EnterGame,OnClientEnterGame);
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C_S_Disconnet,OnClientDisConneted);
        
    }

    

    private void SetClientState(ulong clientId, ClientState newState)
    {
        if (clientIdDic.TryGetValue(clientId, out Client client))
        {
            clientStateDic[client.clientState].Remove(client);
            clientStateDic[newState].Add(client);
            client.clientState = newState;
        }
    }

    


    //连接成功
    private void OnClientConneted(ulong clientId)
    {
        Client client = ResSystem.GetOrNew<Client>();
        client.clientId = clientId;
        clientIdDic.Add(clientId, client);
        SetClientState(clientId, ClientState.Connected);
    }
    
    //客户端退出
    private void OnClientNetCodeDisconneted(ulong clientId)
    {
        
        if (clientIdDic.Remove(clientId, out Client client))
        {
            clientStateDic[client.clientState].Remove(client);
            if (client.playerData != null) accountDic.Remove(client.playerData.name);
            //目前采用的是Netcode自己的管理 也就是客户端掉线会自动清除所述网络对象
            //if (client.playerController != null) NetManager.Instance.DestroyObject(client.playerController.NetworkObject);
            client.playerData = null;
            client.playerController = null;
            client.OnDestroy();
        }
    }
    
    //客户端退出到开始场景
    private void OnClientDisConneted(ulong clientId, INetworkSerializable serializable)
    {
        Client client = clientIdDic[clientId];
        //设置旧客户端为已连接但是未登录状态
        SetClientState(clientId, ClientState.Connected);
        if (client.playerController != null)
        {
            NetManager.Instance.DestroyObject(client.playerController.NetworkObject);
            client.playerController = null;
        }

        if (client.playerData == null)
        {
            accountDic.Remove(client.playerData.name);
            client.playerData = null;
        }
        
        //回复消息
        NetMessageManager.Instance.SendMessageToClient<S_C_Disconnet>(MessageType.S_C_Disconnet, default, clientId);
    }
    
    
    
    

    #region Register

    private void OnClientRegister(ulong clientId, INetworkSerializable serializable)
    {
        // 把收到的消息按注册申请类型取出来
        C_S_Register netMessage = (C_S_Register)serializable;

        // 取出消息里的账号和密码
        AccountInfo accountInfo = netMessage.accountInfo;

        // 准备回复客户端的注册结果，初始设置为没有错误
        S_C_Register result = new S_C_Register { errorCode = ErrorCode.None };

        if (!AccountFormatUtility.CheckName(accountInfo.playerName) ||
            !AccountFormatUtility.CheckPassword(accountInfo.password))
        {
            result.errorCode = ErrorCode.AccountFormat;
        }

        else if (DatabaseManager.Instance.GetPlayerData(accountInfo.playerName) != null)
        {
            result.errorCode = ErrorCode.NameDuplication;
        }

        else
        {
            PlayerData playerData = ResSystem.GetOrNew<PlayerData>();
            playerData.characterData = new CharacterData
            {
                position = ServerResSystem.serverConfig.playerDefaultPosition
            };

            playerData.name = accountInfo.playerName;
            playerData.password = accountInfo.password;

            DatabaseManager.Instance.CreatePlayerData(playerData);
        }

        //收到注册申请后找谁处理
        NetMessageManager.Instance.SendMessageToClient(MessageType.S_C_Register, result, clientId);
    }

    #endregion


    #region Login

    private void OnClientLogin(ulong clientId, INetworkSerializable serializable)
    {
        //取出登录申请
        C_S_Login netMessage = (C_S_Login)serializable;
        //拿到账号密码
        AccountInfo accountInfo = netMessage.accountInfo;
        //准备登录结果
        S_C_Login result = new S_C_Login { errorCode = ErrorCode.None };
        
        // 不信任客户端
        if (!AccountFormatUtility.CheckName(accountInfo.playerName) ||
            !AccountFormatUtility.CheckPassword(accountInfo.password))
        {
            result.errorCode = ErrorCode.AccountFormat;
        }

        else
        {
            PlayerData playerData = DatabaseManager.Instance.GetPlayerData(accountInfo.playerName);
            if (playerData == null || playerData.password != accountInfo.password)
            {
                result.errorCode = ErrorCode.NameOrPassword;
            }

            else
            {
                //检查挤号
                if (accountDic.TryGetValue(accountInfo.playerName, out ulong oldClientID))
                {
                    //通知旧客户端
                    NetMessageManager.Instance.SendMessageToClient(MessageType.S_C_Disconnet, new S_C_Disconnet
                    {
                        errorCode = ErrorCode.AccountRepeatLogin
                    }, oldClientID);
                    
                    //设置旧客户端为已连接 但是未登录
                    SetClientState(oldClientID, ClientState.Connected);
                    
                    //有可能存在的角色需要销毁
                    if (clientIdDic.TryGetValue(oldClientID, out Client oldClient))
                    {
                        if (oldClient.playerController != null)
                        {
                            NetManager.Instance.DestroyObject(oldClient.playerController.NetworkObject);
                            oldClient.playerController = null;
                        }

                        oldClient.playerData = null;
                    }
                }
                accountDic[accountInfo.playerName] = clientId;
                //登陆成功 将玩家数据关联到当前客户端
                Client client = clientIdDic[clientId];
                client.playerData = playerData;
                SetClientState(clientId, ClientState.Logined);
                
            }
        }
        
        // 回复客户端登录结果
        NetMessageManager.Instance.SendMessageToClient(
            MessageType.S_C_Login, result, clientId);
    }
    
    //玩家进入游戏
    private void OnClientEnterGame(ulong clientID, INetworkSerializable arg2)
    {
        //无需回复客户端 直接创建角色
        Client client = clientIdDic[clientID];
        if (client.clientState == ClientState.Gaming) return;
        SetClientState(clientID, ClientState.Logined);
        PlayerData playerData = client.playerData;
        CharacterData characterData = playerData.characterData;
        NetworkObject playerObject = NetManager.Instance.SpawnObject(clientID, ServerResSystem.serverConfig.playerPrefab,
            characterData.position, Quaternion.Euler(0, characterData.rotation_Y, 0));
        client.playerController = playerObject.GetComponent<PlayerController>();
        
        //TODO:玩家可能使用不同武器
    }
    
    

    #endregion
    


}





