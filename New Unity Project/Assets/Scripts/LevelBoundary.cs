//阳成垚
//2026.2.26 关卡边界设置
//每个关卡创建一个空物体，添加 LevelBoundary 脚本
//在 Inspector 中设置 Bounds 的 Center 和 Size。Center: 关卡中心点坐标Size: 关卡的宽度和高度

using UnityEngine;

public class LevelBoundary : MonoBehaviour
{
    [Header("关卡信息")]
    public string levelName = "Level 1";
    public Bounds bounds; // 在Inspector中设置
    
    [Header("可视化")]
    [SerializeField] private Color gizmoColor = new Color(0, 1, 0, 0.3f);
    
    // 获取关卡中心点
    public Vector3 centerPosition => bounds.center;
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(bounds.center, bounds.size);
    }
}