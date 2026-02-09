//郑佳鑫
//2026.2.9 第一次修改：球生成点：负责在场景中生成球，管理球的状态，并提供接口给拾取点尝试获取球和请求重生
using System.Collections;
using UnityEngine;

public class BallSpawnPoint : MonoBehaviour
{
    [Header("Ball Prefab (must have BallPickup)")]
    [SerializeField] private GameObject ballPrefab;

    [Header("Visuals")]
    [SerializeField] private GameObject emptyVisual;

    [Header("Respawn")]
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private float respawnDelay = 0.1f;

    [Header("Adopt Existing Ball")]
    [SerializeField] private bool adoptExistingBall = true;
    [SerializeField] private float adoptRadius = 0.2f;

    private GameObject currentBall;
    private bool isBallAvailable;
    private bool respawnRequested;
    private Coroutine respawnCoroutine;

    public bool HasBall => isBallAvailable;

    private void Start()
    {
        if (adoptExistingBall && TryAdoptExistingBall())
        {
            return;
        }

        if (spawnOnStart)
        {
            SpawnBall();
        }
        else
        {
            UpdateVisuals();
        }
    }

    private bool TryAdoptExistingBall()
    {
        BallPickup childPickup = GetComponentInChildren<BallPickup>();
        if (childPickup != null && !childPickup.IsBound)
        {
            BindExistingBall(childPickup.gameObject, childPickup);
            return true;
        }

        if (adoptRadius > 0f)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, adoptRadius);
            for (int i = 0; i < hits.Length; i++)
            {
                BallPickup pickup = hits[i].GetComponent<BallPickup>();
                if (pickup != null && !pickup.IsBound)
                {
                    BindExistingBall(pickup.gameObject, pickup);
                    return true;
                }
            }
        }

        return false;
    }

    private void BindExistingBall(GameObject ballObject, BallPickup pickup)
    {
        currentBall = ballObject;
        pickup.BindToSpawnPoint(this);
        isBallAvailable = true;
        respawnRequested = false;
        UpdateVisuals();
        Debug.Log("[BallSpawnPoint] Adopted existing ball.");
    }

    public void SpawnBall()
    {
        if (ballPrefab == null)
        {
            Debug.LogWarning("[BallSpawnPoint] Missing ballPrefab.");
            return;
        }

        if (currentBall != null)
        {
            return;
        }

        currentBall = Instantiate(ballPrefab, transform.position, Quaternion.identity);
        BallPickup pickup = currentBall.GetComponent<BallPickup>();
        if (pickup != null)
        {
            pickup.BindToSpawnPoint(this);
        }
        else
        {
            Debug.LogWarning("[BallSpawnPoint] Ball prefab has no BallPickup.");
        }

        isBallAvailable = true;
        respawnRequested = false;
        UpdateVisuals();
        Debug.Log("[BallSpawnPoint] Ball spawned.");
    }

    public bool TryGiveBallTo(PlayerBallHolder holder)
    {
        if (!isBallAvailable || holder == null)
        {
            return false;
        }

        if (!holder.TryHoldBall(this))
        {
            return false;
        }

        isBallAvailable = false;
        currentBall = null;
        UpdateVisuals();
        Debug.Log("[BallSpawnPoint] Ball picked up.");
        return true;
    }

    public void NotifyBallDestroyedWithoutPickup()
    {
        if (!isBallAvailable)
        {
            return;
        }

        isBallAvailable = false;
        currentBall = null;
        UpdateVisuals();
        RequestRespawn();
        Debug.Log("[BallSpawnPoint] Ball destroyed before pickup, respawn requested.");
    }

    public void RequestRespawn()
    {
        if (respawnRequested)
        {
            return;
        }

        respawnRequested = true;
        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
        }

        respawnCoroutine = StartCoroutine(RespawnAfterDelay());
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        SpawnBall();
    }

    private void UpdateVisuals()
    {
        if (emptyVisual != null)
        {
            emptyVisual.SetActive(!isBallAvailable);
        }
    }
}
