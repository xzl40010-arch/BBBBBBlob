//郑佳鑫
//2026.1.28：第一次修改，玩家只能完成流动态→气化态和流动态→凝固态两种操作，分别由鼠标左右键控制，无法主动完成其他形态变化

using UnityEngine;

// 玩家形态切换输入：左键->气态，右键->凝固态
[RequireComponent(typeof(Player))]
public class PlayerStateInput : MonoBehaviour
{
    private Player player;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void Update()
    {
        if (player == null)
        {
            Debug.LogWarning("[PlayerStateInput] Player component missing");
            return;
        }

        bool leftClick = Input.GetMouseButtonDown(0) || Input.GetButtonDown("Fire1");
        bool rightClick = Input.GetMouseButtonDown(1) || Input.GetButtonDown("Fire2");

        // 鼠标左键：流态 -> 气态
        if (leftClick)
        {
            bool switched = player.TrySwitchToGasFromLiquid();
            Debug.Log($"[PlayerStateInput] Left input, switch to Gas: {switched}");
        }

        // 鼠标右键：流态 -> 凝固态
        if (rightClick)
        {
            bool switched = player.TrySwitchToSolidFromLiquid();
            Debug.Log($"[PlayerStateInput] Right input, switch to Solid: {switched}");
        }
    }
}
