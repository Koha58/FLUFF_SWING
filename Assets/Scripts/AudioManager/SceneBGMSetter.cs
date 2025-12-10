using UnityEngine;

/// <summary>
/// シーンロード時にAudioManagerにBGMの再生を指示するコンポーネント
/// </summary>
public class SceneBGMSetter : MonoBehaviour
{
    [SerializeField] private AudioClip sceneBGM; // このシーンで流したいBGMクリップ

    // シーンのロードが完了したタイミングでBGMを再生
    private void Start()
    {
        if (AudioManager.Instance != null)
        {
            // AudioManagerの永続化インスタンスにBGMを再生させる
            AudioManager.Instance.PlayBGM(sceneBGM);
        }
    }
}
