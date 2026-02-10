//郑佳鑫
//2026.2.10 第一次修改：气态可穿过平台：根据玩家当前形态决定是否忽略碰撞，气态时可穿过，其他形态阻挡
using UnityEngine;

// 平台：气态可穿过，其他形态阻挡
[RequireComponent(typeof(Collider2D))]
public class GasPassPlatform : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private bool autoFindPlayer = true;
    [SerializeField] private bool logStateChanges = false;

    private Collider2D platformCollider;
    private Collider2D[] playerColliders = new Collider2D[0];
    private Player.PlayerState lastState;
    private bool hasState;

    private void Awake()
    {
        platformCollider = GetComponent<Collider2D>();
        if (platformCollider != null && platformCollider.isTrigger)
        {
            platformCollider.isTrigger = false;
            Debug.LogWarning("[GasPassPlatform] Collider was Trigger, forced to solid");
        }
    }

    private void Start()
    {
        if (autoFindPlayer && player == null)
        {
            player = FindObjectOfType<Player>();
        }

        CachePlayerColliders();
        ApplyState(true);
    }

    private void Update()
    {
        if (player == null)
            return;

        if (!hasState || player.CurrentState != lastState)
        {
            ApplyState(false);
        }
    }

    private void OnEnable()
    {
        CachePlayerColliders();
        ApplyState(true);
    }

    private void OnDisable()
    {
        // 退出时恢复碰撞，避免保持忽略状态
        if (platformCollider == null || playerColliders == null)
            return;

        for (int i = 0; i < playerColliders.Length; i++)
        {
            Collider2D col = playerColliders[i];
            if (col == null) continue;
            Physics2D.IgnoreCollision(platformCollider, col, false);
        }
    }

    private void CachePlayerColliders()
    {
        if (player == null)
        {
            playerColliders = new Collider2D[0];
            return;
        }

        playerColliders = player.GetComponentsInChildren<Collider2D>(true);
    }

    private void ApplyState(bool force)
    {
        if (player == null || platformCollider == null)
            return;

        if (!force && hasState && player.CurrentState == lastState)
            return;

        lastState = player.CurrentState;
        hasState = true;

        CachePlayerColliders();
        bool ignore = (player.CurrentState == Player.PlayerState.Gas);

        for (int i = 0; i < playerColliders.Length; i++)
        {
            Collider2D col = playerColliders[i];
            if (col == null) continue;
            Physics2D.IgnoreCollision(platformCollider, col, ignore);
        }

        if (logStateChanges)
        {
            Debug.Log($"[GasPassPlatform] ignore={ignore}, state={player.CurrentState}");
        }
    }
}

