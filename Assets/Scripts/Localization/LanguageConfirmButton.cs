using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 言語選択確定ボタン用クラス
/// 初回起動時の言語選択完了処理と、SE再生後のタイトル遷移を担当する
/// </summary>
public class LanguageConfirmButton : MonoBehaviour
{
    // 遷移先のタイトルシーン名
    [SerializeField] private string titleSceneName = "TitleScene";

    // 決定時に鳴らすSE
    [SerializeField] private AudioClip confirmSE;

    // ボタン連打防止用フラグ
    private bool _clicked = false;

    /// <summary>
    /// UIボタンから呼ばれる確定処理
    /// </summary>
    public void OnConfirm()
    {
        // すでに押されていたら無視（連打防止）
        if (_clicked) return;
        _clicked = true;

        // 「初回言語選択が完了した」フラグを立てる
        // 次回起動時以降は言語選択画面をスキップするための情報
        LocalizationManager.Instance.MarkLanguageSelectedOnce();

        // SEを再生してからタイトルへ遷移する
        StartCoroutine(PlaySeAndGoTitle());
    }

    /// <summary>
    /// 決定SEを再生し、再生完了後にタイトルシーンへ遷移するコルーチン
    /// </summary>
    private IEnumerator PlaySeAndGoTitle()
    {
        float waitTime = 0f;

        // 決定SEが設定されている場合
        if (confirmSE != null)
        {
            // AudioManager が存在する場合はそちら経由で再生
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySE(confirmSE);
            }
            // AudioManager が存在しない場合のフォールバック
            else
            {
                AudioSource.PlayClipAtPoint(confirmSE, Vector3.zero);
            }

            // SEの再生時間分待機する
            waitTime = confirmSE.length;
        }

        // SEがある場合はその長さ分待つ
        if (waitTime > 0f)
        {
            yield return new WaitForSeconds(waitTime);
        }
        // SEがない場合でも、最低1フレームは待機して安定させる
        else
        {
            yield return null;
        }

        // 画面遷移演出付きでシーンロードできる場合
        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.TryPlayTransitionAndLoadScene(titleSceneName);
        }
        // 遷移マネージャーがない場合は即ロード
        else
        {
            SceneManager.LoadScene(titleSceneName);
        }
    }
}
