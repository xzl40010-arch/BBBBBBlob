//  郑佳鑫
// 2026.1.27：第一次修改，添加玩家脚本，记录出发点位置，死亡后重置，打印日志
//2026.1.28：添加玩家状态机，玩家只能完成流动态→气化态和流动态→凝固态两种操作，玩家操作的形态切换之间存在0.3s冷却
// 2026.2.2：增加调试日志，显示玩家状态切换情况
//2026.2.5：修复了状态切换时物理不同步的问题，增加记录存档功能
//许兆璘
//2026.1.29：添加了移动接口，修改玩家初始状态为Liquid

//阳成垚
//2026.1.30：实现尖刺、状态转换的音效播放

//xzl
//2026.2.3:修复了状态转化的音效bug
//2026.2.3:修复状态切换物理不同步的问题
//2026.2.5:彻底修复了三态变化的问题



using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    public enum PlayerState { Solid, Liquid, Gas }

    [SerializeField] private PlayerState currentState = PlayerState.Liquid;
    public PlayerState CurrentState => currentState;

    [Header("形态切换冷却")]
    [SerializeField] private float switchCooldown = 0.3f;
    private float lastSwitchTime = 0f;

    [Header("形态子对象引用")]
    [SerializeField] private GameObject liquidForm;
    [SerializeField] private GameObject gasForm;
    [SerializeField] private GameObject solidForm;

    
    [Header("形态切换特效")]
[SerializeField] private ParticleSystem solidSwitchVfx;
[SerializeField] private ParticleSystem liquidSwitchVfx;
[SerializeField] private ParticleSystem gasSwitchVfx;
    // 调试：记录切换次数
    private int switchCount = 0;


    [Header("出生点")]
    [SerializeField] private Transform spawnPoint;
    private Vector3 defaultSpawn;


    [Header("存档")]
    [SerializeField] private bool clearArchiveOnStart = true;

    private Rigidbody2D rb;
    private AudioController audioController;
    private bool isInitialized = false;

    void Awake()
    {
        if (clearArchiveOnStart)
        {
            ArchiveManager.Clear();
        }

        rb = GetComponent<Rigidbody2D>();

        GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
        if (audioObj != null)
        {
            audioController = audioObj.GetComponent<AudioController>();
        }


        ApplyFormVisibility(currentState);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))

        if (spawnPoint == null)

        {
            defaultSpawn = transform.position;
        }
        else
        {
            defaultSpawn = spawnPoint.position;
        }

    }

    void Start()
    {
        if (!isInitialized)
        {
            InitializePlayer();
        }
    }

    void InitializePlayer()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        ForceSetState(currentState);

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.simulated = true;
            rb.freezeRotation = true;
        }

        isInitialized = true;
    }

    // ========== 玩家主动切换方法 ==========

    public bool TrySwitchToGas()
    {
        return TrySwitchState(PlayerState.Gas, "气化");
    }

    public bool TrySwitchToSolid()
    {
        return TrySwitchState(PlayerState.Solid, "凝固");
    }

    private bool TrySwitchState(PlayerState newState, string stateName)
    {
        if (currentState != PlayerState.Liquid)
        {
            return false;
        }

        if (!IsSwitchReady())
        {
            return false;
        }

        if (SetState(newState))
        {
            lastSwitchTime = Time.time;

            if (audioController != null)
            {
                if (newState == PlayerState.Gas)
                    audioController.PlaySfx(audioController.toGasClip);
                else if (newState == PlayerState.Solid)
                    audioController.PlaySfx(audioController.toSolidClip);
            }

            return true;
        }

        return false;
    }

    // ========== 机关触发方法（新名称） ==========

    public bool ConvertGasToLiquid()
    {
        if (currentState != PlayerState.Gas) return false;

        if (SetState(PlayerState.Liquid))
        {
            if (audioController != null)
                audioController.PlaySfx(audioController.fizzClip);
            return true;
        }

        return false;
    }

    public bool ConvertSolidToLiquid()
    {
        if (currentState != PlayerState.Solid) return false;

        if (SetState(PlayerState.Liquid))
        {
            if (audioController != null)
                audioController.PlaySfx(audioController.sizzleClip);
            return true;
        }

        return false;
    }

    // ========== 机关触发方法（旧名称 - 兼容性） ==========

    public bool EnterLiquidFromGas()
    {
        Debug.Log("机关调用旧方法: EnterLiquidFromGas");
        return ConvertGasToLiquid();
    }

    public bool EnterLiquidFromSolid()
    {
        Debug.Log("机关调用旧方法: EnterLiquidFromSolid");
        return ConvertSolidToLiquid();
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
        if (audioController != null)
            audioController.PlaySfx(audioController.thronKillClip);


        Vector3 respawnPos;
        if (ArchiveManager.TryGetLatestPosition(out respawnPos))

        {
            Debug.Log($"[Player] Respawn at archive position: {respawnPos}");
            transform.position = respawnPos;
        }
        else
        {
            transform.position = spawnPoint != null ? spawnPoint.position : defaultSpawn;
            Debug.Log("[Player] Respawn at default spawn");
        }

        ForceSetState(PlayerState.Liquid);

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    // ========== 状态管理核心方法 ==========

    private bool SetState(PlayerState newState)
    {
        if (currentState == newState)
        {
            return false;
        }

        PlayerState oldState = currentState;
        currentState = newState;

        ApplyFormVisibility();

        SendMessage("OnPlayerStateChanged", currentState, SendMessageOptions.DontRequireReceiver);

        return true;
    }

    private void ForceSetState(PlayerState newState)
    {
        currentState = newState;
        ApplyFormVisibility();
        SendMessage("OnPlayerStateChanged", currentState, SendMessageOptions.DontRequireReceiver);
    }

    private void ApplyFormVisibility()
    {
        if (liquidForm == null || gasForm == null || solidForm == null)
        {
            return;
        }

        liquidForm.SetActive(currentState == PlayerState.Liquid);
        gasForm.SetActive(currentState == PlayerState.Gas);
        solidForm.SetActive(currentState == PlayerState.Solid);
    }

    // ========== 辅助方法 ==========

    private bool IsSwitchReady()
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

        //状态改变时：播一次切换特效
        PlaySwitchVfx(newState);

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

    //2.5 文振一 调用特效
private void PlaySwitchVfx(PlayerState newState)
{
    ParticleSystem ps = null;

    switch (newState)
    {
        case PlayerState.Solid:
            ps = solidSwitchVfx;
            break;
        case PlayerState.Liquid:
            ps = liquidSwitchVfx;
            break;
        case PlayerState.Gas:
            ps = gasSwitchVfx;
            break;
    }

    if (ps == null) return;

    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    ps.Play(true);
}



    public bool IsGrounded { get; private set; }

    public void SetGrounded(bool grounded)
    {
        IsGrounded = grounded;
    }

}