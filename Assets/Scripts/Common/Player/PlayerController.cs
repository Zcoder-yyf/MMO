using JKFrame;
using Unity.Netcode;
using UnityEngine;
using Common.Utility;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

// 第 14.16 节复盘起点：先保留单个类和基础角色流程。
// 本节的 partial 拆分、两端清理和 AOI 事件连接将逐步补回。
public partial class PlayerController : NetworkBehaviour, IStateMachineOwner
{
    private NetVarible<PlayerState> currentState = new NetVarible<PlayerState>(PlayerState.None);
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsClient)
        {
            Client_OnNetworkSpawn();
        }

        if (IsServer)
        {
#if UNITY_SERVER || UNITY_EDITOR
            Server_OnNetworkSpawn();
#endif
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            Client_OnNetworkDespawn();
            
        }

        if (IsServer)
        {
#if UNITY_SERVER || UNITY_EDITOR
            Server_OnNetworkDespawn();
#endif
        }
        base.OnNetworkDespawn();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendInputMoveDirServerRpc(Vector3 moveDir)
    {
#if UNITY_SERVER || UNITY_EDITOR
        Server_ReceiveMoveInput(moveDir);
#endif
    }

    
}

//客户端
public partial class PlayerController : NetworkBehaviour
{
    public Transform cameraLookTarget;
    public Transform cameraFollowTarget;

    public void Start()
    {
        //Start 一定是在OnNetworkSpawn后执行 如果这个阶段IsSpawned = false 说明是个异常对象
        if (!IsSpawned)
        {
            NetManager.Instance.DestroyObject(this.NetworkObject);
        }
    }

    private void Client_OnNetworkSpawn()
    {
        if (!IsOwner) return;

        //这句意思是 属于你的角色已经生成好了 这就是它
        EventSystem.TypeEventTrigger(
            new InitLocalPlayerEvent
            {
                localPlayer = this
            }
        );

        this.AddUpdate(LocalClientUpdate);
    }

    private Vector3 lastInputDir = Vector3.zero;

    private void Client_OnNetworkDespawn()
    {
        this.RemoveUpdate(LocalClientUpdate);
    }

    private void LocalClientUpdate()
    {
        if (currentState.Value == PlayerState.None)
            return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 inputDir =
            new Vector3(h, 0, v);

        if (inputDir == Vector3.zero &&
            lastInputDir == Vector3.zero)
            return;

        lastInputDir = inputDir;

        float cameraEulerAngleY =
            Camera.main.transform.eulerAngles.y;

        SendInputMoveDirServerRpc(
            Quaternion.Euler(
                0,
                cameraEulerAngleY,
                0
            ) * inputDir
        );
    }
}

//服务端
#if UNITY_SERVER || UNITY_EDITOR
public partial class PlayerController : NetworkBehaviour, IStateMachineOwner
{
    #region 内部

    public class InputData
    {
        public Vector3 moveDir;
        
    }

    #endregion

    #region 面板赋值

    [SerializeField] private float moveSpeed = 1f;
    public float MoveSpeed { get => moveSpeed; }
    
    [SerializeField] private float rotateSpeed = 1000;
    public float RotateSpeed { get => rotateSpeed; }

    [SerializeField] private Animator animator;
    public Animator Animator { get => animator; }
    
    [SerializeField] private CharacterController characterController;
    public CharacterController CharacterController { get => characterController; }
    
    [SerializeField] private Player_View view;
    public Player_View View { get => view; }
    

    #endregion

    #region  解决幽灵问题

    private bool aoiInitialized;

    public void Server_InitAOI()
    {
        if (aoiInitialized) return;

        currentAOICoord =
            AOIUtility.GetCoordByWorldPosition(
                transform.position
            );

        aoiInitialized = true;

        AOIUtility.AddPlayer(
            this,
            currentAOICoord
        );
    }

    #endregion
    
    
   
    public Vector2Int currentAOICoord { get; private set; }
    public InputData inputData { get; private set; }
    
    private StateMachine stateMachine;
    
    private void Server_ReceiveMoveInput(Vector3 moveDir)
    {
        
        inputData.moveDir = moveDir.normalized;
        //状态类中根据输入情况进行计算
        
    }
    

    

    private void Server_OnNetworkSpawn()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>(true);

        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        if (view == null)
            view = GetComponentInChildren<Player_View>(true);

        stateMachine = new StateMachine();
        stateMachine.Init(this);

        inputData = new InputData();

        currentAOICoord =
            AOIUtility.GetCoordByWorldPosition(transform.position);

        // AOIUtility.AddPlayer(this, currentAOICoord);

        ChangeState(PlayerState.Idle);
    }
    
    private void Server_OnNetworkDespawn()
    {
        stateMachine?.Destroy();

        if (aoiInitialized)
        {
            AOIUtility.RemovePlayer(
                this,
                currentAOICoord
            );

            aoiInitialized = false;
        }
    }

    public void ChangeState(PlayerState newState)
    {
        switch (newState)
        {
            case PlayerState.Idle:
                stateMachine.ChangeState<PlayerIdleState>();
                break;
            case PlayerState.Move:
                stateMachine.ChangeState<PlayerMoveState>();
                break;
            default:
                return;
        }
        // 将服务端状态同步给客户端，允许初始化完成后的玩家读取输入。
        currentState.Value = newState;
    }

    public void PlayAnimation(string animationName, float fixedTransitionDuration = 0.25f)
    {
        animator.CrossFadeInFixedTime(animationName, fixedTransitionDuration);
    }

    public void UpdateAOICoord()
    {
        Vector2Int newCoord = AOIUtility.GetCoordByWorldPosition(transform.position);
        if (newCoord != currentAOICoord)
        {
            AOIUtility.UpdatePlayerCoord(this,currentAOICoord, newCoord);
            currentAOICoord = newCoord;
        }
    }



}
#endif
