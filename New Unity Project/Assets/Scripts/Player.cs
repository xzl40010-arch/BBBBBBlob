//  郑佳鑫
// 2026.1.27：第一次修改，添加玩家脚本，记录出发点位置，死亡后重置，打印日志
//2026.1.28：添加玩家状态机，玩家只能完成流动态→气化态和流动态→凝固态两种操作，玩家操作的形态切换之间存在0.3s冷却
// 2026.2.2：增加调试日志，显示玩家状态切换情况

//许兆璘
//2026.1.29：添加了移动接口，修改玩家初始状态为Liquid

//阳成垚
//2026.1.30：实现尖刺、状态转换的音效播放

//xzl
//2026.2.3:修复了状态转化的音效bug
//2026.2.3:修复状态切换物理不同步的问题


using UnityEngine;

public class Player : MonoBehaviour
{
    public enum PlayerState { Solid, Liquid, Gas }

    [SerializeField] private PlayerState currentState = PlayerState.Liquid;
    public PlayerState CurrentState => currentState;

    [SerializeField] private float switchCooldownSeconds = 0.3f;
    private float lastPlayerSwitchTime = -999f;
    private Vector3 spawnPoint;

    private AudioController audioController;
    private bool audioControllerFound = false;
    private PlayerMovement playerMovement;

    [Header("形态GameObject")]
    [SerializeField] private GameObject liquidForm;
    [SerializeField] private GameObject gasForm;
    [SerializeField] private GameObject solidForm;

    // 调试：记录切换次数
    private int switchCount = 0;

    void Start()
    {
        spawnPoint = transform.position;
        Debug.Log($"玩家出发点: {spawnPoint}, 初始状态: {currentState}");

        // 获取PlayerMovement组件
        playerMovement = GetComponent<PlayerMovement>();

        if (playerMovement == null)
        {
            Debug.LogWarning("Player: 找不到PlayerMovement组件");
        }

        // 安全获取音频控制器
        GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
        if (audioObj != null)
        {
            audioController = audioObj.GetComponent<AudioController>();
            audioControllerFound = audioController != null;
        }

        ApplyFormVisibility(currentState);
    }

    void Update()
    {
        // 调试：按空格显示当前状态
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log($"[帧{Time.frameCount}] 当前状态: {currentState}, 切换次数: {switchCount}");
        }
    }

    // ========== 机关触发的方法（必须保留） ==========

    public bool EnterLiquidFromGas()
    {
        Debug.Log($"机关触发: 从气态变为流动态, 当前状态={currentState}");

        if (currentState != PlayerState.Gas)
        {
            Debug.LogWarning($"切换失败: 当前状态不是气态 ({currentState})");
            return false;
        }

        if (SetState(PlayerState.Liquid))
        {
            if (audioControllerFound) audioController.PlaySfx(audioController.fizzClip);
            return true;
        }

        return false;
    }

    public bool EnterLiquidFromSolid()
    {
        Debug.Log($"机关触发: 从固态变为流动态, 当前状态={currentState}");

        if (currentState != PlayerState.Solid)
        {
            Debug.LogWarning($"切换失败: 当前状态不是固态 ({currentState})");
            return false;
        }

        if (SetState(PlayerState.Liquid))
        {
            if (audioControllerFound) audioController.PlaySfx(audioController.sizzleClip);
            return true;
        }

        return false;
    }

    // ========== 玩家主动切换的方法 ==========

    public bool TrySwitchToGasFromLiquid()
    {
        Debug.Log($"尝试切换到气态: 当前状态={currentState}, 冷却={IsPlayerSwitchReady()}");

        // 检查是否在流动态且冷却结束
        if (currentState != PlayerState.Liquid)
        {
            Debug.LogWarning($"切换失败: 当前状态不是流动态 ({currentState})");
            return false;
        }

        if (!IsPlayerSwitchReady())
        {
            Debug.LogWarning($"切换失败: 冷却中 (上次切换: {lastPlayerSwitchTime})");
            return false;
        }

        // 执行切换
        if (SetState(PlayerState.Gas))
        {
            lastPlayerSwitchTime = Time.time;
            if (audioControllerFound) audioController.PlaySfx(audioController.toGasClip);
            switchCount++;
            Debug.Log($"切换到气态成功! 切换次数: {switchCount}");
            return true;
        }

        return false;
    }

    public bool TrySwitchToSolidFromLiquid()
    {
        Debug.Log($"尝试切换到固态: 当前状态={currentState}, 冷却={IsPlayerSwitchReady()}");

        if (currentState != PlayerState.Liquid)
        {
            Debug.LogWarning($"切换失败: 当前状态不是流动态 ({currentState})");
            return false;
        }

        if (!IsPlayerSwitchReady())
        {
            Debug.LogWarning($"切换失败: 冷却中 (上次切换: {lastPlayerSwitchTime})");
            return false;
        }

        // 执行切换
        if (SetState(PlayerState.Solid))
        {
            lastPlayerSwitchTime = Time.time;
            if (audioControllerFound) audioController.PlaySfx(audioController.toSolidClip);
            switchCount++;
            Debug.Log($"切换到固态成功! 切换次数: {switchCount}");
            return true;
        }

        return false;
    }

    // ========== 死亡和重置 ==========

    public void Die()
    {
        if (audioControllerFound) audioController.PlaySfx(audioController.thronKillClip);
        Debug.Log("玩家死亡，重置到出发点");
        transform.position = spawnPoint;
        ForceSetState(PlayerState.Liquid);
    }

    private bool ForceSetState(PlayerState newState)
    {
        PlayerState oldState = currentState;
        currentState = newState;
        ApplyFormVisibility(currentState);

        // 立即通知PlayerMovement更新物理
        if (playerMovement != null)
        {
            playerMovement.ForceUpdatePhysics();
        }

        Debug.Log($"强制状态设置: {oldState} -> {currentState}");
        return true;
    }

    // ========== 辅助方法 ==========

    private bool IsPlayerSwitchReady()
    {
        float timeSinceLastSwitch = Time.time - lastPlayerSwitchTime;
        bool ready = timeSinceLastSwitch >= switchCooldownSeconds;
        return ready;
    }

    private bool SetState(PlayerState newState)
    {
        // 检查是否相同状态
        if (currentState == newState)
        {
            Debug.LogWarning($"SetState: 状态未改变 ({currentState} -> {newState})");
            return false;
        }

        PlayerState oldState = currentState;
        currentState = newState;

        ApplyFormVisibility(currentState);
        Debug.Log($"SetState成功: {oldState} -> {currentState}");

        // 立即通知PlayerMovement更新物理
        if (playerMovement != null)
        {
            playerMovement.ForceUpdatePhysics();
        }

        return true;
    }

    private void ApplyFormVisibility(PlayerState state)
    {
        // 确保只切换子对象的可见性，不切换父对象
        if (liquidForm != null) liquidForm.SetActive(state == PlayerState.Liquid);
        if (gasForm != null) gasForm.SetActive(state == PlayerState.Gas);
        if (solidForm != null) solidForm.SetActive(state == PlayerState.Solid);

        Debug.Log($"ApplyFormVisibility: 设置形态可见性 - Liquid: {state == PlayerState.Liquid}, Gas: {state == PlayerState.Gas}, Solid: {state == PlayerState.Solid}");
    }

    public bool IsGrounded { get; private set; }
    public void SetGrounded(bool grounded) => IsGrounded = grounded;
}