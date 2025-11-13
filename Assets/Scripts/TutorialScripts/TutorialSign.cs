using UnityEngine;

/// <summary>
/// プレイヤーが看板（チュートリアルポイント）に近づくと、
/// 対応するチュートリアルデモ（アニメーションなど）を表示するスクリプト。
/// </summary>
public class TutorialSign : MonoBehaviour
{
    // --- チュートリアルの種類 ---
    public enum TutorialType
    {
        Move,       // 移動チュートリアル
        Wire,       // ワイヤーアクションチュートリアル
        WireSkill,  // ワイヤースキルチュートリアル
        Attack      // 攻撃チュートリアル
    }

    [Header("設定")]
    [SerializeField] private TutorialType tutorialType; // この看板が担当するチュートリアル種類
    [SerializeField] private Transform player;           // プレイヤーのTransform（距離判定用）
    [SerializeField] private float showDistance = 3f;    // プレイヤーが近づいたら表示される距離

    [Header("デモオブジェクト")]
    [SerializeField] private GameObject moveDemo;        // 移動デモ
    [SerializeField] private GameObject wireDemo;        // ワイヤーデモ
    [SerializeField] private GameObject wireSkillDemo;   // ワイヤースキルデモ
    [SerializeField] private GameObject attackDemo;      // 攻撃デモ

    [Header("Animator設定")]
    [SerializeField] private Animator demoAnimator;      // 共通アニメーター（全デモで共有）
    [SerializeField] private string triggerMove = "Move";        // 移動デモ用トリガー名
    [SerializeField] private string triggerWire = "Wire";        // ワイヤーデモ用トリガー名
    [SerializeField] private string triggerWireSkill = "WireSkill"; // ワイヤースキルデモ用トリガー名
    [SerializeField] private string triggerAttack = "Attack";    // 攻撃デモ用トリガー名
    [SerializeField] private string triggerIdle = "Idle";        // 待機状態へ戻すトリガー名

    private bool isVisible = false;    // 現在デモが表示中かどうか
    private GameObject activeDemo;     // この看板が使用するデモオブジェクト

    private void Start()
    {
        // --- 初期化処理 ---

        // すべてのデモを非表示にしておく（安全策）
        if (moveDemo != null) moveDemo.SetActive(false);
        if (wireDemo != null) wireDemo.SetActive(false);
        if (wireSkillDemo != null) wireSkillDemo.SetActive(false);
        if (attackDemo != null) attackDemo.SetActive(false);

        // この看板で使うデモオブジェクトを種類に応じて設定
        switch (tutorialType)
        {
            case TutorialType.Move: activeDemo = moveDemo; break;
            case TutorialType.Wire: activeDemo = wireDemo; break;
            case TutorialType.WireSkill: activeDemo = wireSkillDemo; break;
            case TutorialType.Attack: activeDemo = attackDemo; break;
        }

        // 念のため非表示で開始
        if (activeDemo != null)
            activeDemo.SetActive(false);
    }

    private void Update()
    {
        // プレイヤーまたはデモが設定されていない場合は何もしない
        if (player == null || activeDemo == null) return;

        // プレイヤーとの距離を計算
        float distance = Vector2.Distance(transform.position, player.position);

        // 表示距離以内に入ったらデモを表示
        if (distance <= showDistance && !isVisible)
        {
            ShowDemo(true);
        }
        // 表示距離を離れたらデモを非表示
        else if (distance > showDistance && isVisible)
        {
            ShowDemo(false);
        }
    }

    /// <summary>
    /// デモの表示・非表示を切り替える処理
    /// </summary>
    /// <param name="show">trueで表示、falseで非表示</param>
    private void ShowDemo(bool show)
    {
        isVisible = show;                // 現在の表示状態を記録
        activeDemo.SetActive(show);      // オブジェクトの表示切り替え

        // アニメーターが設定されていない場合はここで終了
        if (demoAnimator == null) return;

        if (show)
        {
            // チュートリアルの種類に応じてアニメーションを再生
            switch (tutorialType)
            {
                case TutorialType.Move:
                    demoAnimator.SetTrigger(triggerMove);
                    break;
                case TutorialType.Wire:
                    demoAnimator.SetTrigger(triggerWire);
                    break;
                case TutorialType.WireSkill:
                    demoAnimator.SetTrigger(triggerWireSkill);
                    break;
                case TutorialType.Attack:
                    demoAnimator.SetTrigger(triggerAttack);
                    break;
            }
        }
        else
        {
            // 離れたときはIdleトリガーを発火して待機状態へ
            demoAnimator.SetTrigger(triggerIdle);
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// エディタ上で選択したときに表示される「表示距離」のガイドライン
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, showDistance);
    }
#endif
}
