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

    [Header("当前状态")]
    [SerializeField] private PlayerState currentState = PlayerState.Liquid;
    public PlayerState CurrentState => currentState;

    [Header("形态切换冷却")]
    [SerializeField] private float switchCooldown = 0.3f;
    private float lastSwitchTime = -999f;

    [Header("形态子对象引用")]
    [SerializeField] private GameObject liquidForm;
    [SerializeField] private GameObject gasForm;
    [SerializeField] private GameObject solidForm;

    [Header("形态切换特效")]
    [SerializeField] private ParticleSystem solidSwitchVfx;
    [SerializeField] private ParticleSystem liquidSwitchVfx;
    [SerializeField] private ParticleSystem gasSwitchVfx;

    [Header("出生点")]
    [SerializeField] private Transform spawnPoint;

    [Header("存档")]
    [SerializeField] private bool clearArchiveOnStart = true;

    private Vector3 defaultSpawn;
    private Rigidbody2D rb;
    private AudioController audioController;
    private PlayerMovement playerMovement;

    private bool isInitialized = false;

    void Awake()
    {
        if (clearArchiveOnStart)
        {
            ArchiveManager.Clear();
        }

        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();

        GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
        if (audioObj != null)
        {
            audioController = audioObj.GetComponent<AudioController>();
        }

        // 记录默认出生点
        defaultSpawn = (spawnPoint != null) ? spawnPoint.position : transform.position;

        ApplyFormVisibility(currentState);
    }

    void Start()
    {
        if (!isInitialized)
        {
            InitializePlayer();
        }
    }

    void Update()
    {
        // 鼠标左键：切到气态
        if (Input.GetMouseButtonDown(0))
        {
            TrySwitchToGas();
        }

        // 鼠标右键：切到固态
        if (Input.GetMouseButtonDown(1))
        {
            TrySwitchToSolid();
        }

        // 可选：按空格刷新默认出生点（你原来逻辑是断的，这里补成可用版）
        if (Input.GetKeyDown(KeyCode.Space))
        {
            defaultSpawn = (spawnPoint != null) ? spawnPoint.position : transform.position;
        }
    }

    private void InitializePlayer()
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

    // ========== 玩家主动切换方法（主干接口） ==========

    public bool TrySwitchToGas()
    {
        return TrySwitchState(PlayerState.Gas);
    }

    public bool TrySwitchToSolid()
    {
        return TrySwitchState(PlayerState.Solid);
    }

    private bool TrySwitchState(PlayerState newState)
    {
        // 只能从 Liquid 切换
        if (currentState != PlayerState.Liquid)
            return false;

        if (!IsSwitchReady())
            return false;

        if (SetState(newState))
        {
            // 播放音效
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

    // ========== 机关触发方法（回到 Liquid） ==========

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

    // 旧名称兼容（如果场景里有老机关脚本还在调用）
    public bool EnterLiquidFromGas()
    {
        return ConvertGasToLiquid();
    }

    public bool EnterLiquidFromSolid()
    {
        return ConvertSolidToLiquid();
    }

    // ========== 死亡与重置 ==========

    public void Die()
    {
        if (audioController != null)
            audioController.PlaySfx(audioController.thronKillClip);

        Vector3 respawnPos;
        if (ArchiveManager.TryGetLatestPosition(out respawnPos))
        {
            transform.position = respawnPos;
        }
        else
        {
            transform.position = (spawnPoint != null) ? spawnPoint.position : defaultSpawn;
        }

        ForceSetState(PlayerState.Liquid);

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    // ========== 状态管理核心 ==========

    private bool IsSwitchReady()
    {
        return Time.time - lastSwitchTime >= switchCooldown;
    }

    private bool SetState(PlayerState newState)
    {
        if (currentState == newState)
            return false;

        PlayerState oldState = currentState;
        currentState = newState;

        lastSwitchTime = Time.time;

        PlaySwitchVfx(newState);
        ApplyFormVisibility(currentState);

        SendMessage("OnPlayerStateChanged", currentState, SendMessageOptions.DontRequireReceiver);

        if (playerMovement != null)
        {
            playerMovement.ForceUpdatePhysics();
        }

        Debug.Log($"[Player] State: {oldState} -> {currentState}");
        return true;
    }

    private void ForceSetState(PlayerState newState)
    {
        currentState = newState;
        ApplyFormVisibility(currentState);
        SendMessage("OnPlayerStateChanged", currentState, SendMessageOptions.DontRequireReceiver);

        if (playerMovement != null)
        {
            playerMovement.ForceUpdatePhysics();
        }
    }

    private void ApplyFormVisibility(PlayerState state)
    {
        if (liquidForm != null) liquidForm.SetActive(state == PlayerState.Liquid);
        if (gasForm != null) gasForm.SetActive(state == PlayerState.Gas);
        if (solidForm != null) solidForm.SetActive(state == PlayerState.Solid);
    }

    // ========== 特效 ==========

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

    // ========== 其他 ==========

    public bool IsGrounded { get; private set; }

    public void SetGrounded(bool grounded)
    {
        IsGrounded = grounded;
    }
}
