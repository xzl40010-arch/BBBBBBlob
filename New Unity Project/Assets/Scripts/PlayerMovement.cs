//2026.1.29:���׭U
//���A��D�������ƶ�����

//xzl
//2026.2.3:�޸�������ģ��û���������µ�bug
//2026.2.3:���ForceUpdatePhysics������ȷ��״̬�л�ʱ������������
//2026.2.5��������컨�����ײ



using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerMovement : MonoBehaviour
{
    [Header("�ƶ�����")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float deceleration = 20f;

    [Header("״̬�������")]
    [SerializeField] private float liquidGravity = 15f;
    [SerializeField] private float solidGravity = 20f;
    [SerializeField] private float gasGravity = -8f;  // ��ֵ��ʾ����Ư��

    [Header("��̬��������")]
    [SerializeField] private float gasFloatSpeed = 3f;        // ����Ư���ٶ�
    [SerializeField] private float gasMaxFloatSpeed = 5f;     // ���Ư���ٶ�
    [SerializeField] private bool gasBounceFromCeiling = true; // �Ƿ���컨�巴��
    [SerializeField] private float gasBounceFactor = 0.5f;    // ����ϵ��

    [Header("̨���ʵ�")]
    [SerializeField] private float stepHeight = 0.5f;
    [SerializeField] private float stepCheckDistance = 0.5f;
    [SerializeField] private LayerMask groundLayer;

    [Header("�컨����")]
    [SerializeField] private float ceilingCheckDistance = 0.5f;
    [SerializeField] private bool showCeilingDebug = true;

    [Header("�������")]
    [SerializeField] private PhysicsMaterial2D liquidPhysicsMaterial;
    [SerializeField] private PhysicsMaterial2D solidPhysicsMaterial;
    [SerializeField] private PhysicsMaterial2D gasPhysicsMaterial;

    [Header("Physics Tuning")]
    [SerializeField] private bool useContinuousCollision = true;
    [SerializeField] private bool useInterpolation = true;
    [SerializeField] private bool neverSleep = true;

    // �������
    private Rigidbody2D rb;
    private Collider2D col;
    private Player player;

    // �ƶ�״̬
    private float horizontalInput;
    private float targetVelocityX;
    private float currentVelocityX;
    private bool isGrounded;
    private bool isTouchingCeiling;

    // ״̬����
    private Player.PlayerState lastAppliedState;
    private Player.PlayerState previousState;

    // ʱ�����
    private float gasSwitchTime = 0f;
    private float externalVelocityLockTimer;

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
            Debug.Log($"PlayerMovement��ʼ��: ״̬={lastAppliedState}");
        }
        else
        {
            Debug.LogError("PlayerMovement: �Ҳ���Player���!");
        }
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // ���״̬�仯
        if (player != null && player.CurrentState != lastAppliedState)
        {
            Debug.Log($"PlayerMovement��⵽״̬�仯: {lastAppliedState} -> {player.CurrentState}");

            // ��¼��̬�л�ʱ��
            if (player.CurrentState == Player.PlayerState.Gas)
            {
                gasSwitchTime = Time.time;
            }

            ApplyPhysicsForState(player.CurrentState, true);
            previousState = lastAppliedState;
            lastAppliedState = player.CurrentState;
        }

        // ������Ϣ
        if (Input.GetKeyDown(KeyCode.F2))
        {
            Debug.Log($"�ƶ����� - ״̬: {player?.CurrentState}, �ٶ�: {rb?.velocity}, ����: {isGrounded}, �컨��: {isTouchingCeiling}");
        }
    }

    void FixedUpdate()
    {
        if (player == null || rb == null) return;

        if (externalVelocityLockTimer > 0f)
        {
            externalVelocityLockTimer -= Time.fixedDeltaTime;
        }

        // ��������컨��
        CheckGround();
        CheckCeiling();

        // ����״̬�����ƶ�
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

        // ȷ������Ч����ȷӦ��
        if (player.CurrentState != lastAppliedState)
        {
            ApplyPhysicsForState(player.CurrentState, true);
            lastAppliedState = player.CurrentState;
        }
    }

    // ========== ����״̬Ӧ�� ==========

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

        // Ӧ�ö�Ӧ���������
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
                if (forceApply) Debug.Log($"Ӧ��Һ������: ����={liquidGravity}");
                break;

            case Player.PlayerState.Solid:
                rb.gravityScale = solidGravity;
                rb.mass = 2f; // ����
                if (forceApply) Debug.Log($"Ӧ�ù�������: ����={solidGravity}, ����=2");
                break;

            case Player.PlayerState.Gas:
                rb.gravityScale = gasGravity;
                rb.mass = 0.5f; // ����
                if (forceApply) Debug.Log($"Ӧ����������: ����={gasGravity}, ����=0.5");

                // �л�����̬ʱ�����ʼ�ϸ��ٶ�
                if (forceApply)
                {
                    Vector2 velocity = rb.velocity;
                    velocity.y = Mathf.Max(gasFloatSpeed, velocity.y);
                    rb.velocity = velocity;
                    if (showCeilingDebug) Debug.Log($"��̬�л�: �����ϸ��ٶ� {velocity.y}");
                }
                break;
        }

    }

    // ========== ��̬���е��ƶ��߼� ==========

    private void HandleLiquidMovement()
    {
        SmoothHorizontalMovement();

        // Һ������������̨���ʵ�
        if (isGrounded && Mathf.Abs(horizontalInput) > 0.1f)
        {
            TryClimbStep();
        }

        // Һ��ճ�ԣ����ʱ���ٵ���
        if (isGrounded && Mathf.Abs(rb.velocity.y) > 0.1f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.8f);
        }
    }

    private void HandleSolidMovement()
    {
        SmoothHorizontalMovement();

        // ������ԣ����ٸ���
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

        // �����컨����ײ
        if (isTouchingCeiling)
        {
            // �����컨��ʱ����
            Vector2 velocity = rb.velocity;

            if (gasBounceFromCeiling)
            {
                // ����Ч��
                velocity.y = -Mathf.Abs(velocity.y) * gasBounceFactor;
                if (showCeilingDebug) Debug.Log($"�컨�巴��: �ٶ� {velocity.y}");
            }
            else
            {
                // ֹͣ�ϸ���������΢����
                velocity.y = Mathf.Min(velocity.y, -0.5f);
            }

            rb.velocity = velocity;
        }
        else
        {
            // ����Ư���߼�
            Vector2 velocity = rb.velocity;

            // ȷ����С�ϸ��ٶȣ����л�ʱ��ǿ��
            float timeSinceSwitch = Time.time - gasSwitchTime;
            float currentFloatSpeed = gasFloatSpeed;

            // ���л�ʱ�����������
            if (timeSinceSwitch < 1f)
            {
                currentFloatSpeed = Mathf.Lerp(gasFloatSpeed * 1.5f, gasFloatSpeed, timeSinceSwitch);
            }

            // �����ϸ�
            if (velocity.y < gasMaxFloatSpeed)
            {
                velocity.y = Mathf.Min(velocity.y + currentFloatSpeed * Time.fixedDeltaTime, gasMaxFloatSpeed);
            }

            rb.velocity = velocity;
        }

        // ���岻��ӵ�
        player.SetGrounded(false);
    }

    // ========== ˮƽ�ƶ����� ==========

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

    // ========== ��ײ��� ==========

    private void CheckGround()
    {
        if (col == null || player == null) return;

        // ���岻�������
        if (player.CurrentState == Player.PlayerState.Gas)
        {
            isGrounded = false;
            player.SetGrounded(false);
            return;
        }

        Vector2 rayStart = transform.position;
        float rayLength = col.bounds.extents.y + 0.1f;

        // �����������ȷ�����׼ȷ
        // �޸���ʹ��Vector2.left��Vector2.right���Vector3
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

        // ֻ����̬��Ҫ����컨��
        if (player.CurrentState != Player.PlayerState.Gas)
        {
            isTouchingCeiling = false;
            return;
        }

        Vector2 rayStart = transform.position;
        float rayLength = col.bounds.extents.y + ceilingCheckDistance;

        // ����������߼���컨��
        // �޸���ʹ��Vector2.left��Vector2.right���Vector3
        RaycastHit2D hitCenter = Physics2D.Raycast(rayStart, Vector2.up, rayLength, groundLayer);
        RaycastHit2D hitLeft = Physics2D.Raycast(rayStart + (Vector2.left * col.bounds.extents.x * 0.7f),
                                                 Vector2.up, rayLength, groundLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(rayStart + (Vector2.right * col.bounds.extents.x * 0.7f),
                                                  Vector2.up, rayLength, groundLayer);

        isTouchingCeiling = hitCenter.collider != null || hitLeft.collider != null || hitRight.collider != null;

        // �������
        if (showCeilingDebug && isTouchingCeiling && Time.frameCount % 30 == 0)
        {
            Debug.Log($"�����컨��: �ٶ�Y={rb.velocity.y:F2}");
        }
    }

    // ========== �������� ==========

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
                newPosition.y = stepTop + col.bounds.extents.y + 0.05f; // ��΢��һ����⿨ס
                rb.MovePosition(newPosition);

                Debug.Log($"�ʵ�̨�׳ɹ�: �߶Ȳ�={heightDifference:F2}");
            }
        }
    }

    // ========== �������� ==========

    public float GetHorizontalInput()
    {
        return horizontalInput;
    }

    public void ForceUpdatePhysics()
    {
        if (player != null)
        {
            Debug.Log($"ǿ�Ƹ�������״̬: {player.CurrentState}");
            ApplyPhysicsForState(player.CurrentState, true);
            lastAppliedState = player.CurrentState;
        }
    }

    public bool IsTouchingCeiling()
    {
        return isTouchingCeiling;
    }

    // ========== ���Կ��ӻ� ==========

    void OnDrawGizmosSelected()
    {
        if (col == null) return;

        // ��������ӻ�
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 groundStart = transform.position;
        Vector3 groundEnd = groundStart + Vector3.down * (col.bounds.extents.y + 0.1f);
        Gizmos.DrawLine(groundStart, groundEnd);

        // �컨������ӻ�������̬��
        if (player != null && player.CurrentState == Player.PlayerState.Gas)
        {
            Gizmos.color = isTouchingCeiling ? Color.yellow : Color.cyan;
            Vector3 ceilingStart = transform.position;
            Vector3 ceilingEnd = ceilingStart + Vector3.up * (col.bounds.extents.y + ceilingCheckDistance);
            Gizmos.DrawLine(ceilingStart, ceilingEnd);
        }

        // ̨�׼����ӻ�
        if (Mathf.Abs(horizontalInput) > 0.1f && player != null && player.CurrentState == Player.PlayerState.Liquid)
        {
            Gizmos.color = Color.blue;
            Vector3 stepCheckStart = transform.position + Vector3.up * 0.1f;
            Vector3 stepCheckEnd = stepCheckStart + Vector3.right * stepCheckDistance * Mathf.Sign(horizontalInput);
            Gizmos.DrawLine(stepCheckStart, stepCheckEnd);
        }

        // ��ʾ��ǰ״̬
#if UNITY_EDITOR
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 12;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, 
                                 $"״̬: {player?.CurrentState}\n����: {isGrounded}\n�컨��: {isTouchingCeiling}", style);
#endif
    }

    // ========== ��ײ�¼� ==========

    void OnCollisionEnter2D(Collision2D collision)
    {
        // ������ײ����
        if (player != null && player.CurrentState == Player.PlayerState.Gas)
        {
            // ��̬��ײ���⴦��
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                // ����Ƿ���Ϸ���ײ���컨�壩
                foreach (ContactPoint2D contact in collision.contacts)
                {
                    if (contact.normal.y < -0.5f) // ���·���ײ���������˵���Ϸ���
                    {
                        Debug.Log($"��̬�����컨��: ����={contact.normal}");
                        isTouchingCeiling = true;
                    }
                }
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        // �뿪��ײʱ�����컨��״̬
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
