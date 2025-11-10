using UnityEngine;

public class TutorialSign : MonoBehaviour
{
    public enum TutorialType
    {
        Move,
        Wire,
        Attack
    }

    [Header("設定")]
    [SerializeField] private TutorialType tutorialType; // 看板ごとのチュートリアル種類
    [SerializeField] private Transform player;           // プレイヤーのTransform
    [SerializeField] private float showDistance = 3f;    // 表示距離

    [Header("デモオブジェクト")]
    [SerializeField] private GameObject moveDemo;
    [SerializeField] private GameObject wireDemo;
    [SerializeField] private GameObject attackDemo;

    [Header("Animator設定")]
    [SerializeField] private Animator demoAnimator;      // 共通Animator
    [SerializeField] private string triggerMove = "Move";
    [SerializeField] private string triggerWire = "Wire";
    [SerializeField] private string triggerAttack = "Attack";
    [SerializeField] private string triggerIdle = "Idle";

    private bool isVisible = false;
    private GameObject activeDemo;

    private void Start()
    {
        // すべて非表示にして初期化
        if (moveDemo != null) moveDemo.SetActive(false);
        if (wireDemo != null) wireDemo.SetActive(false);
        if (attackDemo != null) attackDemo.SetActive(false);

        // 種類ごとに使うデモを決定
        switch (tutorialType)
        {
            case TutorialType.Move: activeDemo = moveDemo; break;
            case TutorialType.Wire: activeDemo = wireDemo; break;
            case TutorialType.Attack: activeDemo = attackDemo; break;
        }

        if (activeDemo != null)
            activeDemo.SetActive(false);
    }

    private void Update()
    {
        if (player == null || activeDemo == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= showDistance && !isVisible)
        {
            ShowDemo(true);
        }
        else if (distance > showDistance && isVisible)
        {
            ShowDemo(false);
        }
    }

    private void ShowDemo(bool show)
    {
        isVisible = show;
        activeDemo.SetActive(show);

        if (demoAnimator == null) return;

        if (show)
        {
            // チュートリアル種類に応じてトリガー発火
            switch (tutorialType)
            {
                case TutorialType.Move:
                    demoAnimator.SetTrigger(triggerMove);
                    break;
                case TutorialType.Wire:
                    demoAnimator.SetTrigger(triggerWire);
                    break;
                case TutorialType.Attack:
                    demoAnimator.SetTrigger(triggerAttack);
                    break;
            }
        }
        else
        {
            // Idle トリガーを発火して待機状態へ戻す
            demoAnimator.SetTrigger(triggerIdle);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, showDistance);
    }
#endif
}
