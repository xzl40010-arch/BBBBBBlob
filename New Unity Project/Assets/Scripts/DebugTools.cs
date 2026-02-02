//xzl
//2026.2.3:添加测试工具方便进行debug，无实际表现作用
using UnityEngine;

public class DebugTools : MonoBehaviour
{
    [Header("调试设置")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.F3;

    private Player player;
    private PlayerMovement movement;

    void Start()
    {
        player = GetComponent<Player>();
        movement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            showDebugInfo = !showDebugInfo;
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            LogCurrentState();
        }
    }

    void OnGUI()
    {
        if (!showDebugInfo || player == null) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("=== 调试信息 ===");
        GUILayout.Label($"帧: {Time.frameCount}");
        GUILayout.Label($"时间: {Time.time:F2}");
        GUILayout.Label($"状态: {player.CurrentState}");
        GUILayout.Label($"速度: {GetComponent<Rigidbody2D>()?.velocity}");
        GUILayout.Label($"重力: {GetComponent<Rigidbody2D>()?.gravityScale}");
        GUILayout.Label($"地面: {player.IsGrounded}");
        GUILayout.EndArea();
    }

    private void LogCurrentState()
    {
        Debug.Log($"=== 当前状态快照 ===");
        Debug.Log($"帧: {Time.frameCount}");
        Debug.Log($"时间: {Time.time:F2}");
        Debug.Log($"玩家状态: {player?.CurrentState}");
        Debug.Log($"速度: {GetComponent<Rigidbody2D>()?.velocity}");
        Debug.Log($"重力: {GetComponent<Rigidbody2D>()?.gravityScale}");
    }
}