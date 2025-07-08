using UnityEngine;

/// <summary>
/// 吹き出し内の文字オブジェクトが、
/// 親オブジェクトの左右反転（scale.x = -1）に影響されず、
/// 常に正しい向きで表示されるようにするクラス。
/// </summary>
public class BalloonTextUnflipper : MonoBehaviour
{
    /// <summary>
    /// 初期のローカルスケール（Textの元の大きさ・向き）を記録
    /// </summary>
    private Vector3 originalScale;

    void Start()
    {
        // 最初のスケールを記憶しておく（変更されない前提）
        originalScale = transform.localScale;
    }

    void LateUpdate()
    {
        // 親オブジェクトのワールドスケール（lossyScale）を取得
        // 親の scale.x が負の場合は左右反転していると判定
        Vector3 parentScale = transform.parent.lossyScale;

        // 文字の向きを常に正に保つため、親が反転していたら自分もX軸を反転して打ち消す
        float xScale = parentScale.x < 0 ? -originalScale.x : originalScale.x;

        // スケールを更新して反転を打ち消す（YとZはそのまま）
        transform.localScale = new Vector3(xScale, originalScale.y, originalScale.z);
    }
}
