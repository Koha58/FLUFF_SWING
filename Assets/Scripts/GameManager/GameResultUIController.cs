using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲームのリザルトUI（クリア／ゲームオーバー）を制御するクラス。
/// </summary>
public enum GameResult { Clear, GameOver }

public class GameResultUIController : MonoBehaviour
{
    #region Singleton

    /// <summary>シングルトンインスタンス</summary>
    public static GameResultUIController Instance { get; private set; }

    #endregion

    #region Inspector Fields

    [Header("共通 UI 設定")]
    [SerializeField]
    private GraphicRaycaster raycaster; // UIボタンの有効化制御用Raycaster

    [Header("結果別 UI 設定")]
    [SerializeField]
    private GameObject clearUI; // ステージクリア時に表示するUI
    [SerializeField]
    private GameObject gameOverUI; // ゲームオーバー時に表示するUI

    [Header("アニメーション設定")]
    [SerializeField]
    private float targetScale = 0.4f; // 拡大後のスケール
    [SerializeField]
    private float scaleUpDuration = 0.5f; // アニメーションにかける時間

    [Header("イージング設定")]
    [SerializeField]
    private float easeOutBackStrength = 1.70158f; // EaseOutBack の強度

    #endregion

    #region Constants

    /// <summary>セレクトシーンの名前（SceneManagerで使用）</summary>
    private const string SelectSceneName = "SelectScene";
    /// <summary>タイトルシーンの名前（SceneManagerで使用）</summary>
    private const string TitleSceneName = "TitleScene";

    /// <summary>通常のゲーム進行速度（Time.timeScale = 1）</summary>
    private const float NormalTimeScale = 1.0f;

    /// <summary>イージング終了時の補正値</summary>
    private const float EasingEnd = 1f;

    /// <summary>イージング入力オフセット</summary>
    private const float EasingOffset = -1f;

    /// <summary>3次の累乗指数</summary>
    private const int CubicPower = 3;

    /// <summary>2次の累乗指数</summary>
    private const int QuadraticPower = 2;

    #endregion

    #region Unity Events

    /// <summary>
    /// 初期化処理。UI非表示化とシングルトン登録を行う。
    /// </summary>
    private void Awake()
    {
        // シングルトン登録
        Instance = this;

        // ゲーム開始時点では UI を非表示にし、Raycaster も無効にする
        raycaster.enabled = false;
        clearUI.SetActive(false);
        gameOverUI.SetActive(false);
    }

    #endregion

    #region Public API

    /// <summary>
    /// 指定された結果に応じてリザルトUIを表示する。
    /// </summary>
    /// <param name="result">ゲーム結果（Clear or GameOver）</param>
    public void ShowResult(GameResult result)
    {
        // 表示対象UIを選定
        GameObject targetUI = (result == GameResult.Clear) ? clearUI : gameOverUI;

        // 非表示側UIを明示的に隠す
        clearUI.SetActive(false);
        gameOverUI.SetActive(false);

        // UIをスケールゼロで表示してアニメーション準備
        targetUI.SetActive(true);
        targetUI.transform.localScale = Vector3.zero;

        // 拡大アニメーション開始
        StartCoroutine(AnimateScaleUp(targetUI));

        // ボタン操作を許可
        raycaster.enabled = true;
    }

    #endregion

    #region Button Handlers

    /// <summary>
    /// 「リトライ」ボタン押下時の処理。現在のシーンを再読み込みする。
    /// </summary>
    public void ClickRetry()
    {
        ResumeGameTime();
        TransitionManager.Instance.PlayTransitionAndLoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// 「タイトルへ戻る」ボタン押下時の処理。
    /// </summary>
    public void ClickQuitToTitle()
    {
        ResumeGameTime();
        TransitionManager.Instance.PlayTransitionAndLoadScene(TitleSceneName);
    }

    /// <summary>
    /// 「次のステージへ」ボタン押下時の処理（※現在は同一シーンを再ロード）。
    /// </summary>
    public void ClickNext()
    {
        ResumeGameTime();
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int totalScenes = SceneManager.sceneCountInBuildSettings;
        int nextIndex = (currentIndex + 1 < totalScenes) ? currentIndex + 1 : 0; // 最後ならタイトルに戻る
        TransitionManager.Instance.PlayTransitionAndLoadScene(SceneUtility.GetScenePathByBuildIndex(nextIndex));
    }

    /// <summary>
    /// 「ステージ選択へ」ボタン押下時の処理（※現在はタイトル画面に遷移）。
    /// </summary>
    public void ClickQuitToStageSelect()
    {
        ResumeGameTime();
        SceneManager.LoadScene(SelectSceneName);
    }

    #endregion

    #region Helpers

    /// <summary>
    /// ゲームの時間スケールを通常状態に戻す（ポーズ解除用）。
    /// </summary>
    private void ResumeGameTime()
    {
        Time.timeScale = NormalTimeScale;
    }

    /// <summary>
    /// 指定されたUIを滑らかに拡大して表示する（ポップアップ風）。
    /// </summary>
    /// <param name="target">拡大対象のGameObject</param>
    private System.Collections.IEnumerator AnimateScaleUp(GameObject target)
    {
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one * targetScale;
        float elapsed = 0f;

        // 指定された時間で拡大処理を行う
        while (elapsed < scaleUpDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            // 時間の進行度（0～1）を算出
            float t = Mathf.Clamp01(elapsed / scaleUpDuration);

            // イージング関数で自然なスケール拡大を適用
            float easedT = EaseOutBack(t);

            // 現在のスケールを更新
            target.transform.localScale = Vector3.Lerp(startScale, endScale, easedT);
            yield return null;
        }

        // 最終スケールを明示的に設定
        target.transform.localScale = endScale;
    }

    #endregion

    #region Easing

    /// <summary>
    /// OutBack風のイージング関数（アニメーション終端でバウンドするような効果）。
    /// </summary>
    /// <param name="t">0〜1 の時間進行度（0:開始, 1:終了）</param>
    /// <returns>イージング後の補間値（0〜1以上）</returns>
    private float EaseOutBack(float t)
    {
        // バウンドの強さ（Inspectorから設定可能）
        float c1 = easeOutBackStrength;

        // バウンド効果の係数（通常 c1 + 1）
        float c3 = c1 + EasingEnd;

        // 時間の進行を調整（t を -1 ～ 0 にシフト）
        float p = t + EasingOffset;

        // EaseOutBack の式に基づいて補間値を返す
        // return = 1 + (c3 * p³) + (c1 * p²)
        // 終盤でやや行き過ぎてから戻るような「バウンド感」を生む
        return EasingEnd + c3 * Mathf.Pow(p, CubicPower) + c1 * Mathf.Pow(p, QuadraticPower);
    }


    #endregion
}
