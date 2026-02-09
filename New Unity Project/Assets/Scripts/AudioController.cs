
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] AudioSource BgmAudio;
    [SerializeField] AudioSource SfxAudio;
    [SerializeField] AudioSource MoveAudio;

    public AudioClip bgm;           //BGM
    public AudioClip clickClip;     //菜单按键音效

    public AudioClip sizzleClip;    //状态转换相关音效
    public AudioClip fizzClip;
    public AudioClip toGasClip;
    public AudioClip toSolidClip;
    
    public AudioClip solidMoveClip; //移动相关音效
    public AudioClip solidFallClip;
    public AudioClip liquidMoveClip;
    public AudioClip gasMoveClip;

    public AudioClip springClip;   //弹簧音效
    public AudioClip thronKillClip;//尖刺音效
    public AudioClip ballPickClip; //小球拾取音效
    public AudioClip portalClip;   //传送门音效
    public AudioClip savepointClip;//存档点音效

    private void Start()
    {
        BgmAudio.clip = bgm;
        BgmAudio.loop=true;
        BgmAudio.Play();
    }

    public void PlaySfx(AudioClip clip)
    {
        SfxAudio.PlayOneShot(clip);
    }

       // 专门播放移动音效的方法
    public void PlayMoveSound(AudioClip clip, bool forceRestart = false)
    {
        if (MoveAudio == null || clip == null) return;
        
        // 如果已经在播放同一个音效，且不强制重启，则忽略
        if (MoveAudio.isPlaying && MoveAudio.clip == clip && !forceRestart)
        {
            return;
        }
        
        // 如果正在播放其他音效，停止它
        if (MoveAudio.isPlaying)
        {
            MoveAudio.Stop();
        }
        
        MoveAudio.clip = clip;
        MoveAudio.Play();
    }
    
    // 停止移动音效
    public void StopMoveSound()
    {
        if (MoveAudio != null && MoveAudio.isPlaying)
        {
            MoveAudio.Stop();
        }
    }
    
    // 检查是否正在播放移动音效
    public bool IsMoveSoundPlaying()
    {
        return MoveAudio != null && MoveAudio.isPlaying;
    }

}
