
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] AudioSource BgmAudio;
    [SerializeField] AudioSource SfxAudio;
    [SerializeField] AudioSource MoveAudio;

    [Header("移动音效淡入淡出")]
    [SerializeField] private float fadeInTime = 0.2f;   // 淡入时间
    [SerializeField] private float fadeOutTime = 0.3f;  // 淡出时间

    [Header("音源")]
    public AudioClip bgm;           //BGM
    public AudioClip clickClip;     //菜单按键音效

    public AudioClip sizzleClip;    //状态转换相关音效
    public AudioClip fizzClip;
    public AudioClip toGasClip;
    public AudioClip toSolidClip;
    
    public AudioClip solidMoveClip; //移动相关音效
    public AudioClip solidFallClip;
    public AudioClip liquidMoveClip;
    public AudioClip liquidFallClip;
    public AudioClip gasMoveClip;

    public AudioClip springClip;   //弹簧音效
    public AudioClip thronKillClip;//尖刺音效
    public AudioClip wallCrackClip;//撞墙音效
    public AudioClip ballPickClip; //小球拾取音效
    public AudioClip portalClip;   //传送门音效
    public AudioClip savepointClip;//存档点音效

    private Coroutine fadeCoroutine;

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

       // 专门播放移动音效的方法（带淡入淡出）
    public void PlayMoveSound(AudioClip clip, bool forceRestart = false)
    {
        if (MoveAudio == null || clip == null) return;
        
        if (forceRestart) MoveAudio.Stop();

        // 如果已经在播放同一个音效，且不强制重启，则忽略
        if (MoveAudio.isPlaying && MoveAudio.clip == clip && !forceRestart)
        {
            return;
        }
        
        // 如果有正在进行的淡入淡出，停止它
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        // 开始播放新音效（带淡入淡出过渡）
        fadeCoroutine = StartCoroutine(PlayWithFade(clip));
    }
    
    // 停止移动音效（带淡出）
    public void StopMoveSound()
    {
        if (MoveAudio == null || !MoveAudio.isPlaying) return;
        
        // 如果有正在进行的淡入淡出，停止它
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        // 开始淡出
        fadeCoroutine = StartCoroutine(FadeOutAndStop());
    }
    // 立即停止移动音效（无淡出）
    public void StopMoveSoundImmediate()
    {
        if (MoveAudio != null)
        {
            MoveAudio.Stop();
        }
    }
    // 检查是否正在播放移动音效
    public bool IsMoveSoundPlaying()
    {
        return MoveAudio != null && MoveAudio.isPlaying;
    }
    
    // ========== 淡入淡出协程 ==========
    
    private IEnumerator PlayWithFade(AudioClip newClip)
    {
        // 如果正在播放其他音效，先淡出
        if (MoveAudio.isPlaying && MoveAudio.clip != newClip)
        {
            float timer = 0f;
            float startVolume = MoveAudio.volume;
            
            while (timer < fadeOutTime)
            {
                timer += Time.deltaTime;
                MoveAudio.volume = Mathf.Lerp(startVolume, 0f, timer / fadeOutTime);
                yield return null;
            }
            
            MoveAudio.Stop();
        }
        
        // 设置新音效并淡入
        MoveAudio.clip = newClip;
        MoveAudio.volume = 0f; // 从0音量开始
        MoveAudio.Play();
        
        float fadeTimer = 0f;
        while (fadeTimer < fadeInTime)
        {
            fadeTimer += Time.deltaTime;
            MoveAudio.volume = Mathf.Lerp(0f, 1f, fadeTimer / fadeInTime);
            yield return null;
        }
        
        MoveAudio.volume = 1f; // 确保音量达到最大
    }
    
    private IEnumerator FadeOutAndStop()
    {
        float timer = 0f;
        float startVolume = MoveAudio.volume;
        
        while (timer < fadeOutTime)
        {
            timer += Time.deltaTime;
            MoveAudio.volume = Mathf.Lerp(startVolume, 0f, timer / fadeOutTime);
            yield return null;
        }
        
        MoveAudio.Stop();
        MoveAudio.volume = 1f; // 重置音量，为下次播放做准备
    }

}
