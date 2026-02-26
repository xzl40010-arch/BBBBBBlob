//阳成垚
//2026.2.26 关卡切换触发器
//在两个关卡连接处放置一个 Collider2D (设为 Trigger)
//添加 LevelTransition 脚本
//将 Target Level 设置为要切换到的关卡的 LevelBoundary


using UnityEngine;

public class LevelTransition : MonoBehaviour
{
    [Header("目标关卡")]
    [SerializeField] private LevelBoundary targetLevel;
    [SerializeField] private string playerTag = "Player";
    
    [Header("过渡效果")]
    [SerializeField] private bool useFade = true;
    [SerializeField] private float fadeDuration = 1f;
    
    private FixedCamera fixedCamera;
    
    private void Start()
    {
        fixedCamera = FindObjectOfType<FixedCamera>();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (fixedCamera == null || targetLevel == null) return;
        
        if (useFade)
        {
            StartCoroutine(TransitionWithFade());
        }
        else
        {
            fixedCamera.SetLevel(targetLevel);
        }
    }
    
    private System.Collections.IEnumerator TransitionWithFade()
    {
        // 这里可以添加淡入淡出效果
        // 比如使用 CanvasGroup 或 Screen Fader
        
        yield return new WaitForSeconds(fadeDuration / 2f);
        
        fixedCamera.SetLevel(targetLevel);
        
        yield return new WaitForSeconds(fadeDuration / 2f);
    }
}