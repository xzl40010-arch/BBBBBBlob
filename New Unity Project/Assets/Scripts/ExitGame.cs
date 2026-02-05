using UnityEngine;
using UnityEngine.UI;

public class ExitGame : MonoBehaviour
{
    [SerializeField] private Button button;

    void Reset()
    {
        if (button == null) button = GetComponent<Button>();
    }

    void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(Quit);
        }
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 在编辑器内停止播放
#elif UNITY_WEBGL
        Debug.Log("WebGL 不支持关闭应用，Quit 调用将被忽略。");
#else
        Application.Quit(); // 在打包后的独立平台(Windows/macOS/Linux)退出应用
#endif
    }
}
