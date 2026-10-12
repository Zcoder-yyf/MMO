using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerOnGameSceneManager : MonoBehaviour
{
    void Start()
    {
        DatabaseManager.Instance.Init();
        AOIManager.Instance.Init();
        GetComponent<ClientsManager>().Init();
        ServerMapmanager.Instance.Init();
        
    }
}
