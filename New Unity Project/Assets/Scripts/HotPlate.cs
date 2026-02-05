//郑佳鑫
//2026.1.28：第一次修改，凝固态触碰立即转为流动态，并播放“滋滋滋”音效
//2026.2.5：第二次修改，彻底修复问题
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HotPlate : MonoBehaviour
{
    [SerializeField] private bool forceSolidCollider = true;
    private Collider2D plateCollider;

    private void Awake()
    {
        plateCollider = GetComponent<Collider2D>();
        if (forceSolidCollider && plateCollider != null && plateCollider.isTrigger)
        {
            plateCollider.isTrigger = false;
            Debug.LogWarning("[HotPlate] Collider was Trigger, forced to solid for platform behavior");
        }
    }

    private void Start()
    {

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryConvertToLiquid(collision.collider, "Collision");
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        TryConvertToLiquid(collider, "Trigger");
    }

    private void TryConvertToLiquid(Collider2D collider, string source)
    {
        if (collider == null) return;

        Debug.Log($"[HotPlate] {source} enter: name={collider.name}, tag={collider.tag}");

        // 只处理玩家
        if (!collider.CompareTag("Player"))
        {
            return;
        }

        // Allow player collider to be on a child object.
        Player player = collider.GetComponentInParent<Player>();
        if (player == null)
        {
            Debug.LogWarning("[HotPlate] Player component not found on collider or parent");
            return;
        }

        if (player.CurrentState != Player.PlayerState.Solid)
        {
            Debug.Log("[HotPlate] Player state is not Solid, conversion skipped");
            return;
        }

        // 凝固态触碰立即转为流动态
        bool changed = player.ConvertSolidToLiquid();
        Debug.Log($"[HotPlate] ConvertSolidToLiquid changed={changed}");
    }
}
