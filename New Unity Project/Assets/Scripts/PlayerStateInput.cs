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
    private bool inputLocked = false;
    private float inputLockTime = 0.1f; // 输入锁定时间

    void Awake()
    {
        player = GetComponent<Player>();
    }

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("PlayerStateInput: 找不到Player组件");
            enabled = false;
        }
    }

    void Update()
    {
        if (player == null || inputLocked) return;

        // 只在流动态时处理切换输入
        if (player.CurrentState != Player.PlayerState.Liquid)
        {
            // 如果不在流动态，显示提示
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                Debug.Log("不在流动态，无法切换形态");
            }
            return;
        }

        bool leftClick = Input.GetMouseButtonDown(0);
        bool rightClick = Input.GetMouseButtonDown(1);

        if (leftClick)
        {
            Debug.Log($"帧{Time.frameCount}: 检测到鼠标左键点击");
            bool switched = player.TrySwitchToGasFromLiquid();

            if (switched)
            {
                // 成功切换后锁定输入一小段时间
                StartCoroutine(LockInput());
            }
        }
        else if (rightClick)
        {
            Debug.Log($"帧{Time.frameCount}: 检测到鼠标右键点击");
            bool switched = player.TrySwitchToSolidFromLiquid();

            if (switched)
            {
                StartCoroutine(LockInput());
            }
        }

    }

    System.Collections.IEnumerator LockInput()
    {
        inputLocked = true;
        yield return new WaitForSeconds(inputLockTime);
        inputLocked = false;
    }
}