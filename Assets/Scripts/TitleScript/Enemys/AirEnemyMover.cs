using UnityEngine;

/// <summary>
/// 空中敵用の移動制御コンポーネント。
/// 
/// ・当たり判定や物理挙動は一切使わない
/// ・Transform を直接動かすだけの軽量な演出用スクリプト
/// ・初期位置（生成位置）を基準に、
///   上下・円・ジグザグなどの動きを付加する
/// 
/// 主な用途：
/// ・タイトル画面の演出敵
/// ・背景を流れる飛行物体
/// </summary>
public class AirEnemyMover : MonoBehaviour
{
    /// <summary>
    /// 空中敵の移動パターン定義
    /// </summary>
    public enum Pattern
    {
        Hover,      // ほぼその場でわずかに上下（控えめ演出用）
        SineUpDown, // サイン波で滑らかに上下
        ZigZag,     // 角ばった上下（ピンポン移動）
        Circle      // 円運動（回り込み演出）
    }

    // =========================================================
    // Motion Parameters
    // =========================================================

    [Header("Motion")]

    [Tooltip("使用する移動パターン")]
    [SerializeField] private Pattern pattern = Pattern.SineUpDown;

    [Tooltip("上下移動の振れ幅（小さいほど控えめ）")]
    [SerializeField] private float amplitude = 0.8f;

    [Tooltip("動きの速さ（周波数）")]
    [SerializeField] private float frequency = 1.2f;

    [Tooltip("横方向への流れ速度（0なら上下動のみ）")]
    [SerializeField] private float horizontalSpeed = 0f;

    // =========================================================
    // Internal State
    // =========================================================

    /// <summary>
    /// 生成時の基準位置。
    /// すべての動きはこの位置を中心に行われる。
    /// </summary>
    private Vector3 startPos;

    /// <summary>
    /// 動き開始時刻。
    /// Time.time を直接使わず差分で扱うために保持。
    /// </summary>
    private float t0;

    // =========================================================
    // Unity Lifecycle
    // =========================================================

    void Start()
    {
        // 生成時の位置を記録
        // → 空中敵は「生成された高さ」を中心に動く
        startPos = transform.position;

        // 動き開始の基準時刻
        t0 = Time.time;
    }

    void Update()
    {
        // 経過時間（生成から何秒経ったか）
        float t = Time.time - t0;

        // 移動量（基準位置からのオフセット）
        float y = 0f;

        // 横移動は常に左方向に流す想定
        // ※ タイトル画面で右方向に進むゲーム前提
        float x = -horizontalSpeed * t;

        switch (pattern)
        {
            case Pattern.Hover:
            case Pattern.SineUpDown:
                // サイン波で滑らかな上下移動
                y = Mathf.Sin(t * frequency) * amplitude;
                break;

            case Pattern.ZigZag:
                // PingPong を使った角ばった上下移動
                // -1 ～ 1 の範囲を行き来する
                y = Mathf.PingPong(t * frequency, 2f) - 1f;
                y *= amplitude;
                break;

            case Pattern.Circle:
                // 円運動（横＋縦）
                // horizontalSpeed に加えて円運動分の横揺れを足す
                x += Mathf.Cos(t * frequency) * amplitude;
                y = Mathf.Sin(t * frequency) * amplitude;
                break;
        }

        // 基準位置 + オフセットで最終位置を決定
        transform.position = startPos + new Vector3(x, y, 0f);
    }

    // =========================================================
    // External Control
    // =========================================================

    /// <summary>
    /// スポナー側から呼ばれ、移動パラメータをランダムに設定する。
    /// 
    /// ・Prefabごとに細かい設定を持たせなくて済む
    /// ・出現ごとに挙動を変えられる
    /// </summary>
    public void Randomize(
        Pattern[] patterns,
        Vector2 ampRange,
        Vector2 freqRange,
        Vector2 hSpeedRange)
    {
        // 使用するパターンをランダム選択
        if (patterns != null && patterns.Length > 0)
        {
            pattern = patterns[Random.Range(0, patterns.Length)];
        }

        // 各パラメータを指定範囲からランダム取得
        amplitude = Random.Range(ampRange.x, ampRange.y);
        frequency = Random.Range(freqRange.x, freqRange.y);
        horizontalSpeed = Random.Range(hSpeedRange.x, hSpeedRange.y);

        // ランダム化後は現在位置を新しい基準位置として再設定
        // → 出現直後から自然に動き始める
        startPos = transform.position;
        t0 = Time.time;
    }
}
