//郑佳鑫
//2026.2.7 第一次修改：弹簧板脚本 - 处理垂直反弹
// 2026.2.9 第二次修改：增加水平反弹选项，调整反弹方向计算
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SpringBoard : MonoBehaviour
{
    [Header("Collider")]
    [SerializeField] private bool forceSolidCollider = true;

    [Header("Behavior")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool onlyFromAbove = true;
    [SerializeField] private float fromAboveNormalThreshold = 0.5f;
    [SerializeField] private float minSpeed = 0.05f;
    [SerializeField] private float bounceMultiplier = 1f;
    [SerializeField] private bool cancelNonSolidBounce = false;
    [SerializeField] private float lockHorizontalTime = 0.15f;
    [SerializeField] private bool invertHorizontalDirection = false;

    [Header("Optional Material")]
    [SerializeField] private PhysicsMaterial2D springMaterial;

    private Collider2D boardCollider;

    private Rigidbody2D lastBouncedRb;
    private float bounceLockTimer;
    private float bounceSpeed;
    private float bounceSign;

    private void Awake()
    {
        boardCollider = GetComponent<Collider2D>();
        if (forceSolidCollider && boardCollider != null && boardCollider.isTrigger)
        {
            boardCollider.isTrigger = false;
        }

        if (boardCollider != null && springMaterial != null)
        {
            boardCollider.sharedMaterial = springMaterial;
        }
    }

    private void FixedUpdate()
    {
        if (bounceLockTimer <= 0f || lastBouncedRb == null)
        {
            return;
        }

        bounceLockTimer -= Time.fixedDeltaTime;

        Vector2 vel = lastBouncedRb.velocity;
        vel.x = bounceSign * bounceSpeed;
        if (vel.y > 0f)
        {
            vel.y = 0f; // prevent upward bounce; gravity can pull down after
        }

        lastBouncedRb.velocity = vel;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (bounceLockTimer <= 0f || lastBouncedRb == null) return;
        if (collision.rigidbody != lastBouncedRb) return;

        Player player = collision.collider.GetComponentInParent<Player>();
        if (player == null || player.CurrentState != Player.PlayerState.Solid) return;

        Vector2 vel = lastBouncedRb.velocity;
        vel.x = bounceSign * bounceSpeed;
        if (vel.y > 0f)
        {
            vel.y = 0f;
        }

        lastBouncedRb.velocity = vel;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision == null || collision.collider == null) return;

        Player player = collision.collider.GetComponentInParent<Player>();
        if (player == null) return;

        if (!string.IsNullOrEmpty(playerTag) && !player.CompareTag(playerTag)) return;

        if (player.CurrentState != Player.PlayerState.Solid)
        {
            if (cancelNonSolidBounce)
            {
                Rigidbody2D nonSolidRb = collision.rigidbody;
                if (nonSolidRb != null && collision.contactCount > 0)
                {
                    ContactPoint2D nonSolidContact = collision.GetContact(0);
                    Vector2 normal = nonSolidContact.normal.normalized;
                    float normalSpeed = Vector2.Dot(nonSolidRb.velocity, normal);
                    nonSolidRb.velocity -= normal * normalSpeed;
                }
            }
            return;
        }

        if (collision.contactCount == 0) return;

        ContactPoint2D contact = collision.GetContact(0);
        Vector2 boardNormal = (Vector2)transform.up;
        if (onlyFromAbove)
        {
            float dot = Vector2.Dot(contact.normal, boardNormal);
            if (dot <= fromAboveNormalThreshold) return;
        }

        Rigidbody2D rb = collision.rigidbody;
        if (rb == null) return;

        Vector2 incoming = rb.velocity;
        float speed = incoming.magnitude;
        if (speed < minSpeed) return;

        // 反弹方向：始终与地面平行，方向由碰撞法线的左右决定
        float horizontalSign = Mathf.Sign(contact.normal.x);
        if (Mathf.Abs(horizontalSign) < 0.001f)
        {
            horizontalSign = Mathf.Sign(transform.right.x);
        }

        if (invertHorizontalDirection)
        {
            horizontalSign *= -1f;
        }

        Vector2 outDir = new Vector2(horizontalSign, 0f);

        // 应用反弹速度，保持速度大小不变，方向改为水平
        Vector2 outVelocity = outDir * speed * bounceMultiplier;
        rb.velocity = outVelocity;
        rb.angularVelocity = 0f;

        lastBouncedRb = rb;
        bounceLockTimer = Mathf.Max(bounceLockTimer, lockHorizontalTime);
        bounceSpeed = outVelocity.magnitude;
        bounceSign = Mathf.Sign(outVelocity.x);

        PlayerMovement movement = collision.collider.GetComponentInParent<PlayerMovement>();
        if (movement != null)
        {
            movement.LockExternalVelocity(lockHorizontalTime);
        }

        Debug.Log($"[SpringBoard] 固态水平反弹 - 入射速度: {speed:F2}, 方向: {outDir}, 反弹后速度: {outVelocity.magnitude:F2}");
    }
}
