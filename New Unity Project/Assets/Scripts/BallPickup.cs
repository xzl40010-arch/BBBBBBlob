//郑佳鑫
//2026.2.9 第一次修改：球拾取点：玩家接触后尝试从绑定的生成点获取球，成功后销毁或隐藏自己
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BallPickup : MonoBehaviour
{
    [SerializeField] private bool destroyOnPickup = true;

    private BallSpawnPoint spawnPoint;
    private bool picked;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    public bool IsBound => spawnPoint != null;

    public void BindToSpawnPoint(BallSpawnPoint source)
    {
        spawnPoint = source;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (picked)
        {
            return;
        }

        PlayerBallHolder holder = other.GetComponentInParent<PlayerBallHolder>();
        if (holder == null)
        {
            return;
        }

        if (spawnPoint != null && spawnPoint.TryGiveBallTo(holder))
        {
            picked = true;
            if (destroyOnPickup)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void OnDestroy()
    {
        if (!picked && spawnPoint != null)
        {
            spawnPoint.NotifyBallDestroyedWithoutPickup();
        }
    }
}
