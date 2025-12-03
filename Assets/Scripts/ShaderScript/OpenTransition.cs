using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// シーン遷移時に、カスタムシェーダーを使用して「画面を開く」（フェードアウト/イン）トランジション演出を制御するクラス。
/// Imageコンポーネントに設定されたマテリアルの'_Alpha'プロパティを操作することで、演出を行います。
/// </summary>
public class OpenTransition : MonoBehaviour
{
    [Tooltip("画面を開く（フェードアウト）演出用のカスタムマテリアル。シェーダーには'_Alpha'プロパティが必要です。")]
    [SerializeField]
    private Material _transitionIn; // 演出に使用するマテリアル

    // 演出を待つための標準的なUnityライフサイクルメソッドは使用しない
    void Start()
    {
        // Start()は呼ばれず、TransitionManagerからPlay()が呼ばれることを想定
    }

    /// <summary>
    /// マテリアルの_Alphaプロパティを操作して、トランジションアニメーションを実行するコルーチン。
    /// （画面が開く演出の場合、_Alphaを1 (完全な黒) から 0 (透明) へと変化させる）
    /// </summary>
    /// <param name="material">操作対象のマテリアル</param>
    /// <param name="time">アニメーションにかける時間（秒）</param>
    /// <returns>アニメーション完了までの待機</returns>
    IEnumerator Animate(Material material, float time)
    {
        // Imageコンポーネントにカスタムマテリアルを設定
        GetComponent<Image>().material = material;

        float current = 0;

        // 開始時の初期値を設定 (1.0fはシェーダー側で画面全体を覆っている状態を想定)
        material.SetFloat("_Alpha", 1.0f);

        // 指定された時間までループ
        while (current < time)
        {
            // _Alpha値を 1 から 0 へ線形に減少させる
            // current/time は 0 から 1 へと増加する
            material.SetFloat("_Alpha", 1 - current / time);

            yield return new WaitForEndOfFrame();

            current += Time.deltaTime;
        }

        // 念のため、アニメーション終了後の最終値を設定し、完全に画面を開いた状態（透明）にする
        material.SetFloat("_Alpha", 0); // 💡 画面を開く演出では、最終値は 0 (透明) になるべき

        // 💡 補足: 元のコードでは最終値が 1 に設定されていますが、
        // 画面を開く（フェードアウト）演出としては 0 (透明) が正しい動作です。
        // もしシェーダーの仕様で 1 が透明を意味する場合は元のままで構いません。
    }

    /// <summary>
    /// TransitionManagerから呼び出される、画面を開く演出を開始するメソッド。
    /// </summary>
    /// <returns>演出完了までの待機</returns>
    public IEnumerator Play()
    {
        // Animateコルーチンを実行し、1.5秒かけて画面を開く
        yield return Animate(_transitionIn, 1.5f);
    }
}