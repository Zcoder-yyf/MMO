#if UNITY_SERVER || UNITY_EDITOR
using Common.Utility;
using UnityEngine;

public class PlayerIdleState : PlayerStateBase
{
    public override void Enter()
    {
        player.PlayAnimation("Idle");
        
    }

    

    public override void Update()
    {
        
        if (player.inputData.moveDir != Vector3.zero)
        {
            player.ChangeState(PlayerState.Move);
            return;
        }

        if (!player.CharacterController.isGrounded)
        {
            player.CharacterController.Move(new Vector3(0, -9.8f * Time.deltaTime, 0));
        }

        

    }

    
}

#endif
