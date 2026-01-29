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
        // 鼠标左键：流态 -> 气态
        if (Input.GetMouseButtonDown(0))
        {
            player.TrySwitchToGasFromLiquid();
        }

        // 鼠标右键：流态 -> 凝固态
        if (Input.GetMouseButtonDown(1))
        {
            player.TrySwitchToSolidFromLiquid();
        }
    }
}
