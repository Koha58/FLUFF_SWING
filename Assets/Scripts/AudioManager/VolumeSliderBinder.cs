using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 🎚 VolumeSliderBinder
/// 
/// 各スライダー（BGM / SE）を AudioManager に自動で紐付ける補助クラス。
/// 
/// 【目的】
/// ・AudioManagerのBindBGMSlider / BindSESliderを自動で呼び出す  
/// ・シーンごとにスライダーが生成されても、自動で再リンクされるようにする  
/// 
/// 【使い方】
/// ・BGM用スライダーにこのスクリプトをアタッチ → type を「BGM」に設定  
/// ・SE用スライダーにこのスクリプトをアタッチ → type を「SE」に設定  
/// 
/// AudioManagerはシーンを跨いで保持されるので、各スライダーが生成されるたびに
/// 自動的にBindメソッドを呼び出して接続します。
/// </summary>
public class VolumeSliderBinder : MonoBehaviour
{
    //====================================================================
    // 🔧 設定項目
    //====================================================================
    public enum VolumeType { BGM, SE }   // スライダーが制御する音量タイプ
    [SerializeField] private VolumeType type;  // インスペクターで指定

    //====================================================================
    // 🎬 Start：スライダーをAudioManagerに登録
    //====================================================================
    private void Start()
    {
        // AudioManagerが存在しない場合（ロード順の問題など）は何もしない
        if (AudioManager.Instance == null) return;

        // このGameObjectに付いているSliderコンポーネントを取得
        var slider = GetComponent<Slider>();

        // nullチェック（もしSliderがない場合は警告を出す）
        if (slider == null)
        {
            Debug.LogWarning($"{name} にSliderコンポーネントが見つかりません。VolumeSliderBinderを使用するにはSliderが必要です。");
            return;
        }

        // 指定されたタイプに応じてAudioManagerに登録
        if (type == VolumeType.BGM)
        {
            // BGMスライダーとして登録
            AudioManager.Instance.BindBGMSlider(slider);
        }
        else
        {
            // SEスライダーとして登録
            AudioManager.Instance.BindSESlider(slider);
        }
    }
}
