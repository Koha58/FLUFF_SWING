using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲームのリザルトUI（クリア／ゲームオーバー）を制御するクラス。
/// - 結果UIの表示（Clear / GameOver）
/// ポップアップ風の拡大アニメーション
/// - ボタン押下時のシーン遷移
/// - 連打によるSE多重再生・遷移多重実行の防止（Raycasterを止める）
/// </summary>
public enum GameResult { Clear, GameOver }

public class GameResultUIController : MonoBehaviour
{
    #region Singleton

    /// <summary>
    /// シングルトン参照（外部から GameResultUIController.Instance でアクセスできる）
    /// </summary>
    public static GameResultUIController Instance { get; private set; }

    #endregion

    #region Inspector Fields

    [Header("共通 UI 設定")]
    [SerializeField] private GraphicRaycaster raycaster;
    // ↑ クリック入力の遮断/許可に使う（連打防止で OFF にするのがポイント）

    [Header("結果別 UI 設定")]
    [SerializeField] private GameObject clearUI;     // Clear時に表示するUIルート
    [SerializeField] private GameObject gameOverUI;  // GameOver時に表示するUIルート

    [Header("アニメーション設定")]
    [SerializeField] private float targetScale = 0.4f;     // 最終的に到達させるスケール
    [SerializeField] private float scaleUpDuration = 0.5f; // スケールアップにかける時間（秒）

    [Header("イージング設定")]
    [SerializeField] private float easeOutBackStrength = 1.70158f; // バウンド感の強さ

    [Header("リザルトボタンSE")]
    [Tooltip("戻る/リトライ/ステージ選択などの決定SE")]
    [SerializeField] private AudioClip backClickSE;

    [Tooltip("次のステージ用の決定SE")]
    [SerializeField] private AudioClip nextClickSE;

    #endregion

    #region Constants

    /// <summary>ステージ選択シーン名</summary>
    private const string SelectSceneName = "SelectScene";

    /// <summary>タイトルシーン名</summary>
    private const string TitleSceneName = "TitleScene";

    /// <summary>ゲーム通常速度</summary>
    private const float NormalTimeScale = 1.0f;

    // EaseOutBack 計算用定数
    private const float EasingEnd = 1f;
    private const float EasingOffset = -1f;
    private const int CubicPower = 3;
    private const int QuadraticPower = 2;

    #endregion

    #region Runtime State (連打対策)

    /// <summary>
    /// 「もうリザルト画面から離脱処理を開始したか？」
    /// true になった瞬間から、以降のボタン押下はすべて無視する。
    /// 目的：
    /// - SE多重再生防止
    /// - シーン遷移多重実行防止
    /// </summary>
    private bool _isLeavingResult = false;

    #endregion

    #region Unity Events

    private void Awake()
    {
        // シングルトン登録
        // ※厳密にするなら「既にInstanceがある場合はDestroy」等も可能だが、今回は簡潔に
        Instance = this;

        // 起動時は結果UIを閉じておく（誤表示防止）
        if (clearUI != null) clearUI.SetActive(false);
        if (gameOverUI != null) gameOverUI.SetActive(false);

        // 起動時は入力を受け付けない（結果表示時にONにする）
        if (raycaster != null) raycaster.enabled = false;

        // 連打対策フラグ初期化（念のため）
        _isLeavingResult = false;
    }

    #endregion

    #region Public API

    /// <summary>
    /// 指定された結果に応じてリザルトUIを表示する。
    /// </summary>
    public void ShowResult(GameResult result)
    {
        // 「結果画面に入った」ので、ボタンの押下状態をリセット
        _isLeavingResult = false;

        // 表示するUIルートを決める
        GameObject targetUI = (result == GameResult.Clear) ? clearUI : gameOverUI;

        // 念のため両方いったん非表示（同時表示を防ぐ）
        if (clearUI != null) clearUI.SetActive(false);
        if (gameOverUI != null) gameOverUI.SetActive(false);

        // 表示対象があるなら表示してアニメーション準備
        if (targetUI != null)
        {
            // 表示
            targetUI.SetActive(true);

            // 0から拡大するために初期スケールをゼロに
            targetUI.transform.localScale = Vector3.zero;

            // ポップアップ風スケールアップ演出
            StartCoroutine(AnimateScaleUp(targetUI));
        }

        // 結果画面のボタン操作を許可
        if (raycaster != null) raycaster.enabled = true;
    }

    #endregion

    #region Button Handlers

    /// <summary>
    /// 「リトライ」ボタン：現在のシーンを再読み込み
    /// </summary>
    public void ClickRetry()
    {
        // 連打対策：初回押下のみ通す（SEも1回だけ）
        // ★ここは nextClickSE を鳴らす運用にしている
        if (!BeginExitFromResult(nextClickSE)) return;

        // TimeScale停止している可能性があるので、遷移前に戻す
        ResumeGameTime();

        // 現在のシーン名を取得してリロード
        string currentSceneName = SceneManager.GetActiveScene().name;

        // トランジション付きで遷移
        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.TryPlayTransitionAndLoadScene(currentSceneName);
        }
        else
        {
            // フォールバック：TransitionManagerが無ければ通常ロード
            Debug.LogError("TransitionManagerが見つかりません。直接シーンロードします。");
            SceneManager.LoadScene(currentSceneName);
        }
    }

    /// <summary>
    /// 「タイトルへ」ボタン：タイトルシーンへ戻る
    /// </summary>
    public void ClickQuitToTitle()
    {
        // 連打対策：初回のみ
        if (!BeginExitFromResult(backClickSE)) return;

        ResumeGameTime();

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.TryPlayTransitionAndLoadScene(TitleSceneName);
        }
        else
        {
            Debug.LogError("TransitionManagerが見つかりません。直接シーンロードします。");
            SceneManager.LoadScene(TitleSceneName);
        }
    }

    /// <summary>
    /// 「次へ」ボタン：次のステージに進む（最後なら0へ）
    /// </summary>
    public void ClickNext()
    {
        // 連打対策：初回のみ（next用SEを鳴らす）
        if (!BeginExitFromResult(nextClickSE)) return;

        ResumeGameTime();

        // 現在のビルドIndexを取得
        int currentIndex = SceneManager.GetActiveScene().buildIndex;

        // BuildSettingsに登録されているシーン数
        int totalScenes = SceneManager.sceneCountInBuildSettings;

        // 次のIndex（最後なら0に戻す）
        int nextIndex = (currentIndex + 1 < totalScenes) ? currentIndex + 1 : 0;

        // buildIndex→シーンパスを取得（あなたのTransitionManagerが受け取れる形式ならOK）
        // ※一般的には「シーン名」で遷移する方が安全。ここは運用に合わせて調整
        string nextScenePath = SceneUtility.GetScenePathByBuildIndex(nextIndex);

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.TryPlayTransitionAndLoadScene(nextScenePath);
        }
        else
        {
            Debug.LogError("TransitionManagerが見つかりません。直接シーンロードします。");
            SceneManager.LoadScene(nextIndex);
        }
    }

    /// <summary>
    /// 「ステージ選択へ」ボタン：SelectSceneへ戻る
    /// </summary>
    public void ClickQuitToStageSelect()
    {
        // 連打対策：初回のみ
        if (!BeginExitFromResult(backClickSE)) return;

        ResumeGameTime();

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.TryPlayTransitionAndLoadScene(SelectSceneName);
        }
        else
        {
            Debug.LogError("TransitionManagerが見つかりません。直接シーンロードします。");
            SceneManager.LoadScene(SelectSceneName);
        }
    }

    #endregion

    #region Exit Guard + SE

    /// <summary>
    /// リザルト画面から離脱する処理の共通入口。
    /// - 連打による多重実行を防ぐ（最初の1回だけ通す）
    /// - RaycasterをOFFにして以降のクリック入力自体を遮断
    /// - 指定SEを一度だけ鳴らす
    /// </summary>
    /// <param name="se">このボタンで鳴らしたいSE</param>
    /// <returns>true: 初回なので処理続行 / false: 2回目以降なので無視</returns>
    private bool BeginExitFromResult(AudioClip se)
    {
        // すでにどれかのボタンが押されていたら何もしない
        // （ここで止めることで、SE多重・遷移多重が起こらない）
        if (_isLeavingResult) return false;

        // 初回押下として確定
        _isLeavingResult = true;

        // UI入力を即停止（以降のクリックを物理的に遮断）
        if (raycaster != null) raycaster.enabled = false;

        // 指定SEを1回だけ鳴らす（nullなら鳴らさない）
        PlayClickSE(se);

        return true;
    }

    /// <summary>
    /// SE再生処理（AudioManagerがあるなら統一音量で再生）
    /// </summary>
    private void PlayClickSE(AudioClip se)
    {
        // 未設定なら何もしない
        if (se == null) return;

        // AudioManagerがあればそちらで再生（音量統一・Mixer管理など）
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(se);
        }
        else
        {
            // フォールバック：AudioManagerが無い場合の簡易再生
            // ※プロジェクト次第では削除してOK
            AudioSource.PlayClipAtPoint(se, Vector3.zero);
        }
    }

    #endregion

    #region Helpers

    /// <summary>
    /// ポーズ等でTimeScaleが止まっている場合に備えて、通常速度に戻す
    /// </summary>
    private void ResumeGameTime()
    {
        Time.timeScale = NormalTimeScale;
    }

    /// <summary>
    /// ポップアップ風にスケールアップして表示する演出
    /// ※TimeScale停止中でも動くように unscaledDeltaTime を使用
    /// </summary>
    private System.Collections.IEnumerator AnimateScaleUp(GameObject target)
    {
        // 0 → 目標スケールに向けて補間
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one * targetScale;

        float elapsed = 0f;

        // 規定時間になるまで拡大し続ける
        while (elapsed < scaleUpDuration)
        {
            // TimeScaleに影響されない時間経過
            elapsed += Time.unscaledDeltaTime;

            // 進行度（0〜1）
            float t = Mathf.Clamp01(elapsed / scaleUpDuration);

            // バウンド感のある補間値に変換
            float easedT = EaseOutBack(t);

            // スケール更新
            target.transform.localScale = Vector3.Lerp(startScale, endScale, easedT);

            yield return null;
        }

        // 最終値を明示（誤差吸収）
        target.transform.localScale = endScale;
    }

    /// <summary>
    /// EaseOutBack：終盤に少し行き過ぎて戻る「バウンド感」を作る
    /// </summary>
    private float EaseOutBack(float t)
    {
        // バウンドの強さ
        float c1 = easeOutBackStrength;

        // 一般的に c1 + 1 を使う
        float c3 = c1 + EasingEnd;

        // tを -1〜0 にずらして式に入れる
        float p = t + EasingOffset;

        // 1 + c3*p^3 + c1*p^2
        return EasingEnd + c3 * Mathf.Pow(p, CubicPower) + c1 * Mathf.Pow(p, QuadraticPower);
    }

    #endregion
}
