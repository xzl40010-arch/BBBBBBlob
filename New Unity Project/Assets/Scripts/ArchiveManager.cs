// 郑佳鑫
// 2026.2.5 第一次修改：存档管理：记录与读取最近存档点

// 阳成垚
// 2026.2.12 第二次修改：增加文件存档备份

using UnityEngine;
using System.IO;
using System;

public static class ArchiveManager
{
    private const string HasKey = "Archive_Has";
    private const string XKey = "Archive_X";
    private const string YKey = "Archive_Y";
    private const string ZKey = "Archive_Z";

    // 文件存档路径
    private static string saveFilePath;

    private static bool cacheLoaded = false;
    private static bool hasArchive = false;
    private static Vector3 cachedPosition = Vector3.zero;

    // 静态构造函数：初始化文件路径
    static ArchiveManager()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "archive.json");
    }

    public static void Save(Vector3 position)
    {
        cachedPosition = position;
        hasArchive = true;
        cacheLoaded = true;

        // 保存到PlayerPrefs
        PlayerPrefs.SetInt(HasKey, 1);
        PlayerPrefs.SetFloat(XKey, position.x);
        PlayerPrefs.SetFloat(YKey, position.y);
        PlayerPrefs.SetFloat(ZKey, position.z);
        PlayerPrefs.Save();

        // 同时保存到文件
        SaveToFile(position);

        Debug.Log($"[Archive] Saved position: {position}");
    }

    public static bool TryGetLatestPosition(out Vector3 position)
    {
        if (!cacheLoaded)
        {
            LoadCache();
        }

        if (hasArchive)
        {
            position = cachedPosition;
            return true;
        }

        position = Vector3.zero;
        return false;
    }

    public static void Clear()
    {
        hasArchive = false;
        cacheLoaded = true;
        cachedPosition = Vector3.zero;

        // 清除PlayerPrefs
        PlayerPrefs.DeleteKey(HasKey);
        PlayerPrefs.DeleteKey(XKey);
        PlayerPrefs.DeleteKey(YKey);
        PlayerPrefs.DeleteKey(ZKey);
        PlayerPrefs.Save();

        // 同时删除存档文件
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
        }

        Debug.Log("[Archive] Cleared");
    }

    private static void LoadCache()
    {
        cacheLoaded = true;

        // 先从PlayerPrefs加载
        if (PlayerPrefs.GetInt(HasKey, 0) == 1)
        {
            float x = PlayerPrefs.GetFloat(XKey, 0f);
            float y = PlayerPrefs.GetFloat(YKey, 0f);
            float z = PlayerPrefs.GetFloat(ZKey, 0f);
            cachedPosition = new Vector3(x, y, z);
            hasArchive = true;
        }
        // 如果PlayerPrefs没有，尝试从文件加载
        else
        {
            LoadFromFile();
        }
    }

    // ========== 文件存档相关方法 ==========

    [Serializable]
    private class SaveData
    {
        public float x;
        public float y;
        public float z;
        public string saveTime;
    }

    private static void SaveToFile(Vector3 position)
    {
        try
        {
            SaveData data = new SaveData
            {
                x = position.x,
                y = position.y,
                z = position.z,
                saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(saveFilePath, json);
            
            Debug.Log($"[Archive] File saved to: {saveFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Archive] Failed to save file: {e.Message}");
        }
    }

    private static void LoadFromFile()
    {
        try
        {
            if (File.Exists(saveFilePath))
            {
                string json = File.ReadAllText(saveFilePath);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                
                if (data != null)
                {
                    cachedPosition = new Vector3(data.x, data.y, data.z);
                    hasArchive = true;
                    Debug.Log($"[Archive] Loaded from file: {cachedPosition}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[Archive] Failed to load file: {e.Message}");
        }
    }

    // ========== 获取存档信息 ==========
    // 获取存档文件的保存时间
    public static string GetLastSaveTime()
    {
        if (File.Exists(saveFilePath))
        {
            return File.GetLastWriteTime(saveFilePath).ToString("yyyy-MM-dd HH:mm:ss");
        }
        return "无存档";
    }

    // 获取存档文件的完整路径
    public static string GetSaveFilePath()
    {
        return saveFilePath;
    }
}