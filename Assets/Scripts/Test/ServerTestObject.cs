
using Unity.Netcode;
using UnityEngine;
using Common.Utility;

public class ServerTestObject : NetworkBehaviour
{
    public float moveSpeed = 3f;
    public static ServerTestObject Instance;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
#if UNITY_SERVER || UNITY_EDITOR
        if (!IsServer) return;
        Instance = this;
        AOIManager.Instance.InitServerObject(NetworkObject, Vector2Int.zero);
#endif
    }

    private void Update()
    {
#if UNITY_SERVER || UNITY_EDITOR
            

        if (IsServer)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 dir = new Vector3(h, 0, v).normalized;
            HandleMovementServer(dir);
            
        }
#endif
    }

 #if UNITY_SERVER || UNITY_EDITOR
    private void HandleMovementServer(Vector3 inputDir)
    {
        Vector2Int oldCoord = AOIUtility.GetCoordByWorldPosition(transform.position);
        transform.Translate(Time.deltaTime * moveSpeed * inputDir);
        Vector2Int newCoord = AOIUtility.GetCoordByWorldPosition(transform.position);
        if (newCoord != oldCoord) //发生了地图块坐标变化
        {
            AOIManager.Instance.UpdateServerObjectChunkCoord(NetworkObject, oldCoord, newCoord);
        
        }
    }
#endif
}
