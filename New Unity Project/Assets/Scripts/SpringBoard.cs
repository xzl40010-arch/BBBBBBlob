//郑佳鑫
//2026.2.7 第一次修改：弹簧板脚本 - 处理垂直反弹
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
    [SerializeField] private bool cancelNonSolidBounce = true;

    [Header("Optional Material")]
    [SerializeField] private PhysicsMaterial2D springMaterial;

    private Collider2D boardCollider;

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

        // 反弹方向：使用碰撞法线作为反弹方向（垂直于弹簧板表面朝外）
        Vector2 outDir = contact.normal.normalized;

        // 应用反弹速度，保持速度大小不变，方向改为法线方向
        // 反弹后速度会由于重力逐渐衰减
        Vector2 outVelocity = outDir * speed * bounceMultiplier;
        rb.velocity = outVelocity;
        rb.angularVelocity = 0f;

        Debug.Log($"[SpringBoard] 固态反弹 - 入射速度: {speed:F2}, 反弹方向: {outDir}, 反弹后速度: {outVelocity.magnitude:F2}");
    }
}
