using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 初回言語設定画面で言語設定済みか確認し、設定済みの場合選択画面をスキップさせるクラス
/// </summary>
public class BootSceneController : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string languageSceneName = "StartupScene"; // 初回言語選択シーンの名前
    [SerializeField] private string mainSceneName = "TitleScene";       // 2回目以降に遷移するメインシーンの名前

    [Header("Debug")]
    [Tooltip("ONにすると次回起動時に必ず初回判定になる（Editor / DevBuild限定）")]
    [SerializeField] private bool forceFirstLaunch = false; // 開発用フラグ：ONにすると初回フローを強制表示

    private void Start()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // 開発用：強制初回判定フロー
        if (forceFirstLaunch)
        {
            Debug.Log("Debug: Force First Launch ON");

            // PlayerPrefs から言語選択済みフラグと選択言語を削除
            // これにより HasSelectedLanguageOnce が false になる
            PlayerPrefs.DeleteKey("LANG_SELECTED_ONCE");
            PlayerPrefs.DeleteKey("LANG");
            PlayerPrefs.Save();

            // LocalizationManager に初回判定を再評価させる
            // Awake で既に初期化済みの場合、この呼び出しで CurrentLanguage を再設定
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.ResetFirstLaunch();
            }
        }
#endif

        // シーン遷移処理
        // 初回選択済みかどうかで遷移先を分ける
        if (LocalizationManager.Instance.HasSelectedLanguageOnce)
        {
            // 2回目以降 → メインシーンへ
            SceneManager.LoadScene(mainSceneName);
        }
        else
        {
            // 初回 → 言語選択シーンへ
            SceneManager.LoadScene(languageSceneName);
        }
    }
}
