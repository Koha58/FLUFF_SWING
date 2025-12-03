using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// シーン遷移時に、カスタムシェーダーを使用して「画面を閉じる」（フェードイン/アウト）トランジション演出を制御するクラス。
/// Imageコンポーネントに設定されたマテリアルの'_Alpha'プロパティを操作することで、演出を行います。
/// </summary>
public class CloseTransition : MonoBehaviour
{
    [Tooltip("画面を閉じる（フェードイン）演出用のカスタムマテリアル。シェーダーには'_Alpha'プロパティが必要です。")]
    [SerializeField]
    private Material _transitionIn; // 演出に使用するマテリアル

    // 演出を待つための標準的なUnityライフサイクルメソッドは使用しない
    void Start()
    {
        // Start()は呼ばれず、TransitionManagerからPlay()が呼ばれることを想定
    }

    /// <summary>
    /// マテリアルの_Alphaプロパティを操作して、トランジションアニメーションを実行するコルーチン。
    /// （画面を閉じる演出の場合、_Alphaを0 (透明) から 1 (完全な黒) へと変化させる）
    /// </summary>
    /// <param name="material">操作対象のマテリアル</param>
    /// <param name="time">アニメーションにかける時間（秒）</param>
    /// <returns>アニメーション完了までの待機</returns>
    IEnumerator Animate(Material material, float time)
    {
        // Imageコンポーネントにカスタムマテリアルを設定
        GetComponent<Image>().material = material;

        float current = 0;

        // 開始時の初期値を設定 (0はシェーダー側で透明な状態を想定)
        material.SetFloat("_Alpha", 0);

        // 指定された時間までループ
        while (current < time)
        {
            // _Alpha値を 0 から 1 へ線形に増加させる (画面が徐々に閉じる)
            // current/time は 0 から 1 へと増加する
            material.SetFloat("_Alpha", current / time);

            yield return new WaitForEndOfFrame();

            current += Time.deltaTime;
        }

        // アニメーション終了後の最終値として、完全に画面を閉じた状態（不透明）にする
        material.SetFloat("_Alpha", 1);
    }

    /// <summary>
    /// TransitionManagerから呼び出される、画面を閉じる演出を開始するメソッド。
    /// </summary>
    /// <returns>演出完了までの待機</returns>
    public IEnumerator Play()
    {
        // Animateコルーチンを実行し、1.5秒かけて画面を閉じる
        yield return Animate(_transitionIn, 1.5f);
    }
}