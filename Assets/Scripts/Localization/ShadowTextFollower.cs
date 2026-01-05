using TMPro;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(TMP_Text))]
public class ShadowTextFollower : MonoBehaviour
{
    [SerializeField] private TMP_Text source; // 本体Text
    private TMP_Text _self;

    private void Awake()
    {
        _self = GetComponent<TMP_Text>();
    }

    private void LateUpdate()
    {
        if (source == null) return;
        if (_self == null) _self = GetComponent<TMP_Text>();

        // 影は文字だけ追従（必要ならfontSize等も追従してOK）
        _self.text = source.text;
    }
}
