// 郑佳鑫
// 2026.2.5 第一次修改：存档管理：记录与读取最近存档点
using UnityEngine;

public static class ArchiveManager
{
    private const string HasKey = "Archive_Has";
    private const string XKey = "Archive_X";
    private const string YKey = "Archive_Y";
    private const string ZKey = "Archive_Z";

    private static bool cacheLoaded = false;
    private static bool hasArchive = false;
    private static Vector3 cachedPosition = Vector3.zero;

    public static void Save(Vector3 position)
    {
        cachedPosition = position;
        hasArchive = true;
        cacheLoaded = true;

        PlayerPrefs.SetInt(HasKey, 1);
        PlayerPrefs.SetFloat(XKey, position.x);
        PlayerPrefs.SetFloat(YKey, position.y);
        PlayerPrefs.SetFloat(ZKey, position.z);
        PlayerPrefs.Save();

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

        PlayerPrefs.DeleteKey(HasKey);
        PlayerPrefs.DeleteKey(XKey);
        PlayerPrefs.DeleteKey(YKey);
        PlayerPrefs.DeleteKey(ZKey);
        PlayerPrefs.Save();

        Debug.Log("[Archive] Cleared");
    }

    private static void LoadCache()
    {
        cacheLoaded = true;

        if (PlayerPrefs.GetInt(HasKey, 0) == 1)
        {
            float x = PlayerPrefs.GetFloat(XKey, 0f);
            float y = PlayerPrefs.GetFloat(YKey, 0f);
            float z = PlayerPrefs.GetFloat(ZKey, 0f);
            cachedPosition = new Vector3(x, y, z);
            hasArchive = true;
        }
        else
        {
            hasArchive = false;
        }
    }
}
