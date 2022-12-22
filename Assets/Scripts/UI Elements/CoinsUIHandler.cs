using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinsUIHandler : MonoBehaviour
{
    [SerializeField] Text coinsText;

    const string COINS_FORMAT = "Coins: {0}";

    public void UpdateCoins(int coins)
    {
        coinsText.text = string.Format(COINS_FORMAT, coins);
    }
}
