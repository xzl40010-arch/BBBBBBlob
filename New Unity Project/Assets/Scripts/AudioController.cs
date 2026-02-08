using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] AudioSource BgmAudio;
    [SerializeField] AudioSource SfxAudio;
    public AudioClip bgm;
    public AudioClip sizzleClip;//状态转换相关音效
    public AudioClip fizzClip;
    public AudioClip toGasClip;
    public AudioClip toSolidClip;
    public AudioClip thronKillClip;//尖刺音效
    public AudioClip solidMoveClip;//移动相关音效
    public AudioClip liquidMoveClip;
    public AudioClip gasMoveClip;
    
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

}
