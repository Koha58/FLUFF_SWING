using UnityEngine;

/// <summary>
/// SpawnDataSO に基づいて、プレハブをシーンにスポーンするマネージャー。
/// - スポーン情報は ScriptableObject（spawnData）から読み込む。
/// - Resources フォルダからプレハブをロードして配置。
/// - オプションで spawnParent に子として配置可能。
/// </summary>
public class SpawnManager : MonoBehaviour
{
    /// <summary>
    /// スポーンデータ（ScriptableObject）。
    /// CSVなどから事前に生成されたデータを読み込む。
    /// </summary>
    public SpawnDataSO spawnData;

    /// <summary>
    /// 生成したオブジェクトの親となる Transform。
    /// null の場合はルートに生成される。
    /// </summary>
    public Transform spawnParent;

    /// <summary>
    /// ゲーム開始時にスポーン処理を実行。
    /// </summary>
    void Start()
    {
        foreach (var entry in spawnData.entries)
        {
            // Resources フォルダからプレハブをロード
            GameObject prefab = Resources.Load<GameObject>(entry.prefabName);
            if (prefab == null)
            {
                Debug.LogWarning($"Prefabが見つかりません: {entry.prefabName}");
                continue;
            }

            // プレハブを指定された位置に生成
            GameObject obj = Instantiate(prefab, entry.position, Quaternion.identity, spawnParent);

            // TODO: 必要に応じて type に応じた初期化処理を追加可能
            // 例: if (entry.type == "Enemy") { ... }
        }
    }
}
