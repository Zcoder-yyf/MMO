using System.Collections;
using JKFrame;
using Unity.Netcode;
using UnityEngine;

public class GameSceneLaunch : MonoBehaviour
{
    IEnumerator Start()
    {
        while (NetworkManager.Singleton == null)
        {
            yield return CoroutineTool.WaitForFrame();
        }
        
        EventSystem.TypeEventTrigger<GameSceneLaunchEvent>(default);
        
        //只要网络准备好了，就把自己挂掉
        Destroy(gameObject);
    }
}
