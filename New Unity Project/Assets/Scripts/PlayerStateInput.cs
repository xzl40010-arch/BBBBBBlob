//郑佳鑫
//2026.1.28：第一次修改，玩家只能完成流动态→气化态和流动态→凝固态两种操作，分别由鼠标左右键控制，无法主动完成其他形态变化

//xzl
//2026.2.3:修复了玩家切换错误的bug
//2026.2.3:修复协程启动问题

//2026.2.5 添加测试模块


using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerStateInput : MonoBehaviour
{
    private Player player;

    [Header("输入设置")]
    [SerializeField] private KeyCode gasKey = KeyCode.Mouse0;     // 鼠标左键
    [SerializeField] private KeyCode solidKey = KeyCode.Mouse1;   // 鼠标右键
    [SerializeField] private KeyCode respawnKey = KeyCode.R;      // 重置位置
    [SerializeField] private KeyCode suicideKey = KeyCode.T;      // 自杀重置

    [Header("输入冷却")]
    [SerializeField] private float inputCooldown = 0.1f;
    private float lastInputTime = 0f;

    void Awake()
    {
        player = GetComponent<Player>();

        if (player == null)
        {
            Debug.LogError("PlayerStateInput: 找不到Player组件!");
            enabled = false;
        }
    }

    void Update()
    {
        // 输入冷却检查
        if (Time.time - lastInputTime < inputCooldown)
            return;

        // 只在流动态时允许形态切换
        if (player.CurrentState == Player.PlayerState.Liquid)
        {
            HandleMorphInput();
        }
        else
        {
            // 如果不是流动态，按切换键给出提示
            if (Input.GetKeyDown(gasKey) || Input.GetKeyDown(solidKey))
            {
                Debug.Log("当前不是流动态，无法切换形态");
            }
        }

        // 功能键（不受状态限制）
        HandleFunctionKeys();
    }

    void HandleMorphInput()
    {
        bool switchAttempted = false;

        if (Input.GetKeyDown(gasKey))
        {
            Debug.Log($"切换气态输入: 帧{Time.frameCount}");
            switchAttempted = player.TrySwitchToGas();
        }
        else if (Input.GetKeyDown(solidKey))
        {
            Debug.Log($"切换固态输入: 帧{Time.frameCount}");
            switchAttempted = player.TrySwitchToSolid();
        }

        if (switchAttempted)
        {
            lastInputTime = Time.time;
        }

    }

    void HandleFunctionKeys()
    {
        if (Input.GetKeyDown(respawnKey))
        {
            Debug.Log("重置到出生点");
            player.transform.position = Vector3.zero; // 暂时，实际应该有存档点
        }

        if (Input.GetKeyDown(suicideKey))
        {
            Debug.Log("自杀重置");
            player.Die();
        }
    }

    void OnGUI()
    {
        // 简单的输入提示
        GUILayout.BeginArea(new Rect(10, Screen.height - 100, 300, 100));
        GUILayout.Label("=== 操作说明 ===");
        GUILayout.Label($"当前状态: {player.CurrentState}");
        GUILayout.Label("鼠标左键: 切换到气态");
        GUILayout.Label("鼠标右键: 切换到固态");
        GUILayout.Label("R键: 回到出生点");
        GUILayout.Label("T键: 自杀重置");
        GUILayout.EndArea();
    }
}