//郑佳鑫
//2026.1.28：第一次修改，气化态触碰后转为流动态，并播放“滋滋滋”音效
//2026.2.5：第二次修改，彻底修复问题
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CoolingPlate : MonoBehaviour
{
    [SerializeField] private bool forceSolidCollider = true;
    private Collider2D plateCollider;

    private void Awake()
    {
        plateCollider = GetComponent<Collider2D>();
        if (forceSolidCollider && plateCollider != null && plateCollider.isTrigger)
        {
            plateCollider.isTrigger = false;
            Debug.LogWarning("[CoolingPlate] Collider was Trigger, forced to solid for platform behavior");
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

        Debug.Log($"[CoolingPlate] {source} enter: name={collider.name}, tag={collider.tag}");

        if (!collider.CompareTag("Player"))
        {
            return;
        }

        // Allow player collider to be on a child object.
        Player player = collider.GetComponentInParent<Player>();
        if (player == null)
        {
            Debug.LogWarning("[CoolingPlate] Player component not found on collider or parent");
            return;
        }

        if (player.CurrentState != Player.PlayerState.Gas)
        {
            Debug.Log("[CoolingPlate] Player state is not Gas, conversion skipped");
            return;
        }

        // 气化态触碰立即转为流动态
        bool changed = player.ConvertGasToLiquid();
        Debug.Log($"[CoolingPlate] ConvertGasToLiquid changed={changed}");
    }
}
