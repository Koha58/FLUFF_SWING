using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// プレイヤーの所持コイン数をUIに表示するクラス（シングルトン）
/// </summary>
public class PlayerCoinUI : MonoBehaviour
{
    /// <summary>
    /// シングルトンインスタンス（他スクリプトからアクセス可能）。
    /// </summary>
    public static PlayerCoinUI Instance { get; private set; }

    /// <summary>
    /// コイン数を表示するTextMeshProUGUI。
    /// </summary>
    [SerializeField] private TextMeshProUGUI coinText;

    /// <summary>
    /// 現在のコイン数。
    /// </summary>
    private int coinCount = 0;

    /// <summary>
    /// インスタンス初期化（シングルトン設定とUI更新）。
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        UpdateUI();
    }

    /// <summary>
    /// 指定したコイン数を加算してUIを更新。
    /// </summary>
    /// <param name="amount">加算するコイン数</param>
    public void AddCoin(int amount)
    {
        coinCount += amount;
        UpdateUI();
    }

    /// <summary>
    /// UIテキストを現在のコイン数で更新（2桁表示、ゼロ埋め）。
    /// </summary>
    private void UpdateUI()
    {
        if (coinText != null)
            coinText.text = coinCount.ToString("D2"); // 例：01, 09, 10, 25 など
    }
}
