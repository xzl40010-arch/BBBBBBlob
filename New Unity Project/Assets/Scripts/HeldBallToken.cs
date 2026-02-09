//郑佳鑫
//2026.2.9 第一次修改：持有球令牌：当玩家从生成点获取球时创建，销毁时如果未被标记为已使用则通知生成点请求重生
using UnityEngine;

public class HeldBallToken : MonoBehaviour
{
    private BallSpawnPoint source;
    private bool used;

    public void Initialize(BallSpawnPoint sourcePoint)
    {
        source = sourcePoint;
    }

    public void MarkUsed()
    {
        used = true;
    }

    private void OnDestroy()
    {
        if (!used && source != null)
        {
            source.RequestRespawn();
        }
    }
}

