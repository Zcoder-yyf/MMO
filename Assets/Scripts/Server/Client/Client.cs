using JKFrame;


public class Client
{ 
    public ulong clientId;
    public ClientState clientState;
    public PlayerData playerData;
    public PlayerController playerController;
    public void OnDestroy() 
    { 
        playerData = null;
        this.ObjectPushPool();
    }
}
