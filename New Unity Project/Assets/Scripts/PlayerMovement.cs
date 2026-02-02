//2026.1.29:许兆璘
//添加A、D键左右移动功能

//xzl
//2026.2.3:修复了物理模型没有立即更新的bug
//2026.2.3:添加ForceUpdatePhysics方法，确保状态切换时立即更新物理


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
    [SerializeField] private float gasGravity = -2f;

    [Header("台阶攀登")]
    [SerializeField] private float stepHeight = 0.5f;
    [SerializeField] private float stepCheckDistance = 0.5f;
    [SerializeField] private LayerMask groundLayer;

    // 组件引用
    private Rigidbody2D rb;
    private Collider2D col;
    private Player player;

    // 移动状态
    private float horizontalInput;
    private float targetVelocityX;
    private float currentVelocityX;
    private bool isGrounded;

    // 状态跟踪
    private Player.PlayerState lastAppliedState;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        player = GetComponent<Player>();

        if (player != null)
        {
            lastAppliedState = player.CurrentState;
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
    }

    void FixedUpdate()
    {
        if (player == null || rb == null) return;

        CheckGround();

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
    }

    // 公开方法：强制更新物理效果
    public void ForceUpdatePhysics()
    {
        if (player != null)
        {
            Debug.Log($"ForceUpdatePhysics: 强制更新物理状态 {player.CurrentState}");
            ApplyPhysicsForState(player.CurrentState, true);
            lastAppliedState = player.CurrentState;
        }
    }

    private void ApplyPhysicsForState(Player.PlayerState state, bool forceApply = false)
    {
        switch (state)
        {
            case Player.PlayerState.Liquid:
                rb.gravityScale = liquidGravity;
                Debug.Log($"应用液体物理: 重力={liquidGravity}");
                break;
            case Player.PlayerState.Solid:
                rb.gravityScale = solidGravity;
                Debug.Log($"应用固体物理: 重力={solidGravity}");
                break;
            case Player.PlayerState.Gas:
                rb.gravityScale = gasGravity;
                Debug.Log($"应用气体物理: 重力={gasGravity}");

                // 切换到气态时立即给予上浮速度
                if (forceApply)
                {
                    Vector2 velocity = rb.velocity;
                    velocity.y = Mathf.Max(2f, velocity.y);
                    rb.velocity = velocity;
                    Debug.Log($"气态切换: 设置上浮速度 {velocity.y}");
                }
                break;
        }
    }

    private void HandleLiquidMovement()
    {
        SmoothHorizontalMovement();
        if (isGrounded && Mathf.Abs(horizontalInput) > 0.1f)
        {
            TryClimbStep();
        }
    }

    private void HandleSolidMovement()
    {
        SmoothHorizontalMovement();
    }

    private void HandleGasMovement()
    {
        SmoothHorizontalMovement();

        // 确保最小上浮速度
        Vector2 velocity = rb.velocity;
        if (velocity.y < 2f)
        {
            velocity.y = 2f;
            rb.velocity = velocity;
        }
    }

    private void SmoothHorizontalMovement()
    {
        targetVelocityX = horizontalInput * moveSpeed;
        float smoothRate = (Mathf.Abs(horizontalInput) > 0.1f) ? acceleration : deceleration;
        currentVelocityX = Mathf.Lerp(currentVelocityX, targetVelocityX, smoothRate * Time.fixedDeltaTime);

        Vector2 velocity = rb.velocity;
        velocity.x = currentVelocityX;
        rb.velocity = velocity;
    }

    private void TryClimbStep()
    {
        if (col == null) return;

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
                newPosition.y = stepTop + col.bounds.extents.y;
                transform.position = newPosition;
            }
        }
    }

    private void CheckGround()
    {
        if (col == null) return;

        Vector2 rayStart = transform.position;
        float rayLength = col.bounds.extents.y + 0.1f;

        RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.down, rayLength, groundLayer);
        isGrounded = hit.collider != null;

        if (player != null)
        {
            player.SetGrounded(isGrounded);
        }
    }

    void OnDrawGizmosSelected()
    {
        float extentsY = 0.5f;
        Collider2D gizmoCol = GetComponent<Collider2D>();
        if (gizmoCol != null)
        {
            extentsY = gizmoCol.bounds.extents.y + 0.1f;
        }

        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 start = transform.position;
        Vector3 end = start + Vector3.down * extentsY;
        Gizmos.DrawLine(start, end);

        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            Gizmos.color = Color.blue;
            Vector3 stepCheckStart = transform.position + Vector3.up * 0.1f;
            Vector3 stepCheckEnd = stepCheckStart + Vector3.right * stepCheckDistance * Mathf.Sign(horizontalInput);
            Gizmos.DrawLine(stepCheckStart, stepCheckEnd);
        }
    }
}