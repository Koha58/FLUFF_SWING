using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UIボタンにカーソルが乗っている間だけ、子オブジェクト（説明テキストなど）を表示するクラス。
/// ボタンにこのスクリプトをアタッチし、表示対象のオブジェクトをインスペクターから設定する。
/// </summary>
public class HoverTextDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("マウスオーバー時に表示させる子オブジェクト")]
    [SerializeField]
    private GameObject targetTextObject;

    /// <summary>
    /// 初期化時に対象オブジェクトを非表示にする。
    /// </summary>
    private void Awake()
    {
        if (targetTextObject != null)
        {
            targetTextObject.SetActive(false); // ゲーム開始時は非表示
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] targetTextObject が設定されていません。");
        }
    }

    /// <summary>
    /// マウスカーソルがボタンに入った時に呼ばれる。
    /// 指定されたオブジェクトを表示する。
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetTextObject != null)
        {
            targetTextObject.SetActive(true);
        }
    }

    /// <summary>
    /// マウスカーソルがボタンから離れた時に呼ばれる。
    /// 指定されたオブジェクトを非表示にする。
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetTextObject != null)
        {
            targetTextObject.SetActive(false);
        }
    }
}
