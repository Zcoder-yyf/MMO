using JKFrame;
using Cinemachine;
using UnityEngine;

public class PlayerManager : SingletonMono<PlayerManager>
{
    [SerializeField]
    private CinemachineFreeLook cinemachine;

    public static PlayerController localPlayer { get; private set; }

    public void Init()
    {
        EventSystem.AddTypeEventListener<InitLocalPlayerEvent>(
            OnInitLocalPlayerEvent
        );
    }
    
    private void OnDestroy()
    {
        EventSystem.RemoveTypeEventListener<InitLocalPlayerEvent>(OnInitLocalPlayerEvent);
    }

    private void OnInitLocalPlayerEvent(
        InitLocalPlayerEvent arg)
    {
        InitLocalPlayer(arg.localPlayer);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UI_GamePopupWindow gamePopupWindow = UISystem.GetWindow<UI_GamePopupWindow>();

            if (gamePopupWindow == null || !gamePopupWindow.gameObject.activeInHierarchy)
            {
                UISystem.Show<UI_GamePopupWindow>();
            }

            else
            {
                UISystem.Close<UI_GamePopupWindow>();
            }
            
        }
    }
    
    public bool IsLoadingCompleted()
    {
        return localPlayer != null; 
    }

    private void InitLocalPlayer(PlayerController player)
    {
        localPlayer = player;

        cinemachine.transform.position =
            player.transform.position;
        // 本机玩家生成后，客户端相机也必须绑定注视和跟随目标。
        cinemachine.LookAt =
            localPlayer.cameraLookTarget;

        cinemachine.Follow =
            localPlayer.cameraFollowTarget;
        
    }

    
    
}