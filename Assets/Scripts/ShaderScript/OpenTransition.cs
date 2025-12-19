using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// シーン遷移後に「画面を開く」トランジションを再生するクラス。
/// カスタムシェーダーの _Threshold を 0→1 に動かし、
/// ドットが消えていく（下の画面が見える）演出を行う。
/// </summary>
public class OpenTransition : MonoBehaviour
{
    [Header("Transition Material (Source)")]
    [Tooltip("トランジション用シェーダーを設定した元マテリアル（実行時に複製して使用）")]
    [SerializeField] private Material _transitionMatSource;

    [Header("Animation Settings")]
    [Tooltip("トランジションにかける時間（秒）")]
    [SerializeField] private float _duration = 1.5f;

    // Imageコンポーネント参照
    private Image _img;

    // 実行時に生成するマテリアル（sharedMaterialを書き換えないため）
    private Material _mat;

    // シェーダープロパティID（高速＆安全）
    private static readonly int AlphaId = Shader.PropertyToID("_Alpha");
    private static readonly int ThresholdId = Shader.PropertyToID("_Threshold");
    private static readonly int ScreenWId = Shader.PropertyToID("_ScreenW");
    private static readonly int ScreenHId = Shader.PropertyToID("_ScreenH");

    /// <summary>
    /// 初期化処理。
    /// 生成直後に一瞬表示される「白フラッシュ」を防ぐため、
    /// Image自体を最初は非表示にしておく。
    /// </summary>
    private void Awake()
    {
        _img = GetComponent<Image>();

        // UI操作をブロックしないようにする
        _img.raycastTarget = false;

        // ★重要：生成直後は描画しない（白一色の瞬間表示を防ぐ）
        _img.enabled = false;
    }

    /// <summary>
    /// TransitionManager から呼ばれるエントリーポイント。
    /// 画面を「開く」トランジションを再生する。
    /// </summary>
    public IEnumerator Play()
    {
        // ---- 描画開始 ----
        _img.enabled = true;

        // 重要：元マテリアルを直接使わず、必ず複製する
        // （UIでsharedMaterialを書き換える事故を防ぐ）
        _mat = new Material(_transitionMatSource);
        _img.material = _mat;

        // 画面解像度をシェーダーに渡す
        // （ドットサイズ・低解像度演出のズレ防止）
        _mat.SetFloat(ScreenWId, Screen.width);
        _mat.SetFloat(ScreenHId, Screen.height);

        // ---- 初期状態の明示 ----
        // ・Alpha = 1 → トランジション自体は表示する
        // ・Threshold = 0 → 完全に覆われた状態（まだ開いていない）
        _mat.SetFloat(AlphaId, 1f);
        _mat.SetFloat(ThresholdId, 0f);

        // UI反映を1フレーム待つ（白フラッシュ防止に非常に重要）
        yield return null;

        // ---- Open演出 ----
        // Threshold を 0 → 1 に動かすことで、
        // 左から右へドットが消えていき、下の画面が見える
        float t = 0f;
        while (t < _duration)
        {
            float progress = t / _duration; // 0..1
            _mat.SetFloat(ThresholdId, progress);

            yield return null;
            t += Time.deltaTime;
        }

        // ---- 念のため最終値を明示 ----
        _mat.SetFloat(ThresholdId, 1f); // 完全に開いた状態
        _mat.SetFloat(AlphaId, 0f);     // トランジション自体を透明に

        // ---- 後始末 ----
        // Imageを非表示にして、以降の描画・入力干渉を防ぐ
        _img.enabled = false;
    }
}
