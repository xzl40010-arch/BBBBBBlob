// 2026.1.27 郑佳鑫
// 第一次修改：尖刺脚本 - 接触立即致命

using UnityEngine;
// 尖刺脚本 - 接触立即致命
public class Thorn : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player playerScript = collision.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.Die();
            }
        }
    }
}
