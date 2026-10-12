using System;
using UnityEngine;
/// <summary>
/// 
/// Player_View 自己不决定玩家要不要移动
/// </summary>
public class Player_View : MonoBehaviour
{
#if UNITY_SERVER || UNITY_EDITOR
    [SerializeField] private Animator animator;

    private Action<Vector3, Quaternion> rootMotionAction;
    private void OnAnimatorMove()
    {
        rootMotionAction?.Invoke(animator.deltaPosition, animator.deltaRotation);
    }

    public void SetRootMotionAction(Action<Vector3, Quaternion> rootMotionAction)
    {
        this.rootMotionAction = rootMotionAction;
    }

    public void CleanRootMotionAction()
    {
        this.rootMotionAction = null;
    }
#endif

    #region 动画事件
    
    private void FootStep()
    {
    }
    
    #endregion
    
    //1服务端生成玩家 客户端连接后 服务端ClientsManager创建Player 调用SpawnAsPlayerObject(clientId) NGO在服务端和客户端都生成这个玩家对象
    //调用各自的PlayerController.OnNetworkSpawn 服务端的创建inputData 和 状态机 先进入Idle
    //客户端发出本机玩家已生成的事件 PlayerManager接受以后让FreeLook相机跟随并开始读取按键
    
    //2客户端读取按键 结合相机朝向算出移动方向 通过SendInputMoveDirServerRpc发给服务端 服务端的ReceiveMoveInput只把方向存到inputData.moveDir
    
    //3服务端的PlayerIdleState.Update发现方向不为0 切换到PlayerMoveState
    
    //4MoveState 播放Move的动画 Player_View 取出动画这一帧的位移deltaPosition 把唯一传给MoveState.OnRootMotion MoveState调用角色状态机的Move
    //从而让服务端的位置真正改变 同时MoveState发现 传送过来的是0向量就切回Idle 停止位移和切回动画
    
    //5Player根对象上的NetworkTransform 把服务端移动后的实际位置同步给客户端 模型上的同步组件处理模型朝向和动画 客户端应用这些结果
    //FreeLook 相机继续跟随移动后的玩家
}
