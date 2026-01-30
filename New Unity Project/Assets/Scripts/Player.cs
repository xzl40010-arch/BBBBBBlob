//  郑佳鑫
// 2026.1.27：第一次修改，添加玩家脚本，记录出发点位置，死亡后重置，打印日志
//2026.1.28：添加玩家状态机，玩家只能完成流动态→气化态和流动态→凝固态两种操作，玩家操作的形态切换之间存在0.3s冷却

//许兆璘
//2026.1.29：添加了移动接口，修改玩家初始状态为Liquid

//阳成垚
//2026.1.30：实现尖刺、状态转换的音效播放

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

    [SerializeField] private PlayerState currentState = PlayerState.Liquid;
    public PlayerState CurrentState => currentState;

    [SerializeField] private float switchCooldownSeconds = 0.3f;
    private float lastPlayerSwitchTime = -999f;//记录上一次切换玩家角色的时间戳，初始值设为负数可以确保游戏刚启动时就满足冷却条件。

    private Vector3 spawnPoint; // 出发点位置

    AudioController audiocontroller;// 音效控制
    private void Start()
    {
        // 记录玩家的初始位置作为出发点
        spawnPoint = transform.position;
        Debug.Log($"玩家出发点已设置: {spawnPoint}");

        audiocontroller=GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioController>();
    }
    // 玩家死亡，重置到出发点
    public void Die()
    {
        // 播放尖刺音效
        audiocontroller.PlaySfx(audiocontroller.thronKillClip);

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
        // 播放“呲——”音效
        audiocontroller.PlaySfx(audiocontroller.sizzleClip);

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
        //播放“滋滋滋”音效
        audiocontroller.PlaySfx(audiocontroller.fizzClip);

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
        //播放转换音效
        audiocontroller.PlaySfx(audiocontroller.toGasClip);

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
        //播放转换音效
        audiocontroller.PlaySfx(audiocontroller.toSolidClip);

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
    public bool IsGrounded { get; private set; }

    // 设置地面状态（由Movement脚本调用）
    public void SetGrounded(bool grounded)
    {
        IsGrounded = grounded;
    }
}
