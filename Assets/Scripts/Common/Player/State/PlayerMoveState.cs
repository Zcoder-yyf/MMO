#if UNITY_SERVER || UNITY_EDITOR
using Common.Utility;
using UnityEngine;

public class PlayerMoveState : PlayerStateBase
{
    public override void Enter()
    {
        player.PlayAnimation("Move");
        player.View.SetRootMotionAction(OnRootMotion);
    }

    public override void Update()
    {
        if (player.inputData.moveDir == Vector3.zero)
        {
            player.ChangeState(PlayerState.Idle);
            return;
        }

        //旋转
        player.View.transform.rotation = Quaternion.RotateTowards(player.View.transform.rotation,
            Quaternion.LookRotation(player.inputData.moveDir),
            Time.deltaTime * player.RotateSpeed);
    }


    public override void Exit()
    {
        player.View.CleanRootMotionAction();
        
    }

    private void OnRootMotion(Vector3 deltaPosition, Quaternion deltaRotation)
    {
        
        player.Animator.speed = player.MoveSpeed;
        deltaPosition.y -= 9.8f * Time.deltaTime; //模拟重力
        player.CharacterController.Move(deltaPosition);
        if (deltaPosition != Vector3.zero)
        {
            player.UpdateAOICoord();
        }
    }
}
#endif