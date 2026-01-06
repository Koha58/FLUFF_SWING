using UnityEngine;

/// <summary>
/// シーン起動時に必要な常駐システムを保証するためのブートストラップ用クラス
/// 主に「Titleシーン単体再生」など、通常の起動経路を通らない場合の保険として使う
/// </summary>
public class Bootstrapper : MonoBehaviour
{
    // Resources フォルダ以下に配置されている LocalizationManager の Prefab パス
    // 例: Assets/Resources/Systems/LocalizationManager.prefab
    [Tooltip("Resources/Systems/LocalizationManager をロードして生成する")]
    [SerializeField] private string localizationPrefabPath = "Systems/LocalizationManager";


    private void Awake()
    {
        // すでに LocalizationManager が存在している場合は何もしない
        // 通常起動では Startup シーン側ですでに生成されている想定
        if (LocalizationManager.Instance != null)
        {
            return;
        }

        // Title シーン単体再生など、
        // Startup を経由しない起動パターンのときだけここで生成する

        // Resources フォルダから LocalizationManager の Prefab をロード
        var prefab = Resources.Load<GameObject>(localizationPrefabPath);

        // Prefab が見つからなかった場合はエラーを出して処理を中断
        if (prefab == null)
        {
            Debug.LogError(
                $"[Bootstrapper] Prefab not found: Resources/{localizationPrefabPath}.prefab"
            );
            return;
        }

        // LocalizationManager をシーン上に生成
        // （通常は DontDestroyOnLoad される想定）
        Instantiate(prefab);
    }
}
