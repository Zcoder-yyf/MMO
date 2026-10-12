using Unity.Netcode;

public class NetVarible<T> : NetworkVariable<T>
{
   
    public NetVarible(T value = default,
        NetworkVariableReadPermission readPerm = DefaultReadPerm,
        NetworkVariableWritePermission writePerm = DefaultWritePerm)
        : base(value ,readPerm, writePerm)
    {
       
    }

    public override bool IsDirty()
    {
        //本项目中 客户端没有修改网络变量的权限 所以直接过滤且避免NetworkVariableSerialization为null的情况
        if (NetworkVariableSerialization<T>.AreEqual == null) return false;
        return base.IsDirty();
        
    }
}