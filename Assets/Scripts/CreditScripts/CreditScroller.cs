using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreditScroller : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] RectTransform viewport; // Maskが付いてる見える枠（無ければcontentの親）
    [SerializeField] RectTransform content;  // VerticalLayoutGroup + ContentSizeFitter
    [SerializeField] CreditDataSO creditData;

    [Header("Prefabs")]
    [SerializeField] GameObject logoRowPrefab;
    [SerializeField] GameObject roleRowPrefab;
    [SerializeField] GameObject thankYouRowPrefab;
    [SerializeField] GameObject spacerRowPrefab;

    [Header("Scroll")]
    [SerializeField] float scrollSpeed = 80f;
    [SerializeField] float startPadding = 200f; // 画面下から何px下で開始するか
    [SerializeField] float endOffset = 400f;    // +にすると遅く止まる

    bool scrolling;
    float contentHeight;

    ScrollRect _scrollRect; // ★追加：ScrollRect上書き対策

    void Awake()
    {
        ForceContentLayoutSettings();

        // ★スクロール用に content の基準を「下」に揃える（超重要）
        content.pivot = new Vector2(0.5f, 0f);
        content.anchorMin = new Vector2(0.5f, 0f);
        content.anchorMax = new Vector2(0.5f, 0f);

        // ===== 解決策2：ScrollRectがcontentを毎フレーム上書きするのを止める =====
        _scrollRect = viewport ? viewport.GetComponentInParent<ScrollRect>() : null;
        if (_scrollRect != null)
        {
            // 念のため、このcontentを参照させておく（事故防止）
            _scrollRect.content = content;

            // 自動で位置を書き換えられるのを止める
            _scrollRect.enabled = false;
        }
    }

    void ForceContentLayoutSettings()
    {
        var v = content.GetComponent<VerticalLayoutGroup>();
        if (v != null)
        {
            v.childControlHeight = true;
            v.childControlWidth = true;
            v.childForceExpandHeight = false; // ★LINEBREAKを効かせる
            v.childForceExpandWidth = true;
        }
    }

    void Start()
    {
        RebuildAll();
    }

    void RebuildAll()
    {
        StopAllCoroutines();
        scrolling = false;

        ClearContentChildren();

        BuildFromData();
        StartCoroutine(BeginScrollAfterLayout());
    }

    void ClearContentChildren()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);
    }

    void BuildFromData()
    {
        foreach (var e in creditData.entries)
        {
            switch (e.type)
            {
                case "LOGO":
                    CreateLogo(e.title);
                    break;

                case "ROLE":
                    CreateRole(e.title, e.name);
                    break;

                case "THANKYOU":
                    CreateThankYou(e.title);
                    break;

                case "LINEBREAK":
                    if (float.TryParse(e.title, out float h))
                        CreateSpacer(h);
                    break;

                case "END":
                    return;
            }
        }
    }

    void CreateLogo(string prefabName)
    {
        // "TitleLogo.prefab" でも来たら拡張子を外す
        prefabName = StripExtension(prefabName);

        // Resources から Prefab を読む（例：Assets/Resources/CreditData/TitleLogo.prefab）
        // 置き場所に合わせてフォルダを付ける
        string path = $"CreditData/{prefabName}";

        var logoPrefab = Resources.Load<GameObject>(path);
        if (logoPrefab == null)
        {
            Debug.LogError($"[Credits] Logo prefab not found: Resources/{path}");
            return;
        }

        var row = Instantiate(logoPrefab, content);
    }


    void CreateRole(string role, string names)
    {
        const float ROLE_NAME_GAP = 120f;      // RoleText と NameText の間
        const float NAME_LINE_SPACING = 50f;   // NameText 内（複数名の間）

        var row = Instantiate(roleRowPrefab, content);

        var roleText = row.transform.Find("RoleText")?.GetComponent<TextMeshProUGUI>();
        var nameText = row.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();

        if (roleText)
        {
            roleText.text = role;
            roleText.textWrappingMode = TextWrappingModes.Normal;
            roleText.margin = Vector4.zero;
            roleText.lineSpacing = 0;
            roleText.paragraphSpacing = 0;
            roleText.verticalAlignment = VerticalAlignmentOptions.Top;
        }

        if (nameText)
        {
            nameText.text = (names ?? "")
                .Replace(", ", "\n")
                .Replace(",", "\n")
                .Trim();

            nameText.textWrappingMode = TextWrappingModes.Normal;
            nameText.margin = Vector4.zero;
            nameText.lineSpacing = NAME_LINE_SPACING;
            nameText.paragraphSpacing = 0;
            nameText.verticalAlignment = VerticalAlignmentOptions.Top;
        }

        // RoleRowに縦レイアウトを追加（Prefabに無くてOK）
        var v = row.GetComponent<VerticalLayoutGroup>() ?? row.AddComponent<VerticalLayoutGroup>();
        v.childControlWidth = true;
        v.childControlHeight = true;
        v.childForceExpandWidth = true;
        v.childForceExpandHeight = false;
        v.spacing = ROLE_NAME_GAP;
        v.childAlignment = TextAnchor.UpperCenter;

        ApplyPreferredHeightFromTMP(row, padding: 0f);
    }

    void CreateThankYou(string text)
    {
        var row = Instantiate(thankYouRowPrefab, content);

        var tmp = row.GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmp)
        {
            tmp.text = text;
            tmp.textWrappingMode = TextWrappingModes.Normal;
        }

        EnsureLayoutElement(row);
        ApplyPreferredHeightFromTMP(row, padding: 0f);
    }

    void CreateSpacer(float height)
    {
        var row = Instantiate(spacerRowPrefab, content);
        var le = EnsureLayoutElement(row);
        le.minHeight = height;
        le.preferredHeight = height;
        le.flexibleHeight = 0f;
    }

    LayoutElement EnsureLayoutElement(GameObject row)
    {
        var le = row.GetComponent<LayoutElement>();
        if (le == null) le = row.AddComponent<LayoutElement>();
        return le;
    }

    void ApplyPreferredHeightFromTMP(GameObject row, float padding)
    {
        var le = EnsureLayoutElement(row);

        float max = 0f;
        var tmps = row.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var t in tmps)
        {
            t.ForceMeshUpdate();
            max = Mathf.Max(max, t.preferredHeight);
        }

        le.minHeight = max + padding;
        le.preferredHeight = max + padding;
        le.flexibleHeight = 0f;
    }

    IEnumerator BeginScrollAfterLayout()
    {
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        yield return null;

        // 幅を強制（縦書き化対策）
        float viewW = viewport ? viewport.rect.width : ((RectTransform)content.parent).rect.width;
        ForceFullWidth(viewW);
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        yield return null;

        contentHeight = content.rect.height;

        float viewH = viewport ? viewport.rect.height : Screen.height;

        content.anchoredPosition = new Vector2(0f, -contentHeight - startPadding);

        scrolling = true;

        Debug.Log($"startPadding={startPadding}, viewH={viewH}, contentHeight={contentHeight}, startY={content.anchoredPosition.y}");
    }

    void ForceFullWidth(float width)
    {
        var contentLE = content.GetComponent<LayoutElement>() ?? content.gameObject.AddComponent<LayoutElement>();
        contentLE.minWidth = width;
        contentLE.preferredWidth = width;
        contentLE.flexibleWidth = 0f;

        for (int i = 0; i < content.childCount; i++)
        {
            var row = content.GetChild(i) as RectTransform;
            if (!row) continue;

            var rowLE = row.GetComponent<LayoutElement>() ?? row.gameObject.AddComponent<LayoutElement>();
            rowLE.minWidth = width;
            rowLE.preferredWidth = width;
            rowLE.flexibleWidth = 0f;

            foreach (var tmp in row.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                var le = tmp.GetComponent<LayoutElement>() ?? tmp.gameObject.AddComponent<LayoutElement>();
                le.minWidth = width;
                le.preferredWidth = width;
                le.flexibleWidth = 0f;

                tmp.textWrappingMode = TextWrappingModes.Normal;
            }
        }
    }

    void Update()
    {
        Debug.Log($"nowY={content.anchoredPosition.y}");

        if (!scrolling) return;

        content.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

        float viewH = viewport ? viewport.rect.height : Screen.height;
        if (content.anchoredPosition.y >= viewH + endOffset)
            scrolling = false;

    }

    static string StripExtension(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        int dot = name.LastIndexOf('.');
        return (dot > 0) ? name.Substring(0, dot) : name;
    }
}
