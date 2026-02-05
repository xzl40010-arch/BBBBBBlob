using UnityEngine;
using UnityEngine.UI;

// 将此脚本挂到 Canvas 下的 Slider 上，
// 自动把滑块初始值设为中间，并控制 AudioManager/BGM 的音量。
public class VolumeSlider : MonoBehaviour
{
    [Header("UI 引用")]
    [SerializeField] private Slider slider;

    [Header("BGM 引用/查找")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private Transform audioManager;
    [SerializeField] private string bgmChildName = "BGM";
    [SerializeField] private bool autoFindOnAwake = true;

    [Header("可选：持久化设置")]
    [SerializeField] private bool saveToPlayerPrefs = false;
    [SerializeField] private string prefsKey = "BGMVolume";

    void Awake()
    {
        if (slider == null) slider = GetComponent<Slider>();

        if (autoFindOnAwake && bgmSource == null)
        {
            if (audioManager == null)
            {
                var amGo = GameObject.Find("AudioManager");
                if (amGo != null) audioManager = amGo.transform;
            }

            if (audioManager != null)
            {
                var child = audioManager.Find(bgmChildName);
                if (child != null) bgmSource = child.GetComponent<AudioSource>();
            }
        }
    }

    void Start()
    {
        if (slider == null)
        {
            Debug.LogWarning("VolumeSlider: 未找到 Slider 组件。");
            return;
        }

        float middle = (slider.minValue + slider.maxValue) * 0.5f;

        if (saveToPlayerPrefs && PlayerPrefs.HasKey(prefsKey))
        {
            middle = PlayerPrefs.GetFloat(prefsKey, middle);
        }

        slider.SetValueWithoutNotify(middle);
        ApplyVolume(middle);
        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    void OnDestroy()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    private void OnSliderChanged(float value)
    {
        ApplyVolume(value);
        if (saveToPlayerPrefs)
        {
            PlayerPrefs.SetFloat(prefsKey, value);
        }
    }

    private void ApplyVolume(float sliderValue)
    {
        if (bgmSource == null) return;

        // 将滑块值映射到 0..1（AudioSource.volume 的范围）
        float normalized = Mathf.InverseLerp(slider.minValue, slider.maxValue, sliderValue);
        bgmSource.volume = Mathf.Clamp01(normalized);
    }
}
