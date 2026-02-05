//编辑器内：点击按钮会停止播放模式（便于调试）。
//已打包的独立平台（Windows/macOS/Linux）：点击按钮会正常退出应用。
//WebGL：浏览器不允许主动关闭页面，Quit 会被忽略并仅输出日志。
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EscOptionTrigger : MonoBehaviour
{
    public Button optionButton;
    public bool simulatePointerClick;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionButton == null) return;

            if (simulatePointerClick)
            {
                ExecuteEvents.Execute(
                    optionButton.gameObject,
                    new PointerEventData(EventSystem.current),
                    ExecuteEvents.pointerClickHandler
                );
            }
            else
            {
                if (optionButton.interactable)
                {
                    optionButton.onClick.Invoke();
                }
            }
        }
    }
}
