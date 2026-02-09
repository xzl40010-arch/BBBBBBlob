//郑佳鑫
//2026.2.9 第一次修改：玩家球持有器：管理玩家持有的球状态，提供接口尝试持球和消耗球，监听 OnPlayerStateChanged，只要不再是固态，就清掉持球
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerBallHolder : MonoBehaviour
{
    [Header("Optional hold point")]
    [SerializeField] private Transform holdPoint;

    [Header("Logging")]
    [SerializeField] private bool logEvents = true;

    private Player player;
    private HeldBallToken heldToken;

    public bool HasBall => heldToken != null;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void OnPlayerStateChanged(Player.PlayerState newState)
    {
        HandleStateChanged(newState);
    }

    public void HandleStateChanged(Player.PlayerState newState)
    {
        if (newState != Player.PlayerState.Solid)
        {
            ClearHeldBallWithoutUse();
        }
    }

    public bool TryHoldBall(BallSpawnPoint source)
    {
        if (player == null)
        {
            return false;
        }

        if (HasBall)
        {
            return false;
        }

        if (player.CurrentState != Player.PlayerState.Liquid)
        {
            return false;
        }

        if (!player.TrySwitchToSolid())
        {
            return false;
        }

        GameObject tokenObj = new GameObject("HeldBallToken");
        tokenObj.transform.SetParent(holdPoint != null ? holdPoint : transform, false);

        heldToken = tokenObj.AddComponent<HeldBallToken>();
        heldToken.Initialize(source);

        if (logEvents)
        {
            Debug.Log("[PlayerBallHolder] Ball picked and player switched to Solid.");
        }

        return true;
    }

    public void ConsumeHeldBall()
    {
        if (heldToken == null)
        {
            return;
        }

        heldToken.MarkUsed();
        Destroy(heldToken.gameObject);
        heldToken = null;

        if (logEvents)
        {
            Debug.Log("[PlayerBallHolder] Ball consumed.");
        }
    }

    public void ClearHeldBallWithoutUse()
    {
        if (heldToken == null)
        {
            return;
        }

        Destroy(heldToken.gameObject);
        heldToken = null;

        if (logEvents)
        {
            Debug.Log("[PlayerBallHolder] Ball cleared without use.");
        }
    }
}
