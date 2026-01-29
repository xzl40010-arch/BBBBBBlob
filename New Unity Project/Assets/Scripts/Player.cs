//  郑佳鑫
// 2026.1.27：第一次修改，添加玩家脚本，记录出发点位置，死亡后重置，打印日志
//2026.1.28：添加玩家状态机，玩家只能完成流动态→气化态和流动态→凝固态两种操作，玩家操作的形态切换之间存在0.3s冷却

using UnityEngine;
//玩家脚本 - 记录出发点位置，死亡后重置
public class Player : MonoBehaviour
{
    public enum PlayerState
    {
        Solid,
        Liquid,
        Gas
    }

    [SerializeField] private PlayerState currentState = PlayerState.Solid;
    public PlayerState CurrentState => currentState;

    [SerializeField] private float switchCooldownSeconds = 0.3f;
    private float lastPlayerSwitchTime = -999f;//记录上一次切换玩家角色的时间戳，初始值设为负数可以确保游戏刚启动时就满足冷却条件。

    private Vector3 spawnPoint; // 出发点位置
    private void Start()
    {
        // 记录玩家的初始位置作为出发点
        spawnPoint = transform.position;
        Debug.Log($"玩家出发点已设置: {spawnPoint}");
    }
    // 玩家死亡，重置到出发点
    public void Die()
    {
        Debug.Log("玩家触碰尖刺！回到出发点");
        transform.position = spawnPoint;
    }

    public bool EnterLiquidFromSolid()
    {
        // 仅在凝固态时变为流态（非玩家操作）
        if (currentState != PlayerState.Solid)
        {
            return false;
        }

        SetState(PlayerState.Liquid);
        return true;
    }

    public bool EnterLiquidFromGas()
    {
        // 仅在气化态时变为流态（非玩家操作）
        if (currentState != PlayerState.Gas)
        {
            return false;
        }

        SetState(PlayerState.Liquid);
        return true;
    }

    public bool TrySwitchToGasFromLiquid()
    {
        // 玩家操作：流态->气态，带冷却
        if (currentState != PlayerState.Liquid || !IsPlayerSwitchReady())
        {
            return false;
        }

        SetState(PlayerState.Gas);
        lastPlayerSwitchTime = Time.time;
        return true;
    }

    public bool TrySwitchToSolidFromLiquid()
    {
        // 玩家操作：流态->凝固态，带冷却
        if (currentState != PlayerState.Liquid || !IsPlayerSwitchReady())
        {
            return false;
        }

        SetState(PlayerState.Solid);
        lastPlayerSwitchTime = Time.time;
        return true;
    }

    private bool IsPlayerSwitchReady()
    {
        return Time.time - lastPlayerSwitchTime >= switchCooldownSeconds;
    }

    private void SetState(PlayerState newState)
    {
        // 忽略重复切换
        if (currentState == newState)
        {
            return;
        }

        currentState = newState;
        Debug.Log($"玩家状态切换为: {currentState}");
    }
}
