using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// プレイヤーのライフ（ハート）UIを制御するクラス。
/// - ライフ数に応じてハートアイコンを生成・配置
/// - 現在HPに応じてハートの表示/非表示を切り替える
/// </summary>
public class PlayerHealthUI : MonoBehaviour
{
    /// <summary>
    /// ハートのプレハブ（ImageなどのUI要素）
    /// </summary>
    [SerializeField] private GameObject heartPrefab;

    /// <summary>
    /// ハートを並べるPanel（HorizontalLayoutGroup などを使わない場合）
    /// </summary>
    [SerializeField] private RectTransform heartPanel;

    /// <summary>
    /// ハート同士の横の間隔（X軸）
    /// </summary>
    private float heartSpacing = 70f;

    /// <summary>
    /// 最初のハートのX座標オフセット（パネルの中心からのずれ）
    /// </summary>
    private float startOffsetX = 100f;

    /// <summary>
    /// ハートのY座標（位置を固定したい場合に使用）
    /// </summary>
    private float startOffsetY = -65f;

    /// <summary>
    /// 現在表示中のハートアイコンのリスト
    /// </summary>
    private List<GameObject> heartIcons = new List<GameObject>();

    /// <summary>
    /// 最大HPに応じてハートアイコンを初期化する。
    /// すでにあるアイコンは削除し、指定数ぶん生成・配置する。
    /// </summary>
    /// <param name="maxHP">プレイヤーの最大HP</param>
    public void SetMaxHealth(int maxHP)
    {
        // 古いハートUIをすべて削除
        foreach (var icon in heartIcons)
        {
            Destroy(icon);
        }
        heartIcons.Clear();

        // 新しくハートUIを生成して横に並べる
        for (int i = 0; i < maxHP; i++)
        {
            GameObject heart = Instantiate(heartPrefab, heartPanel);
            RectTransform rt = heart.GetComponent<RectTransform>();

            // ハートの位置を設定（Xは間隔分ずらす、Yは固定）
            rt.anchoredPosition = new Vector2(startOffsetX + i * heartSpacing, startOffsetY);

            // リストに追加して管理
            heartIcons.Add(heart);
        }
    }

    /// <summary>
    /// 現在のHPに応じてハートの表示/非表示を更新する。
    /// 残りHP以上のハートは非表示にする。
    /// </summary>
    /// <param name="currentHP">現在のプレイヤーHP</param>
    public void UpdateHealth(int currentHP)
    {
        for (int i = 0; i < heartIcons.Count; i++)
        {
            // 現在HP以下なら表示、それ以降は非表示に
            heartIcons[i].SetActive(i < currentHP);
        }
    }
}
