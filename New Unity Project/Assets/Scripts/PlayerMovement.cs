//2026.1.29:许兆璘
//添加A、D键左右移动功能
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("移动参数")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 10f;

    [Header("状态物理参数")]
    [SerializeField] private float liquidGravity = 3f;
    [SerializeField] private float solidGravity = 5f;
    [SerializeField] private float gasGravity = -2f; // 负值=上浮

    [Header("台阶攀登")]
    [SerializeField] private float stepHeight = 0.5f; // 可登上台阶高度
    [SerializeField] private float stepCheckDistance = 0.5f; // 台阶检测距离
    [SerializeField] private LayerMask groundLayer; // 地面图层

    // 组件引用
    private Rigidbody2D rb;
    private Collider2D col;
    private Player player;

    // 移动状态
    private float horizontalInput;
    private float targetVelocityX;
    private float currentVelocityX;
    private bool isGrounded;

    void Start()
    {
        // 获取组件
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        player = GetComponent<Player>();

        if (player == null)
        {
            Debug.LogError("PlayerMovement需要Player组件！");
        }
    }

    void Update()
    {
        // 获取输入（在Update中处理输入更精确）
        horizontalInput = Input.GetAxisRaw("Horizontal"); // 使用Raw获得即时响应
    }

    void FixedUpdate()
    {
        if (player == null || rb == null) return;

        // 检查地面
        CheckGround();

        // 根据状态应用不同的移动逻辑
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
    }

    // 流动态移动（可登台阶）
    private void HandleLiquidMovement()
    {
        // 设置重力
        rb.gravityScale = liquidGravity;

        // 平滑移动
        SmoothHorizontalMovement();

        // 台阶攀登检测
        if (isGrounded && Mathf.Abs(horizontalInput) > 0.1f)
        {
            TryClimbStep();
        }
    }

    // 凝聚态移动（标准移动）
    private void HandleSolidMovement()
    {
        rb.gravityScale = solidGravity;
        SmoothHorizontalMovement();
    }

    // 气化态移动（上浮+移动）
    private void HandleGasMovement()
    {
        rb.gravityScale = gasGravity;

        // 水平移动
        SmoothHorizontalMovement();

        // 确保最小上浮速度
        Vector2 velocity = rb.velocity;
        float minFloatSpeed = 2f;
        if (velocity.y < minFloatSpeed)
        {
            velocity.y = minFloatSpeed;
        }
        rb.velocity = velocity;
    }

    // 平滑水平移动
    private void SmoothHorizontalMovement()
    {
        // 计算目标速度
        targetVelocityX = horizontalInput * moveSpeed;

        // 平滑过渡到目标速度
        float smoothRate = (Mathf.Abs(horizontalInput) > 0.1f) ? acceleration : deceleration;
        currentVelocityX = Mathf.Lerp(
            currentVelocityX,
            targetVelocityX,
            smoothRate * Time.fixedDeltaTime
        );

        // 应用水平速度
        Vector2 velocity = rb.velocity;
        velocity.x = currentVelocityX;
        rb.velocity = velocity;
    }

    // 尝试登上台阶
    private void TryClimbStep()
    {
        // 检测前方是否有台阶
        Vector2 rayOrigin = (Vector2)transform.position + Vector2.up * 0.1f; // 从略高于脚底的位置检测
        Vector2 direction = horizontalInput > 0 ? Vector2.right : Vector2.left;

        RaycastHit2D hit = Physics2D.Raycast(
            rayOrigin,
            direction,
            stepCheckDistance,
            groundLayer
        );

        if (hit.collider != null)
        {
            // 检查台阶高度是否可攀登
            float stepTop = hit.collider.bounds.max.y;
            float playerBottom = transform.position.y - col.bounds.extents.y;
            float heightDifference = stepTop - playerBottom;

            if (heightDifference > 0 && heightDifference <= stepHeight)
            {
                // 登上台阶
                Vector3 newPosition = transform.position;
                newPosition.y = stepTop + col.bounds.extents.y;
                transform.position = newPosition;
            }
        }
    }

    // 检查是否在地面
    private void CheckGround()
    {
        if (col == null) return;

        Vector2 rayStart = transform.position;
        float rayLength = col.bounds.extents.y + 0.1f; // 稍微超过碰撞体底部

        RaycastHit2D hit = Physics2D.Raycast(
            rayStart,
            Vector2.down,
            rayLength,
            groundLayer
        );

        isGrounded = hit.collider != null;
    }

    // 绘制调试信息（仅编辑器中可见）
    void OnDrawGizmosSelected()
    {
        // 绘制地面检测线
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 start = transform.position;
        Vector3 end = start + Vector3.down * (GetComponent<Collider2D>()?.bounds.extents.y + 0.1f ?? 0.5f);
        Gizmos.DrawLine(start, end);

        // 绘制台阶检测区域
        Gizmos.color = Color.blue;
        Vector3 stepCheckStart = transform.position + Vector3.up * 0.1f;
        Vector3 stepCheckEnd = stepCheckStart + Vector3.right * stepCheckDistance * Mathf.Sign(horizontalInput);
        Gizmos.DrawLine(stepCheckStart, stepCheckEnd);
    }
}