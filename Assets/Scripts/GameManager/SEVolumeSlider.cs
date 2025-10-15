using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 🎚 SEスライダーで、ユーザーが指やマウスを離したタイミングを検知して
/// AudioManagerに確認用SE再生を依頼するスクリプト。
/// </summary>
public class SEVolumeSlider : MonoBehaviour, IPointerUpHandler
{
    public void OnPointerUp(PointerEventData eventData)
    {
        if (AudioManager.Instance != null && AudioManager.Instance.TestSE != null)
        {
            AudioManager.Instance.PlaySE(AudioManager.Instance.TestSE);
        }
    }
}
