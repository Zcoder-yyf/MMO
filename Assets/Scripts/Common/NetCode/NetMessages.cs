using Unity.Netcode;

public enum MessageType : byte
{
    None,
    C_S_Register, //从客户端到服务端
    S_C_Register, //从服务端到客户端
    C_S_Login,
    S_C_Login,
    C_S_EnterGame,
    S_C_Disconnet,
    C_S_Disconnet,
}

public enum ErrorCode : byte
{
    None, //意味着成功
    AccountFormat, //账号格式
    NameDuplication, // 名称或密码错误
    NameOrPassword, //账号或密码错误
    AccountRepeatLogin, //重复登录
}

/// <summary>
/// 客户端发送注册数据 服务端接受
/// </summary>
public struct C_S_Register : INetworkSerializable
{
    public AccountInfo accountInfo;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        accountInfo.NetworkSerialize(serializer);
    }
}

/// <summary>
/// 服务端返回注册结果
/// </summary>
public struct S_C_Register : INetworkSerializable
{
    public ErrorCode errorCode;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref errorCode);
    }
}

/// <summary>
/// 登录的时候客户端把用户数据发给服务端
/// </summary>
public struct C_S_Login : INetworkSerializable
{
    public AccountInfo accountInfo;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        accountInfo.NetworkSerialize(serializer);
    }
}

/// <summary>
/// 服务端返回登录结果
/// </summary>
public struct S_C_Login : INetworkSerializable
{
    public ErrorCode errorCode;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref errorCode);
    }
}

/// <summary>
/// /// <summary>
/// 客户端进入游戏
/// </summary>
public struct C_S_EnterGame : INetworkSerializable
{
    public ErrorCode errorCode; //纯占位
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref errorCode);
    }
}

public struct S_C_Disconnet : INetworkSerializable
{
    public ErrorCode errorCode;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref errorCode);
    }
}

public struct C_S_Disconnet : INetworkSerializable
{
    public ErrorCode errorCode;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref errorCode);
    }
}

/// <summary>
/// 准备通过网络发送的账号密码
/// </summary>
public struct AccountInfo : INetworkSerializable
{
    public string playerName;
    public string password;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref password);
    }
}