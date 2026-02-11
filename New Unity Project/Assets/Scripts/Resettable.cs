// 郑佳鑫
// 2026.2.11：通用可重置组件
// 挂在任何需要重置的物体上，支持T键重置功能
using UnityEngine;

// 通用可重置组件
// 任何需要在T键重置时被恢复的物体都可以添加这个组件
// 例如：可破坏平台、会消失的机关等

public class Resettable : MonoBehaviour
{
    [Header("可选：预制体引用（用于重建）")]
    [Tooltip("如果设置了预制体，重置时会使用预制体重建；否则会尝试手动重建")]
    [SerializeField] private GameObject prefab;

    [Header("调试")]
    [SerializeField] private bool logDebug = false;

    private void Start()
    {
        RoomResetManager.RegisterResettable(gameObject, prefab);
        
        if (logDebug)
        {
            Debug.Log($"[Resettable] 已注册: {gameObject.name}");
        }
    }

    private void OnDestroy()
    {
        if (Application.isPlaying)
        {
            RoomResetManager.MarkDestroyed(gameObject);
            
            if (logDebug)
            {
                Debug.Log($"[Resettable] 已标记销毁: {gameObject.name}");
            }
        }
    }
}

