// ステージセレクト>SceneManagerで使用

using UnityEngine;

public class SetPanelScript : MonoBehaviour
{
    [Header("音量設定パネル")]
    [SerializeField] private GameObject setPanel;

    // 初期状態は非表示
    private void Start()
    {
        if (setPanel != null)
            setPanel.SetActive(false);
    }

    // 表示
    public void OnPanel()
    {
        if (setPanel == null) return;
        setPanel.SetActive(true);

        Debug.Log("表示");
    }

    // 非表示
    public void OffPanel()
    {
        if (setPanel != null)
            setPanel.SetActive(false);

        Debug.Log("非表示");
    }

}
