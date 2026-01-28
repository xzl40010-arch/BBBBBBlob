//郑佳鑫
//2026.1.28：第一次修改，气化态触碰后转为流动态，并播放“滋滋滋”音效

using UnityEngine;
public class CoolingPlate : MonoBehaviour
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

        // 气化态触碰立即转为流动态
        bool changed = player.EnterLiquidFromGas();

        //播放“滋滋滋”音效
        audiocontroller.PlaySfx(audiocontroller.fizzClip);
    }
}
