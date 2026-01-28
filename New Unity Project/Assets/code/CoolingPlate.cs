//郑佳鑫
//2026.1.28：第一次修改，气化态触碰后转为流动态，并播放“滋滋滋”音效

using UnityEngine;
public class CoolingPlate : MonoBehaviour
{
    // 播放“滋滋滋”音效
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fizzClip;

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

        // 气化态触碰立即转为流动态
        bool changed = player.EnterLiquidFromGas();
        if (changed && audioSource != null && fizzClip != null)
        {
            audioSource.PlayOneShot(fizzClip);
        }
    }
}
