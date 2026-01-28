//郑佳鑫
//2026.1.28：第一次修改，凝固态触碰立即转为流动态，并播放“滋滋滋”音效

using UnityEngine;

public class HotPlate : MonoBehaviour
{
    // 播放“呲——”音效
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sizzleClip;

    private void Awake()
    {
        // 兼容未手动绑定的情况
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 只处理玩家
        if (!collision.collider.CompareTag("Player"))
        {
            return;
        }

        Player player = collision.collider.GetComponent<Player>();
        if (player == null)
        {
            return;
        }

        // 凝固态触碰立即转为流动态
        bool changed = player.EnterLiquidFromSolid();
        if (changed && audioSource != null && sizzleClip != null)
        {
            audioSource.PlayOneShot(sizzleClip);
        }
    }
}
