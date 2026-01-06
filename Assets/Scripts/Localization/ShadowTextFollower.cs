using TMPro;
using UnityEngine;

/// <summary>
/// 本体 TMP_Text の内容をコピーして表示する「影用テキスト」コンポーネント
/// 
/// テキスト内容だけを追従させることで、
/// 疑似的な影・アウトライン表現を実現する
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(TMP_Text))]
public class ShadowTextFollower : MonoBehaviour
{
    // 追従元となる本体テキスト
    [SerializeField] private TMP_Text source;

    // 自身の TMP_Text
    private TMP_Text _self;

    /// <summary>
    /// 初期化
    /// </summary>
    private void Awake()
    {
        _self = GetComponent<TMP_Text>();
    }

    /// <summary>
    /// 毎フレーム、本体テキストの内容をコピーする
    /// LateUpdate にすることで、
    /// 他のスクリプト（ローカライズ・アニメーション等）の
    /// 更新後の最終状態を反映できる
    /// </summary>
    private void LateUpdate()
    {
        // 追従元が未設定なら何もしない
        if (source == null) return;

        // ExecuteAlways 対応のため、念のため null チェック
        if (_self == null)
            _self = GetComponent<TMP_Text>();

        // 影テキストは「文字内容のみ」追従させる
        // 必要に応じて fontSize / font / alignment なども同期可能
        _self.text = source.text;
    }
}
