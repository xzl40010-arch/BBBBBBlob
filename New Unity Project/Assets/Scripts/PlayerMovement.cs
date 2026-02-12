//2026.1.29:许兆璘
//玩家根据状态进行移动控制

//xzl
//2026.2.3:修复了气态模式下无重力上升的bug
//2026.2.3:添加ForceUpdatePhysics确保状态切换时物理属性更新
//2026.2.5:修复气态天花板碰撞

//ycy
//2026.2.10：添加三态移动音效，下落音效；待改气体移动音效
//2026.2.12：优化移动音效性能，修复卡顿问题

//郑佳鑫
//2026.2.10：添加外部速度锁定功能，供其他脚本调用（如跳跃脚本）以暂时禁止水平输入影响速度

using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerMovement : MonoBehaviour
{
    [Header("移动参数")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float deceleration = 20f;

    [Header("状态物理属性")]
    [SerializeField] private float liquidGravity = 15f;
    [SerializeField] private float solidGravity = 20f;
    [SerializeField] private float gasGravity = -8f;  // 负值表示向上漂浮

    [Header("气态特殊参数")]
    [SerializeField] private float gasFloatSpeed = 3f;        // 基础漂浮速度
    [SerializeField] private float gasMaxFloatSpeed = 5f;     // 最大漂浮速度
    [SerializeField] private bool gasBounceFromCeiling = true; // 是否从天花板反弹
    [SerializeField] private float gasBounceFactor = 0.5f;    // 反弹系数

    [Header("台阶攀爬")]
    [SerializeField] private float stepHeight = 0.5f;
    [SerializeField] private float stepCheckDistance = 0.5f;
    [SerializeField] private LayerMask groundLayer;

    [Header("天花板检测")]
    [SerializeField] private float ceilingCheckDistance = 0.5f;
    [SerializeField] private bool showCeilingDebug = true;

    [Header("物理材质")]
    [SerializeField] private PhysicsMaterial2D liquidPhysicsMaterial;
    [SerializeField] private PhysicsMaterial2D solidPhysicsMaterial;
    [SerializeField] private PhysicsMaterial2D gasPhysicsMaterial;

    [Header("Physics Tuning")]
    [SerializeField] private bool useContinuousCollision = true;
    [SerializeField] private bool useInterpolation = true;
    [SerializeField] private bool neverSleep = true;

    // ==========  移动音效参数 ==========
    [Header("移动音效设置")]
    [SerializeField] private float minSpeedForSound = 0.8f;      // 触发移动音效的最小速度
    [SerializeField] private float moveSoundCheckInterval = 0.1f; // 音效检测间隔（优化性能）

    // 组件引用
    private Rigidbody2D rb;
    private Collider2D col;
    private Player player;

    // 移动状态
    private float horizontalInput;
    private float targetVelocityX;
    private float currentVelocityX;
    private bool isGrounded;
    private bool isTouchingCeiling;

    // 状态缓存
    private Player.PlayerState lastAppliedState;
    private Player.PlayerState previousState;

    // 时间控制
    private float gasSwitchTime = 0f;
    private float externalVelocityLockTimer;

    // ==========  音效相关变量 ==========
    private AudioController audioController;
    private bool wasMoving = false;
    private bool wasGrounded = false;
    private float lastVerticalVelocity = 0f;
    private float moveSoundCheckTimer = 0f;
    
    // 缓存音效文件
    private AudioClip cachedSolidMoveClip;
    private AudioClip cachedLiquidMoveClip;
    private AudioClip cachedGasMoveClip;
    private AudioClip cachedSolidFallClip;
    private AudioClip cachedLiquidFallClip;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        player = GetComponent<Player>();

        // ==========  获取AudioController并缓存音效 ==========
        GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
        if (audioObj != null)
        {
            audioController = audioObj.GetComponent<AudioController>();
            
            if (audioController != null)
            {
                cachedSolidMoveClip = audioController.solidMoveClip;
                cachedLiquidMoveClip = audioController.liquidMoveClip;
                cachedGasMoveClip = audioController.gasMoveClip;
                cachedSolidFallClip = audioController.solidFallClip;
                cachedLiquidFallClip = audioController.liquidFallClip;
            }
        }

        if (player != null)
        {
            lastAppliedState = player.CurrentState;
            previousState = player.CurrentState;
            ApplyPhysicsForState(lastAppliedState, true);
            Debug.Log($"PlayerMovement初始化: 状态={lastAppliedState}");
        }
        else
        {
            Debug.LogError("PlayerMovement: 找不到Player组件!");
        }
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // 检测状态变化
        if (player != null && player.CurrentState != lastAppliedState)
        {
            Debug.Log($"PlayerMovement检测到状态变化: {lastAppliedState} -> {player.CurrentState}");

            // 记录气态切换时间
            if (player.CurrentState == Player.PlayerState.Gas)
            {
                gasSwitchTime = Time.time;
            }

                // ========== 形态切换时，强制更新移动音效 ==========
            if (audioController != null)
                {
                    // 获取新形态的音效
                    AudioClip newMoveClip = GetMoveClipForCurrentState();
                    if (newMoveClip != null)
                    {   
                        // 再播放新音效
                        audioController.StopMoveSoundImmediate();
                        audioController.PlayMoveSound(newMoveClip, true);
                        Debug.Log($"形态切换，强制更新移动音效: {lastAppliedState} -> {player.CurrentState}");
                    }
                }
                // ====================================================

            ApplyPhysicsForState(player.CurrentState, true);
            previousState = lastAppliedState;
            lastAppliedState = player.CurrentState;
        }

        // 调试信息
        if (Input.GetKeyDown(KeyCode.F2))
        {
            Debug.Log($"移动状态 - 状态: {player?.CurrentState}, 速度: {rb?.velocity}, 接地: {isGrounded}, 天花板: {isTouchingCeiling}");
        }
    }

    void FixedUpdate()
    {
        if (player == null || rb == null) return;

        if (externalVelocityLockTimer > 0f)
        {
            externalVelocityLockTimer -= Time.fixedDeltaTime;
        }

        // 检测地面和天花板
        CheckGround();
        CheckCeiling();

        // ==========  间隔检测音效，避免每帧调用 ==========
        moveSoundCheckTimer += Time.fixedDeltaTime;
        if (moveSoundCheckTimer >= moveSoundCheckInterval)
        {
            HandleMovementSound();
            moveSoundCheckTimer = 0f;
        }

        // 根据状态处理移动
        switch (player.CurrentState)
        {
            case Player.PlayerState.Liquid:
                HandleLiquidMovement();
                break;
            case Player.PlayerState.Solid:
                HandleSolidMovement();
                break;
            case Player.PlayerState.Gas:
                HandleGasMovement();
                break;
        }

        // 确保物理效果正确应用
        if (player.CurrentState != lastAppliedState)
        {
            ApplyPhysicsForState(player.CurrentState, true);
            lastAppliedState = player.CurrentState;
        }

        // ========== 记录垂直速度用于落地检测 ==========
        lastVerticalVelocity = rb.velocity.y;
    }

    // ========== 状态物理应用 ==========

    private void ApplyPhysicsForState(Player.PlayerState state, bool forceApply = false)
    {
        if (rb == null) return;

        if (useContinuousCollision)
        {
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        if (useInterpolation)
        {
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        if (neverSleep)
        {
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        }

        // 应用对应的物理材质
        if (col != null)
        {
            switch (state)
            {
                case Player.PlayerState.Liquid:
                    col.sharedMaterial = liquidPhysicsMaterial;
                    break;
                case Player.PlayerState.Solid:
                    col.sharedMaterial = solidPhysicsMaterial;
                    break;
                case Player.PlayerState.Gas:
                    col.sharedMaterial = gasPhysicsMaterial;
                    break;
            }
        }

        switch (state)
        {
            case Player.PlayerState.Liquid:
                rb.gravityScale = liquidGravity;
                rb.mass = 1f;
                if (forceApply) Debug.Log($"应用液态物理: 重力={liquidGravity}");
                break;

            case Player.PlayerState.Solid:
                rb.gravityScale = solidGravity;
                rb.mass = 2f; // 较重
                if (forceApply) Debug.Log($"应用固态物理: 重力={solidGravity}, 质量=2");
                break;

            case Player.PlayerState.Gas:
                rb.gravityScale = gasGravity;
                rb.mass = 0.5f; // 较轻
                if (forceApply) Debug.Log($"应用气态物理: 重力={gasGravity}, 质量=0.5");

                // 切换为气态时，给予初始上升速度
                if (forceApply)
                {
                    Vector2 velocity = rb.velocity;
                    velocity.y = Mathf.Max(gasFloatSpeed, velocity.y);
                    rb.velocity = velocity;
                    if (showCeilingDebug) Debug.Log($"气态切换: 设置上升速度 {velocity.y}");
                }
                break;
        }
    }

    // ==========  移动音效处理 ==========

    private void HandleMovementSound()
    {
        if (audioController == null) return;
        if (player == null) return;

        // 检查是否在移动
        bool isMoving = CheckIfMoving();
        bool isGroundedNow = isGrounded && player.CurrentState != Player.PlayerState.Gas;
        
        // 根据状态选择对应的移动音效（使用缓存）
        AudioClip moveClip = null;
        AudioClip fallClip = null;
        
        switch (player.CurrentState)
        {
            case Player.PlayerState.Gas:
                moveClip = cachedGasMoveClip;
                break;
            case Player.PlayerState.Solid:
                moveClip = cachedSolidMoveClip;
                fallClip = cachedSolidFallClip;
                break;
            case Player.PlayerState.Liquid:
                moveClip = cachedLiquidMoveClip;
                fallClip = cachedLiquidFallClip;
                break;
        }
        
        if (moveClip == null) return;
        
        // 状态变化：开始移动
        if (isMoving && !wasMoving)
        {
            audioController.PlayMoveSound(moveClip, true);
        }
        // 状态变化：停止移动
        else if (!isMoving && wasMoving)
        {
            audioController.StopMoveSound();
        }
        // 持续移动但音效已结束
        else if (isMoving && !audioController.IsMoveSoundPlaying())
        {
            audioController.PlayMoveSound(moveClip, true);
        }
        
        // 落地音效（仅固态和流动态）
        if (player.CurrentState != Player.PlayerState.Gas && fallClip != null)
        {
            if (isGroundedNow && !wasGrounded)
            {
                float fallSpeed = Mathf.Abs(lastVerticalVelocity);
                if (fallSpeed > 3f)
                {
                    audioController.PlaySfx(fallClip);
                }
            }
        }
        
        // 更新状态
        wasMoving = isMoving;
        wasGrounded = isGroundedNow;
    }

    private bool CheckIfMoving()
    {
        if (rb == null) return false;
        
        float currentSpeed = rb.velocity.magnitude;
        bool hasHorizontalInput = Mathf.Abs(horizontalInput) > 0.1f;
        
        // 不同状态有不同的移动判断标准
        switch (player.CurrentState)
        {
            case Player.PlayerState.Solid:
                return currentSpeed > minSpeedForSound && hasHorizontalInput;
                
            case Player.PlayerState.Liquid:
                return currentSpeed > minSpeedForSound * 0.7f && hasHorizontalInput;
                
            case Player.PlayerState.Gas:
                return currentSpeed > minSpeedForSound * 0.5f;
                
            default:
                return false;
        }
    }

        // ==========  获取当前状态的移动音效 ==========
    private AudioClip GetMoveClipForCurrentState()
    {
        if (audioController == null) return null;
        
        switch (player.CurrentState)
        {
            case Player.PlayerState.Solid:
                return cachedSolidMoveClip ?? audioController.solidMoveClip;
            case Player.PlayerState.Liquid:
                return cachedLiquidMoveClip ?? audioController.liquidMoveClip;
            case Player.PlayerState.Gas:
                return cachedGasMoveClip ?? audioController.gasMoveClip;
            default:
                return null;
        }
    }

    // ========== 状态特定的移动逻辑 ==========

    private void HandleLiquidMovement()
    {
        SmoothHorizontalMovement();

        // 液态可以尝试攀爬台阶
        if (isGrounded && Mathf.Abs(horizontalInput) > 0.1f)
        {
            TryClimbStep();
        }

        // 液态粘性，落地时减速
        if (isGrounded && Mathf.Abs(rb.velocity.y) > 0.1f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.8f);
        }
    }

    private void HandleSolidMovement()
    {
        SmoothHorizontalMovement();

        // 固态惯性，减速更慢
        if (externalVelocityLockTimer <= 0f && Mathf.Abs(horizontalInput) < 0.1f)
        {
            Vector2 velocity = rb.velocity;
            velocity.x = Mathf.Lerp(velocity.x, 0, deceleration * 0.5f * Time.fixedDeltaTime);
            rb.velocity = velocity;
        }
    }

    private void HandleGasMovement()
    {
        SmoothHorizontalMovement();

        // 处理天花板碰撞
        if (isTouchingCeiling)
        {
            // 气态碰天花板时处理
            Vector2 velocity = rb.velocity;

            if (gasBounceFromCeiling)
            {
                // 反弹效果
                velocity.y = -Mathf.Abs(velocity.y) * gasBounceFactor;
                if (showCeilingDebug) Debug.Log($"天花板反弹: 速度 {velocity.y}");
            }
            else
            {
                // 停止上升并轻微下降
                velocity.y = Mathf.Min(velocity.y, -0.5f);
            }

            rb.velocity = velocity;
        }
        else
        {
            // 自由漂浮逻辑
            Vector2 velocity = rb.velocity;

            // 确保最小上升速度，切换时增强
            float timeSinceSwitch = Time.time - gasSwitchTime;
            float currentFloatSpeed = gasFloatSpeed;

            // 切换时给予更强上升力
            if (timeSinceSwitch < 1f)
            {
                currentFloatSpeed = Mathf.Lerp(gasFloatSpeed * 1.5f, gasFloatSpeed, timeSinceSwitch);
            }

            // 控制上升
            if (velocity.y < gasMaxFloatSpeed)
            {
                velocity.y = Mathf.Min(velocity.y + currentFloatSpeed * Time.fixedDeltaTime, gasMaxFloatSpeed);
            }

            rb.velocity = velocity;
        }

        // 气态不接触地面
        player.SetGrounded(false);
    }

    // ========== 水平移动处理 ==========

    private void SmoothHorizontalMovement()
    {
        if (externalVelocityLockTimer > 0f)
        {
            return;
        }

        targetVelocityX = horizontalInput * moveSpeed;
        float smoothRate = (Mathf.Abs(horizontalInput) > 0.1f) ? acceleration : deceleration;
        currentVelocityX = Mathf.Lerp(currentVelocityX, targetVelocityX, smoothRate * Time.fixedDeltaTime);

        Vector2 velocity = rb.velocity;
        velocity.x = currentVelocityX;
        rb.velocity = velocity;
    }

    // ========== 碰撞检测 ==========

    private void CheckGround()
    {
        if (col == null || player == null) return;

        // 气态不检测地面
        if (player.CurrentState == Player.PlayerState.Gas)
        {
            isGrounded = false;
            player.SetGrounded(false);
            return;
        }

        Vector2 rayStart = transform.position;
        float rayLength = col.bounds.extents.y + 0.1f;

        // 多方向射线确保检测准确
        RaycastHit2D hitCenter = Physics2D.Raycast(rayStart, Vector2.down, rayLength, groundLayer);
        RaycastHit2D hitLeft = Physics2D.Raycast(rayStart + (Vector2.left * col.bounds.extents.x * 0.7f),
                                                 Vector2.down, rayLength, groundLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(rayStart + (Vector2.right * col.bounds.extents.x * 0.7f),
                                                  Vector2.down, rayLength, groundLayer);

        isGrounded = hitCenter.collider != null || hitLeft.collider != null || hitRight.collider != null;

        player.SetGrounded(isGrounded);
    }

    private void CheckCeiling()
    {
        if (col == null || player == null) return;

        // 只有气态需要检测天花板
        if (player.CurrentState != Player.PlayerState.Gas)
        {
            isTouchingCeiling = false;
            return;
        }

        Vector2 rayStart = transform.position;
        float rayLength = col.bounds.extents.y + ceilingCheckDistance;

        // 多方向检测天花板
        RaycastHit2D hitCenter = Physics2D.Raycast(rayStart, Vector2.up, rayLength, groundLayer);
        RaycastHit2D hitLeft = Physics2D.Raycast(rayStart + (Vector2.left * col.bounds.extents.x * 0.7f),
                                                 Vector2.up, rayLength, groundLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(rayStart + (Vector2.right * col.bounds.extents.x * 0.7f),
                                                  Vector2.up, rayLength, groundLayer);

        isTouchingCeiling = hitCenter.collider != null || hitLeft.collider != null || hitRight.collider != null;

        // 调试输出
        if (showCeilingDebug && isTouchingCeiling && Time.frameCount % 30 == 0)
        {
            Debug.Log($"气态碰天花板: 速度Y={rb.velocity.y:F2}");
        }
    }

    // ========== 辅助功能 ==========

    private void TryClimbStep()
    {
        if (col == null || player == null || player.CurrentState != Player.PlayerState.Liquid) return;

        Vector2 rayOrigin = (Vector2)transform.position + Vector2.up * 0.1f;
        Vector2 direction = horizontalInput > 0 ? Vector2.right : Vector2.left;

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, direction, stepCheckDistance, groundLayer);
        if (hit.collider != null)
        {
            float stepTop = hit.collider.bounds.max.y;
            float playerBottom = transform.position.y - col.bounds.extents.y;
            float heightDifference = stepTop - playerBottom;

            if (heightDifference > 0 && heightDifference <= stepHeight)
            {
                Vector3 newPosition = transform.position;
                newPosition.y = stepTop + col.bounds.extents.y + 0.05f;
                rb.MovePosition(newPosition);

                Debug.Log($"攀爬台阶成功: 高度差={heightDifference:F2}");
            }
        }
    }

    // ========== 公共接口 ==========

    public float GetHorizontalInput()
    {
        return horizontalInput;
    }

    public void ForceUpdatePhysics()
    {
        if (player != null)
        {
            Debug.Log($"强制更新物理状态: {player.CurrentState}");
            ApplyPhysicsForState(player.CurrentState, true);
            lastAppliedState = player.CurrentState;
        }
    }

    public bool IsTouchingCeiling()
    {
        return isTouchingCeiling;
    }

    // ========== 调试可视化 ==========

    void OnDrawGizmosSelected()
    {
        if (col == null) return;

        // 地面检测可视化
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 groundStart = transform.position;
        Vector3 groundEnd = groundStart + Vector3.down * (col.bounds.extents.y + 0.1f);
        Gizmos.DrawLine(groundStart, groundEnd);

        // 天花板检测可视化（气态时）
        if (player != null && player.CurrentState == Player.PlayerState.Gas)
        {
            Gizmos.color = isTouchingCeiling ? Color.yellow : Color.cyan;
            Vector3 ceilingStart = transform.position;
            Vector3 ceilingEnd = ceilingStart + Vector3.up * (col.bounds.extents.y + ceilingCheckDistance);
            Gizmos.DrawLine(ceilingStart, ceilingEnd);
        }

        // 台阶检测可视化
        if (Mathf.Abs(horizontalInput) > 0.1f && player != null && player.CurrentState == Player.PlayerState.Liquid)
        {
            Gizmos.color = Color.blue;
            Vector3 stepCheckStart = transform.position + Vector3.up * 0.1f;
            Vector3 stepCheckEnd = stepCheckStart + Vector3.right * stepCheckDistance * Mathf.Sign(horizontalInput);
            Gizmos.DrawLine(stepCheckStart, stepCheckEnd);
        }

        // 显示当前状态
#if UNITY_EDITOR
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 12;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, 
                                 $"状态: {player?.CurrentState}\n接地: {isGrounded}\n天花板: {isTouchingCeiling}", style);
#endif
    }

    // ========== 碰撞事件 ==========

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 检测碰撞类型
        if (player != null && player.CurrentState == Player.PlayerState.Gas)
        {
            // 气态碰撞特殊处理
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                // 检查是否是上方碰撞（天花板）
                foreach (ContactPoint2D contact in collision.contacts)
                {
                    if (contact.normal.y < -0.5f)
                    {
                        Debug.Log($"气态撞到天花板: 法线={contact.normal}");
                        isTouchingCeiling = true;
                    }
                }
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        // 离开碰撞时重置天花板状态
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isTouchingCeiling = false;
        }
    }

    public void LockExternalVelocity(float duration)
    {
        externalVelocityLockTimer = Mathf.Max(externalVelocityLockTimer, duration);
    }
}