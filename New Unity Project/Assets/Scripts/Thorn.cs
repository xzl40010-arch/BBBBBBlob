// 2026.1.27 郑佳鑫
// 第一次修改：尖刺脚本 - 接触立即致命
//第二次修改：2026.2.3 增加调试日志

using UnityEngine;
// 尖刺脚本 - 接触立即致命
public class Thorn : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryKillPlayer(collision, "Trigger");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryKillPlayer(collision.collider, "Collision");
    }

    private void TryKillPlayer(Collider2D collider2D, string source)
    {
        if (collider2D == null) return;

        Debug.Log($"[Thorn] {source} enter: name={collider2D.name}, tag={collider2D.tag}");

        // Allow the collider to be on a child while Player is on the parent.
        Player playerScript = collider2D.GetComponentInParent<Player>();
        if (playerScript != null)
        {
            Debug.Log("[Thorn] Player script found, calling Die()");
            playerScript.Die();
        }
        else
        {
            Debug.LogWarning("[Thorn] Player component not found on collider or parent");
        }
    }
}
