using System;
using Unity.Collections;
using Unity.Netcode;
using System.Collections.Generic;
using JKFrame;
using UnityEngine;


public class NetMessageManager : SingletonMono<NetMessageManager>
{
    //得到当前运行的网络管理器,取得它提供的 CustomMessagingManager
    private CustomMessagingManager messagingManager => NetManager.Instance.CustomMessagingManager;
    
    private Dictionary<MessageType, Action<ulong, INetworkSerializable>>  receiveMessageCallbackDic =
        new Dictionary<MessageType, Action<ulong, INetworkSerializable>>();

    public void Init()
    {
        messagingManager.OnUnnamedMessage += ReceiveMessage;
    }

    /// <summary>
    /// 收到某种消息，应该调用哪个方法处理
    /// </summary>
    /// <param name="messageType"></param>
    /// <param name="callback"></param>
    public void RegisterMessageCallback(MessageType messageType, Action<ulong, INetworkSerializable> callback)
    {
        if (receiveMessageCallbackDic.ContainsKey(messageType))
        {
            receiveMessageCallbackDic[messageType] += callback;
        }
        else
        {
            receiveMessageCallbackDic.Add(messageType, callback);
        }
    }
    
    /// <summary>
    /// 取消消息处理方法的登记
    /// </summary>
    /// <param name="messageType"></param>
    /// <param name="callback"></param>
    public void UnRegisterMessageCallback(MessageType messageType, Action<ulong, INetworkSerializable> callback)
    {
        if (receiveMessageCallbackDic.ContainsKey(messageType))
        {
            receiveMessageCallbackDic[messageType] -= callback;
        }
        
    }


    /// <summary>
    /// 按消息类型查字典，再执行对应方法
    /// </summary>
    /// <param name="messageType"></param>
    /// <param name="clientId"></param>
    /// <param name="data"></param>
    public void TriggerMessageCallback(MessageType messageType, ulong clientId, INetworkSerializable data)
    {
        if (receiveMessageCallbackDic.TryGetValue(messageType, out Action<ulong, INetworkSerializable> callback))
        {
            callback?.Invoke(clientId, data);
        }
    }

    private FastBufferWriter WriteData<T>(MessageType messageType, T data) where T : INetworkSerializable
    {
        FastBufferWriter writer = new FastBufferWriter(1024, Allocator.Temp); //创建一个初始容量为 1024 字节的二进制缓冲区
        writer.WriteValueSafe(messageType); //先写消息类型
        writer.WriteValueSafe(data); //再写消息内容
        return writer;
    }

    /// <summary>
    /// 客户端 → SendMessageToServer → 服务端
    /// </summary>
    /// <param name="messageType"></param>
    /// <param name="data"></param>
    /// <typeparam name="T"></typeparam>
    public void SendMessageToServer<T>(MessageType messageType, T data) where T : INetworkSerializable
    {
        //把传进来的消息类型和消息内容交给 WriteData，拿到写好的二进制缓冲区 writer。
        using (FastBufferWriter writer = WriteData(messageType, data))
        {
            messagingManager.SendUnnamedMessage(NetManager.ServerClientId, writer);
        }
    }
    
    /// <summary>
    /// 服务端 → SendMessageToClient → 指定客户端
    /// </summary>
    /// <param name="messageType"></param>
    /// <param name="data"></param>
    /// <param name="clientId"></param>
    /// <typeparam name="T"></typeparam>
    public void SendMessageToClient<T>(MessageType messageType, T data, ulong clientId) where T : INetworkSerializable
    {
        
        using (FastBufferWriter writer = WriteData(messageType, data))
        {
            messagingManager.SendUnnamedMessage(clientId, writer);
        }
    }

    /// <summary>
    /// 服务端 → SendMessageToClient → 指定部分客户端
    /// </summary>
    /// <param name="messageType"></param>
    /// <param name="data"></param>
    /// <param name="clientId"></param>
    /// <typeparam name="T"></typeparam>
    public void SendMessageToClients<T>(MessageType messageType, T data, IReadOnlyList<ulong> clientIds) where T : INetworkSerializable
    {
        using (FastBufferWriter writer = WriteData(messageType, data))
        {
            messagingManager.SendUnnamedMessage(clientIds, writer);
        }
    }

    public void SendMessageToAllClients<T>(MessageType messageType, T data, IReadOnlyList<ulong> clientIds)
        where T : INetworkSerializable
    {
        using (FastBufferWriter writer = WriteData(messageType, data))
        {
            messagingManager.SendUnnamedMessageToAll(writer);
        }
    }
    
    private void ReceiveMessage(ulong clientId, FastBufferReader reader)
    {

        try
        {
            // 先读取消息类型
            reader.ReadValueSafe(out MessageType messageType);

            // 根据消息类型，读取对应的消息内容
            switch (messageType)
            {
                // 客户端发来的注册申请
                case MessageType.C_S_Register:
                    reader.ReadValueSafe(out C_S_Register C_S_Register);
                    TriggerMessageCallback(MessageType.C_S_Register, clientId, C_S_Register);
                    break;

                // 客户端发来的登录申请
                case MessageType.C_S_Login:
                    reader.ReadValueSafe(out C_S_Login C_S_Login);
                    TriggerMessageCallback(MessageType.C_S_Login, clientId, C_S_Login);
                    break;

                // 服务端发来的注册结果
                case MessageType.S_C_Register:
                    reader.ReadValueSafe(out S_C_Register S_C_Register);
                    TriggerMessageCallback(MessageType.S_C_Register, clientId, S_C_Register);
                    break;

                // 服务端发来的登录结果
                case MessageType.S_C_Login:
                    reader.ReadValueSafe(out S_C_Login loginMessage);
                    TriggerMessageCallback(
                        MessageType.S_C_Login, clientId, loginMessage);
                    break;

                // 客户端发来的进入游戏
                case MessageType.C_S_EnterGame:
                    reader.ReadValueSafe(out C_S_EnterGame C_S_EnterGame);
                    TriggerMessageCallback(
                        MessageType.C_S_EnterGame, clientId, C_S_EnterGame);
                    break;
                
                // 客户端发来的进入游戏
                case MessageType.S_C_Disconnet:
                    reader.ReadValueSafe(out S_C_Disconnet S_C_Disconnet);
                    TriggerMessageCallback(
                        MessageType.S_C_Disconnet, clientId, S_C_Disconnet);
                    break;
                
                // 客户端发来的进入游戏
                case MessageType.C_S_Disconnet:
                    reader.ReadValueSafe(out C_S_Disconnet C_S_Disconnet);
                    TriggerMessageCallback(
                        MessageType.C_S_Disconnet, clientId, C_S_Disconnet);
                    break;
            }
        }
        catch (Exception)
        {
            Debug.Log("消息接收失败");
        }


    }
    
    
}
