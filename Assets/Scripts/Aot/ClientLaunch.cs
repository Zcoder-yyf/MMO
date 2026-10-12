using JKFrame;
using UnityEngine;

public class ClientLaunch : MonoBehaviour
{
    void Start()
    {
        Application.targetFrameRate = 60;

        GetComponent<HotUpdateSystem>()
            .StartHotUpdate(null, (bool succeed) =>
            {
                if (succeed)
                {
                    OnHotUpdateSucceed();
                }
            });
    }

    private void OnHotUpdateSucceed()
    {
        //ResSystem.InstantiateGameObject<NetManager>("NetworkManager").Init(true);

        ResSystem.InstantiateGameObject("ClientGlobal");

        
    }
}