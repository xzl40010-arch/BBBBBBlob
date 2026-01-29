//郑佳鑫
//2026.1.28：第一次修改，凝固态触碰立即转为流动态，并播放“滋滋滋”音效
using UnityEngine;

public class HotPlate : MonoBehaviour
{
    
    AudioController audiocontroller;

    private void Start()
    {
        audiocontroller=GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioController>();
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
        
        // 播放“呲——”音效
        audiocontroller.PlaySfx(audiocontroller.sizzleClip);
    }
}
