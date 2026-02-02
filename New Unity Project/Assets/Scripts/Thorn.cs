// 2026.1.27 郑佳鑫
// 第一次修改：尖刺脚本 - 接触立即致命
//第二次修改：2026.2.3 增加调试日志

using UnityEngine;
// 尖刺脚本 - 接触立即致命
public class Thorn : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"[Thorn] Trigger enter: name={collision.name}, tag={collision.tag}");
        if (collision.CompareTag("Player"))
        {
            Player playerScript = collision.GetComponent<Player>();
            if (playerScript != null)
            {
                Debug.Log("[Thorn] Player script found, calling Die()");
                playerScript.Die();
            }
            else
            {
                Debug.LogWarning("[Thorn] Player tag found but Player component missing");
            }
        }
    }
}
