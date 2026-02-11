//郑佳鑫
//2026.2.10：固态从上方下落可撞碎的平台
//核心逻辑：只有凝固态 + 正在下落 + 玩家在平台上方 才会碎
//如需支持T键重置，请添加 Resettable 组件
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BreakablePlatform : MonoBehaviour
{
    [Header("触发条件")]
    [Tooltip("玩家下落速度必须超过这个值才会碎（正数，例如5表示向下速度5）")]
    [SerializeField] private float minFallSpeed = 5f;

    [Header("调试")]
    [SerializeField] private bool logHit = false;

    private Collider2D platformCollider;

    private void Awake()
    {
        platformCollider = GetComponent<Collider2D>();
        if (platformCollider != null && platformCollider.isTrigger)
        {
            platformCollider.isTrigger = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision == null || collision.collider == null)
            return;

        // 1. 获取玩家
        Player hitPlayer = collision.collider.GetComponentInParent<Player>();
        if (hitPlayer == null)
            return;

        // 2. 必须是凝固态
        if (hitPlayer.CurrentState != Player.PlayerState.Solid)
        {
            if (logHit) Debug.Log($"[BreakablePlatform] 不是凝固态，跳过");
            return;
        }

        // 3. 用 relativeVelocity 获取碰撞前的速度
        // relativeVelocity 是玩家相对于平台的速度
        // 如果玩家从上方落下，relativeVelocity.y 应该是负值（向下）
        float relativeY = collision.relativeVelocity.y;
        
        if (logHit) Debug.Log($"[BreakablePlatform] relativeVelocity.y = {relativeY:F2}");

        // 4. 关键判断：玩家必须是从上方撞下来的
        // relativeVelocity.y < 0 表示玩家相对于平台向下移动
        if (relativeY >= 0)
        {
            if (logHit) Debug.Log($"[BreakablePlatform] 玩家不是从上方下落 (relY={relativeY:F2})，跳过");
            return;
        }

        // 5. 关键判断：玩家必须在平台上方（玩家底部 >= 平台顶部 - 容差）
        float playerBottom = collision.collider.bounds.min.y;
        float platformTop = platformCollider.bounds.max.y;
        float tolerance = 0.5f;

        if (playerBottom < platformTop - tolerance)
        {
            if (logHit) Debug.Log($"[BreakablePlatform] 玩家不在平台上方 (playerBottom={playerBottom:F2}, platformTop={platformTop:F2})，跳过");
            return;
        }

        // 6. 下落速度检查
        float fallSpeed = -relativeY; // 转为正数
        if (fallSpeed < minFallSpeed)
        {
            if (logHit) Debug.Log($"[BreakablePlatform] 下落速度不够 ({fallSpeed:F2} < {minFallSpeed})，跳过");
            return;
        }

        // 全部通过，破碎！
        if (logHit) Debug.Log($"[BreakablePlatform] 凝固态从上方下落，速度={fallSpeed:F2}，破碎！");
        Destroy(gameObject);
    }
}
