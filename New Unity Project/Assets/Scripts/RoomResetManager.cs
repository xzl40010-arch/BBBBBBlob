// 郑佳鑫
// 2026.2.11：通用房间重置系统
// 任何物体只需添加 Resettable 组件即可支持T键重置
using System.Collections.Generic;
using UnityEngine;
// 保存物体的所有数据
[System.Serializable]
public class SavedObjectData
{
    public int instanceId;
    public string objectName;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public int layer;
    public string tag;
    public bool isDestroyed;
    public GameObject currentInstance;
    
    // 视觉数据
    public Sprite sprite;
    public Color color;
    public string sortingLayerName;
    public int sortingOrder;
    
    // 碰撞体数据
    public bool hasBoxCollider;
    public Vector2 colliderSize;
    public Vector2 colliderOffset;
    public bool isTrigger;
    
    // 预制体引用（如果有）
    public GameObject prefab;
    
    // 原始物体上的所有组件类型（用于重建）
    public List<string> componentTypes = new List<string>();
}


// 房间重置管理器 - 通用版

public class RoomResetManager : MonoBehaviour
{
    private static RoomResetManager instance;
    public static RoomResetManager Instance => instance;

    private List<SavedObjectData> savedObjects = new List<SavedObjectData>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            Debug.Log("[RoomResetManager] 管理器已创建");
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    
    // 注册一个可重置物体
    
    public static void RegisterResettable(GameObject obj, GameObject prefab = null)
    {
        if (instance == null)
        {
            Debug.LogWarning("[RoomResetManager] 没有找到管理器实例！请在场景中创建 RoomResetManager");
            return;
        }

        int id = obj.GetInstanceID();
        
        // 检查是否已注册
        foreach (var data in instance.savedObjects)
        {
            if (data.instanceId == id)
            {
                return;
            }
        }

        // 保存物体数据
        SavedObjectData savedData = new SavedObjectData();
        savedData.instanceId = id;
        savedData.objectName = obj.name;
        savedData.position = obj.transform.position;
        savedData.rotation = obj.transform.rotation;
        savedData.scale = obj.transform.localScale;
        savedData.layer = obj.layer;
        savedData.tag = obj.tag;
        savedData.isDestroyed = false;
        savedData.currentInstance = obj;
        savedData.prefab = prefab;

        // 保存 SpriteRenderer 数据
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            savedData.sprite = sr.sprite;
            savedData.color = sr.color;
            savedData.sortingLayerName = sr.sortingLayerName;
            savedData.sortingOrder = sr.sortingOrder;
        }

        // 保存 Collider 数据
        BoxCollider2D col = obj.GetComponent<BoxCollider2D>();
        if (col != null)
        {
            savedData.hasBoxCollider = true;
            savedData.colliderSize = col.size;
            savedData.colliderOffset = col.offset;
            savedData.isTrigger = col.isTrigger;
        }

        // 保存组件类型列表
        Component[] components = obj.GetComponents<Component>();
        foreach (Component comp in components)
        {
            if (comp != null && !(comp is Transform) && !(comp is Resettable))
            {
                savedData.componentTypes.Add(comp.GetType().AssemblyQualifiedName);
            }
        }

        instance.savedObjects.Add(savedData);
        Debug.Log($"[RoomResetManager] 已注册: {obj.name} at {savedData.position}, 总数: {instance.savedObjects.Count}");
    }

   
    // 标记物体已被销毁
   
    public static void MarkDestroyed(GameObject obj)
    {
        if (instance == null) return;

        int id = obj.GetInstanceID();
        foreach (var data in instance.savedObjects)
        {
            if (data.instanceId == id || data.currentInstance == obj)
            {
                data.isDestroyed = true;
                data.currentInstance = null;
                Debug.Log($"[RoomResetManager] 已标记销毁: {data.objectName} at {data.position}");
                return;
            }
        }
    }

    
    // 重置房间
    
    public static void ResetRoom()
    {
        if (instance == null)
        {
            Debug.LogWarning("[RoomResetManager] 没有找到管理器实例！");
            return;
        }

        Debug.Log($"[RoomResetManager] 正在重置房间... 已保存 {instance.savedObjects.Count} 个物体");

        int recreatedCount = 0;
        foreach (var data in instance.savedObjects)
        {
            if (data.isDestroyed || data.currentInstance == null)
            {
                GameObject newObj = instance.RecreateObject(data);
                if (newObj != null)
                {
                    data.currentInstance = newObj;
                    data.instanceId = newObj.GetInstanceID();
                    data.isDestroyed = false;
                    recreatedCount++;
                }
            }
        }

        Debug.Log($"[RoomResetManager] 房间重置完成！重建了 {recreatedCount} 个物体");
    }

   
    // 重建物体
    
    private GameObject RecreateObject(SavedObjectData data)
    {
        GameObject newObj;

        // 如果有预制体，使用预制体实例化
        if (data.prefab != null)
        {
            newObj = Instantiate(data.prefab, data.position, data.rotation);
            newObj.name = data.objectName;
            newObj.transform.localScale = data.scale;
            Debug.Log($"[RoomResetManager] 使用预制体重建: {data.objectName}");
        }
        else
        {
            // 没有预制体，手动重建
            newObj = new GameObject(data.objectName + "_Recreated");
            newObj.transform.position = data.position;
            newObj.transform.rotation = data.rotation;
            newObj.transform.localScale = data.scale;
            newObj.layer = data.layer;
            newObj.tag = data.tag;

            // 添加 SpriteRenderer
            if (data.sprite != null)
            {
                SpriteRenderer sr = newObj.AddComponent<SpriteRenderer>();
                sr.sprite = data.sprite;
                sr.color = data.color;
                sr.sortingLayerName = data.sortingLayerName;
                sr.sortingOrder = data.sortingOrder;
            }

            // 添加 Collider
            if (data.hasBoxCollider)
            {
                BoxCollider2D col = newObj.AddComponent<BoxCollider2D>();
                col.size = data.colliderSize;
                col.offset = data.colliderOffset;
                col.isTrigger = data.isTrigger;
            }

            // 重建组件
            foreach (string typeName in data.componentTypes)
            {
                System.Type type = System.Type.GetType(typeName);
                if (type != null && !newObj.GetComponent(type))
                {
                    try
                    {
                        newObj.AddComponent(type);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"[RoomResetManager] 无法添加组件 {typeName}: {e.Message}");
                    }
                }
            }

            // 确保添加 Resettable 组件
            if (newObj.GetComponent<Resettable>() == null)
            {
                newObj.AddComponent<Resettable>();
            }

            Debug.Log($"[RoomResetManager] 手动重建: {data.objectName}");
        }

        return newObj;
    }
}

