//2026.2.2 郑佳鑫
//第一次修改：摄像机跟随脚本
//2026.2.5 第二次修改：解决兼容性冲突，增加自动寻找玩家功能
//2.5 文振一
//第二次修改：解决平滑跟随导致的人物角色轻微糊/抖动问题
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField] private float followSpeed = 10f;
    [SerializeField] private bool autoFindTarget = true;

    private void Awake()
    {
        TryResolveTarget("Awake");
    }

    private void OnEnable()
    {
        TryResolveTarget("OnEnable");
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            TryResolveTarget("LateUpdate");
            if (target == null)
            {
                return;
            }
        }

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        //26.2.5 新增
        int PPU = 24;
        float unit = 1f / PPU;
        Vector3 p = transform.position;
        p.x = Mathf.Round(p.x / unit) * unit;
        p.y = Mathf.Round(p.y / unit) * unit;
        transform.position = p;
    }

    private void TryResolveTarget(string source)
    {
        if (!autoFindTarget || target != null)
        {
            return;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            target = playerObj.transform;
            Debug.Log($"[CameraFollow] Target auto-assigned from tag Player at {source}");
        }
        else
        {
            Debug.LogWarning($"[CameraFollow] Target missing at {source}. Assign in Inspector or tag Player.");
        }
    }
}
