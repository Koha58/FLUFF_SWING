using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
    [Tooltip("Resources/Systems/LocalizationManager をロードして生成する")]
    [SerializeField] private string localizationPrefabPath = "Systems/LocalizationManager";

    private void Awake()
    {
        // すでにあれば何もしない（通常起動ではStartup側で生成済み）
        if (LocalizationManager.Instance != null) return;

        // Title単体再生などのときだけ生成
        var prefab = Resources.Load<GameObject>(localizationPrefabPath);
        if (prefab == null)
        {
            Debug.LogError($"[Bootstrapper] Prefab not found: Resources/{localizationPrefabPath}.prefab");
            return;
        }

        Instantiate(prefab);
    }
}
