// 郑佳鑫
// 2026.2.5 第一次修改： 存档点：玩家接触后保存当前位置
using UnityEngine;

public class ArchivePoint : MonoBehaviour
{
    [Header("存档点设置")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private bool saveOnTrigger = true;
    [SerializeField] private bool saveOnCollision = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!saveOnTrigger) return;
        TrySave(other, "Trigger");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!saveOnCollision) return;
        TrySave(collision.collider, "Collision");
    }

    private void TrySave(Collider2D collider2D, string source)
    {
        if (collider2D == null) return;
        if (!collider2D.CompareTag("Player")) return;

        Vector3 savePos = collider2D.transform.position;
        ArchiveManager.Save(savePos);
        Debug.Log($"[ArchivePoint] {source} save at {savePos}");
    }
}
