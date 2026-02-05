//2026.1.29:许兆璘
//添加A、D键左右移动功能

//xzl
//2026.2.3:修复了物理模型没有立即更新的bug
//2026.2.3:添加ForceUpdatePhysics方法，确保状态切换时立即更新物理
//2026.2.5：添加了天花板的碰撞



using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerMovement : MonoBehaviour
{
    [Header("移动参数")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float deceleration = 20f;

    [Header("状态物理参数")]
    [SerializeField] private float liquidGravity = 15f;
    [SerializeField] private float solidGravity = 20f;
    [SerializeField] private float gasGravity = -8f;  // 负值表示向上漂浮

    [Header("气态特殊设置")]
    [SerializeField] private float gasFloatSpeed = 3f;        // 基础漂浮速度
    [SerializeField] private float gasMaxFloatSpeed = 5f;     // 最大漂浮速度
    [SerializeField] private bool gasBounceFromCeiling = true; // 是否从天花板反弹
    [SerializeField] private float gasBounceFactor = 0.5f;    // 反弹系数

    [Header("台阶攀登")]
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

    // 状态跟踪
    private Player.PlayerState lastAppliedState;
    private Player.PlayerState previousState;

    // 时间跟踪
    private float gasSwitchTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        player = GetComponent<Player>();

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

            ApplyPhysicsForState(player.CurrentState, true);
            previousState = lastAppliedState;
            lastAppliedState = player.CurrentState;
        }

        // 调试信息
        if (Input.GetKeyDown(KeyCode.F2))
        {
            Debug.Log($"移动调试 - 状态: {player?.CurrentState}, 速度: {rb?.velocity}, 地面: {isGrounded}, 天花板: {isTouchingCeiling}");
        }
    }

    void FixedUpdate()
    {
        if (player == null || rb == null) return;

        // 检查地面和天花板
        CheckGround();
        CheckCeiling();

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
    }

    // ========== 物理状态应用 ==========

    private void ApplyPhysicsForState(Player.PlayerState state, bool forceApply = false)
    {
        if (rb == null) return;

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
                if (forceApply) Debug.Log($"应用液体物理: 重力={liquidGravity}");
                break;

            case Player.PlayerState.Solid:
                rb.gravityScale = solidGravity;
                rb.mass = 2f; // 更重
                if (forceApply) Debug.Log($"应用固体物理: 重力={solidGravity}, 质量=2");
                break;

            case Player.PlayerState.Gas:
                rb.gravityScale = gasGravity;
                rb.mass = 0.5f; // 更轻
                if (forceApply) Debug.Log($"应用气体物理: 重力={gasGravity}, 质量=0.5");

                // 切换到气态时给予初始上浮速度
                if (forceApply)
                {
                    Vector2 velocity = rb.velocity;
                    velocity.y = Mathf.Max(gasFloatSpeed, velocity.y);
                    rb.velocity = velocity;
                    if (showCeilingDebug) Debug.Log($"气态切换: 设置上浮速度 {velocity.y}");
                }
                break;
        }

    }

    // ========== 形态特有的移动逻辑 ==========

    private void HandleLiquidMovement()
    {
        SmoothHorizontalMovement();

        // 液体特殊能力：台阶攀登
        if (isGrounded && Mathf.Abs(horizontalInput) > 0.1f)
        {
            TryClimbStep();
        }

        // 液体粘性：落地时减少弹跳
        if (isGrounded && Mathf.Abs(rb.velocity.y) > 0.1f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.8f);
        }
    }

    private void HandleSolidMovement()
    {
        SmoothHorizontalMovement();

        // 固体惯性：减速更慢
        if (Mathf.Abs(horizontalInput) < 0.1f)
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
            // 碰到天花板时处理
            Vector2 velocity = rb.velocity;

            if (gasBounceFromCeiling)
            {
                // 反弹效果
                velocity.y = -Mathf.Abs(velocity.y) * gasBounceFactor;
                if (showCeilingDebug) Debug.Log($"天花板反弹: 速度 {velocity.y}");
            }
            else
            {
                // 停止上浮，允许轻微下落
                velocity.y = Mathf.Min(velocity.y, -0.5f);
            }

            rb.velocity = velocity;
        }
        else
        {
            // 正常漂浮逻辑
            Vector2 velocity = rb.velocity;

            // 确保最小上浮速度（刚切换时更强）
            float timeSinceSwitch = Time.time - gasSwitchTime;
            float currentFloatSpeed = gasFloatSpeed;

            // 刚切换时给予额外推力
            if (timeSinceSwitch < 1f)
            {
                currentFloatSpeed = Mathf.Lerp(gasFloatSpeed * 1.5f, gasFloatSpeed, timeSinceSwitch);
            }

            // 持续上浮
            if (velocity.y < gasMaxFloatSpeed)
            {
                velocity.y = Mathf.Min(velocity.y + currentFloatSpeed * Time.fixedDeltaTime, gasMaxFloatSpeed);
            }

            rb.velocity = velocity;
        }

        // 气体不会接地
        player.SetGrounded(false);
    }

    // ========== 水平移动处理 ==========

    private void SmoothHorizontalMovement()
    {
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

        // 气体不会检测地面
        if (player.CurrentState == Player.PlayerState.Gas)
        {
            isGrounded = false;
            player.SetGrounded(false);
            return;
        }

        Vector2 rayStart = transform.position;
        float rayLength = col.bounds.extents.y + 0.1f;

        // 发射多条射线确保检测准确
        // 修复：使用Vector2.left和Vector2.right替代Vector3
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

        // 发射多条射线检测天花板
        // 修复：使用Vector2.left和Vector2.right替代Vector3
        RaycastHit2D hitCenter = Physics2D.Raycast(rayStart, Vector2.up, rayLength, groundLayer);
        RaycastHit2D hitLeft = Physics2D.Raycast(rayStart + (Vector2.left * col.bounds.extents.x * 0.7f),
                                                 Vector2.up, rayLength, groundLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(rayStart + (Vector2.right * col.bounds.extents.x * 0.7f),
                                                  Vector2.up, rayLength, groundLayer);

        isTouchingCeiling = hitCenter.collider != null || hitLeft.collider != null || hitRight.collider != null;

        // 调试输出
        if (showCeilingDebug && isTouchingCeiling && Time.frameCount % 30 == 0)
        {
            Debug.Log($"触到天花板: 速度Y={rb.velocity.y:F2}");
        }
    }

    // ========== 特殊能力 ==========

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
                newPosition.y = stepTop + col.bounds.extents.y + 0.05f; // 稍微高一点避免卡住
                rb.MovePosition(newPosition);

                Debug.Log($"攀登台阶成功: 高度差={heightDifference:F2}");
            }
        }
    }

    // ========== 公开方法 ==========

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

        // 天花板检测可视化（仅气态）
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
                                 $"状态: {player?.CurrentState}\n地面: {isGrounded}\n天花板: {isTouchingCeiling}", style);
#endif
    }

    // ========== 碰撞事件 ==========

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 特殊碰撞处理
        if (player != null && player.CurrentState == Player.PlayerState.Gas)
        {
            // 气态碰撞特殊处理
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                // 检查是否从上方碰撞（天花板）
                foreach (ContactPoint2D contact in collision.contacts)
                {
                    if (contact.normal.y < -0.5f) // 从下方碰撞（对玩家来说是上方）
                    {
                        Debug.Log($"气态触到天花板: 法线={contact.normal}");
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
}