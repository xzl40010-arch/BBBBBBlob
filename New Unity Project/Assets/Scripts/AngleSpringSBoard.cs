//郑佳鑫
//2026.2.7 第一次修改：倾斜弹簧板脚本 - 处理斜向反弹

//ycy
//2026.2.10 添加弹簧音效
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AngleSpringSBoard : MonoBehaviour
{
    [Header("碰撞体设置")]
    [SerializeField] private bool forceSolidCollider = true;

    [Header("行为配置")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool onlyFromAbove = true;
    [SerializeField] private float minDownwardSpeed = 0.1f;
    [SerializeField] private float minSpeed = 0.05f;
    [SerializeField] private float bounceMultiplier = 1f;
    [SerializeField] private bool cancelNonSolidBounce = true;

    [Header("反弹方向")]
    [SerializeField] private bool forceHorizontalBounce = true;
    [SerializeField] private bool bounceToLeft = true;

    [Header("可选材质")]
    [SerializeField] private PhysicsMaterial2D springMaterial;

    private Collider2D boardCollider;

    private int lastBounceFrame = -999;

    private AudioController audioController;

    private void Awake()
    {
        boardCollider = GetComponent<Collider2D>();

        GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
        if (audioObj != null)
        {
            audioController = audioObj.GetComponent<AudioController>();
        }

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
        TryHandleBounce(collision);

        //播放弹簧音效
        if (audioController != null)
        {
            audioController.PlaySfx(audioController.springClip);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryHandleBounce(collision);
    }

    private void TryHandleBounce(Collision2D collision)
    {
        if (collision == null || collision.collider == null) return;

        Player player = collision.collider.GetComponentInParent<Player>();
        if (player == null) return;

        if (!string.IsNullOrEmpty(playerTag) && !player.CompareTag(playerTag)) return;

        // 非固态处理：消除法线速度，防止被材质弹性反弹
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

        Rigidbody2D rb = collision.rigidbody;
        if (rb == null) return;

        Vector2 incomingVelocity = rb.velocity;
        float speed = incomingVelocity.magnitude;
        if (speed < minSpeed) return;

        if (onlyFromAbove && incomingVelocity.y >= -minDownwardSpeed) return;

        if (Time.frameCount == lastBounceFrame) return;
        lastBounceFrame = Time.frameCount;

        Vector2 bounceDir;
        if (forceHorizontalBounce)
        {
            bounceDir = bounceToLeft ? Vector2.left : Vector2.right;
        }
        else
        {
            ContactPoint2D contact = collision.GetContact(0);
            Vector2 contactNormal = contact.normal.normalized;
            bounceDir = new Vector2(-contactNormal.y, contactNormal.x);
            if (bounceToLeft && bounceDir.x > 0f) bounceDir = -bounceDir;
            if (!bounceToLeft && bounceDir.x < 0f) bounceDir = -bounceDir;
        }

        Vector2 bounceVelocity = bounceDir.normalized * speed * bounceMultiplier;
        rb.velocity = bounceVelocity;
        rb.angularVelocity = 0f;
    }
}
