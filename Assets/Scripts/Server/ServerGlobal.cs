using JKFrame;
using Unity.Netcode;
using UnityEngine;

public class ServerGlobal : SingletonMono<ServerGlobal>
{
    [SerializeField] private ServerConfig serverConfig;

    public ServerConfig ServerConfig
    {
        get => serverConfig;
    }

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        EventSystem.AddTypeEventListener<GameSceneLaunchEvent>(OnGameSceneLaunchEvent);

        
    }

    private void OnGameSceneLaunchEvent(GameSceneLaunchEvent @event)
    {
        ServerResSystem.InstantiateServerOnGameScene();
    }

}
