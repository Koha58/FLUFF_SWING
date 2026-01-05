using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LanguageConfirmButton : MonoBehaviour
{
    [SerializeField] private string titleSceneName = "TitleScene";
    [SerializeField] private AudioClip confirmSE;

    private bool _clicked = false; // 連打防止

    public void OnConfirm()
    {
        if (_clicked) return;
        _clicked = true;

        // 初回言語選択完了フラグを立てる
        LocalizationManager.Instance.MarkLanguageSelectedOnce();

        // SEを鳴らしてから遷移
        StartCoroutine(PlaySeAndGoTitle());
    }

    private IEnumerator PlaySeAndGoTitle()
    {
        float waitTime = 0f;

        // 決定SE
        if (confirmSE != null)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySE(confirmSE);
            }
            else
            {
                AudioSource.PlayClipAtPoint(confirmSE, Vector3.zero);
            }

            // SEの長さ分待つ
            waitTime = confirmSE.length;
        }

        // 念のため最低1フレームは待つ（SEなし時でも安定）
        if (waitTime > 0f)
            yield return new WaitForSeconds(waitTime);
        else
            yield return null;

        // タイトルへ遷移
        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.TryPlayTransitionAndLoadScene(titleSceneName);
        }
        else
        {
            SceneManager.LoadScene(titleSceneName);
        }
    }
}
