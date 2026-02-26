//阳成垚：
//2026.2.26 创建固定摄像机脚本，可以切换到指定关卡的中心

//阳成垚：
//2026.2.26 创建固定摄像机脚本，可以切换到指定关卡的中心
//2026.2.26 修改：去掉padding，直接覆盖关卡边界

using UnityEngine;

public class FixedCamera : MonoBehaviour
{
    [Header("摄像机设置")]
    [SerializeField] private Camera cam;
    
    [Header("当前关卡")]
    [SerializeField] private LevelBoundary currentLevel;
    
    private void Start()
    {
        if (cam == null)
            cam = GetComponent<Camera>();
        
        // 设置为正交模式
        cam.orthographic = true;
        
        // 如果有初始关卡，立即定位并调整大小
        if (currentLevel != null)
        {
            SetLevel(currentLevel);
        }
    }
    
    /// <summary>
    /// 切换到指定关卡
    /// </summary>
    public void SetLevel(LevelBoundary level)
    {
        if (level == null) return;
        
        currentLevel = level;
        
        // 将摄像机移动到关卡中心
        transform.position = new Vector3(
            level.centerPosition.x,
            level.centerPosition.y,
            transform.position.z // 保持Z轴不变
        );
        
        // 调整摄像机大小以适应关卡
        AdjustCameraSize(level);
        
        Debug.Log($"摄像机已切换到关卡: {level.levelName}，位置: {transform.position}，大小: {cam.orthographicSize}");
    }
    
    /// <summary>
    /// 根据关卡边界调整摄像机大小
    /// </summary>
    private void AdjustCameraSize(LevelBoundary level)
    {
        if (cam == null || level == null) return;
        
        // 获取关卡边界
        Bounds levelBounds = level.bounds;
        
        // 关卡尺寸
        float levelWidth = levelBounds.size.x;
        float levelHeight = levelBounds.size.y;
        
        // 计算垂直方向需要的摄像机大小
        float verticalSize = levelHeight / 2f;
        
        // 计算水平方向需要的摄像机大小（需要考虑屏幕宽高比）
        float horizontalSize = levelWidth / (2f * cam.aspect);
        
        // 取较大值，确保整个关卡在视野内
        float targetSize = Mathf.Max(verticalSize, horizontalSize);
        
        // 应用计算出的摄像机大小
        cam.orthographicSize = targetSize;
        
        Debug.Log($"关卡尺寸: 宽={levelWidth}, 高={levelHeight}");
        Debug.Log($"摄像机大小: {targetSize} (可视范围: 宽={targetSize*2*cam.aspect}, 高={targetSize*2})");
    }
    
    // 在编辑器中实时预览
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        if (cam == null) cam = GetComponent<Camera>();
        if (currentLevel != null)
        {
            // 在编辑器中预览效果
            transform.position = new Vector3(
                currentLevel.centerPosition.x,
                currentLevel.centerPosition.y,
                transform.position.z
            );
            
            if (cam != null)
            {
                cam.orthographic = true;
                AdjustCameraSize(currentLevel);
            }
        }
    }
    
    // 可视化调试
    private void OnDrawGizmos()
    {
        if (cam == null || currentLevel == null) return;
        
        // 绘制关卡边界（白色）
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(currentLevel.bounds.center, currentLevel.bounds.size);
        
        // 绘制摄像机实际视口范围（黄色）
        float visualHeight = cam.orthographicSize * 2f;
        float visualWidth = visualHeight * cam.aspect;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(visualWidth, visualHeight, 0));
    }
}