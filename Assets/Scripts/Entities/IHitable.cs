using UnityEngine;
public interface IHitable
{
    bool GetHit(int damage, GameObject source, out int expReward, out int coinsReward);
}