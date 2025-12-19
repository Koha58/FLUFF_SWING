using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// クレジットのコンテンツを構築し、自動的にスクロールさせるメインコントローラー。
/// Unityの標準ScrollRectを使用せず、RectTransformを直接操作してコンテンツをスムーズに移動させる。
/// </summary>
public class CreditScroller : MonoBehaviour
{
    // === UI設定 (インスペクターで設定するため [SerializeField] と private を併用) ===
    [Header("UI")]
    [Tooltip("スクロールコンテンツの表示領域（通常はMaskコンポーネントがあるRectTransform）")]
    [SerializeField] private RectTransform viewport;
    [Tooltip("スクロール対象のコンテンツ全体を保持するRectTransform（VerticalLayoutGroup, ContentSizeFitterが付く）")]
    [SerializeField] private RectTransform content;
    [Tooltip("クレジットデータが格納されたScriptableObject")]
    [SerializeField] private CreditDataSO creditData;

    // === Prefabs ===
    [Header("Prefabs")]
    [SerializeField] private GameObject logoRowPrefab;
    [SerializeField] private GameObject roleRowPrefab;
    [SerializeField] private GameObject thankYouRowPrefab;
    [SerializeField] private GameObject spacerRowPrefab;

    // === スクロール設定 ===
    [Header("Scroll")]
    [SerializeField] private float scrollSpeed = 80f;
    [Tooltip("コンテンツの下端がViewportの下端からどれだけ離れた位置（負のY）でスクロールを開始するか")]
    [SerializeField] private float startPadding = 0f;
    [Tooltip("コンテンツの上端がViewportの上端からどれだけ離れた位置（正のY）でスクロールを停止するか")]
    [SerializeField] private float endOffset = 400f;

    // === シーン遷移設定 ===
    [Header("Scene Transition")]
    [Tooltip("遷移先のタイトルシーン名")]
    [SerializeField] private string titleSceneName = "TitleScene";

    // === 画像フェード制御 ===
    [Header("Image Fade Controllers")]
    [Tooltip("画面外に固定され、スクロール位置に応じてフェードイン/アウトする画像コントローラーのリスト")]
    [SerializeField] private CreditImageController[] imageControllers;

    // === ロール行の詳細設定 ===
    [Header("Role Row Layout")]
    [Tooltip("役職名と担当者名の間の縦方向のスペーシング")]
    [SerializeField] private float roleNameGap = 120f;
    [Tooltip("担当者名リスト内の行間")]
    [SerializeField] private float nameLineSpacing = 50f;

    // === 内部コンテンツアンカー設定 ===
    [Header("Internal Content Anchor (Crucial for Scroll Logic)")]
    [Tooltip("ContentのピボットX座標。0.5f（中央）推奨。")]
    [SerializeField] private float contentPivotX = 0.5f;
    [Tooltip("ContentのピボットY座標。Y=0f（下端）が必須。")]
    [SerializeField] private float contentPivotY = 0f;
    [Tooltip("ContentのアンカーMin/Max X座標。0.5f（中央）推奨。")]
    [SerializeField] private float contentAnchorX = 0.5f;
    [Tooltip("ContentのアンカーMin/Max Y座標。Y=0f（下端）が必須。")]
    [SerializeField] private float contentAnchorY = 0f;

    // === プライベートな状態変数 ===
    private bool scrolling;        // スクロール中かどうか
    private float contentHeight;   // コンテンツの全長
    private float viewportHeight;  // Viewportの高さ

    private ScrollRect _scrollRect; // 既存のScrollRectを参照するための変数

    private void Awake()
    {
        // レイアウトグループの設定を強制
        ForceContentLayoutSettings();

        // コンテンツのアンカーとピボットを「下端中央」に設定する
        content.pivot = new Vector2(contentPivotX, contentPivotY);
        content.anchorMin = new Vector2(contentAnchorX, contentAnchorY);
        content.anchorMax = new Vector2(contentAnchorX, contentAnchorY);

        // ScrollRectが存在する場合、その自動制御を無効にする
        _scrollRect = viewport ? viewport.GetComponentInParent<ScrollRect>() : null;
        if (_scrollRect != null)
        {
            _scrollRect.content = content;
            _scrollRect.enabled = false; // ScrollRectによる位置の上書きを停止
        }
    }

    /// <summary>
    /// コンテンツRectTransformに付いているVerticalLayoutGroupの動作を強制的に設定する。
    /// </summary>
    private void ForceContentLayoutSettings()
    {
        var v = content.GetComponent<VerticalLayoutGroup>();
        if (v != null)
        {
            v.childControlHeight = true;
            v.childControlWidth = true;
            v.childForceExpandHeight = false; // LayoutElementのminHeight/preferredHeightを尊重
            v.childForceExpandWidth = true;
        }
    }

    private void Start()
    {
        // アプリ起動時にクレジット構築とスクロールを開始
        RebuildAll();
    }

    /// <summary>
    /// クレジットの構築、レイアウトの計算、スクロールの開始処理全体を実行する。
    /// </summary>
    private void RebuildAll()
    {
        StopAllCoroutines();
        scrolling = false;

        ClearContentChildren(); // 既存の子要素を破棄
        BuildFromData();        // データから新規コンテンツを生成
        StartCoroutine(BeginScrollAfterLayout()); // レイアウト完了を待ってスクロール開始
    }

    /// <summary>
    /// Contentオブジェクトの子要素をすべて削除する。
    /// </summary>
    private void ClearContentChildren()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);
    }

    /// <summary>
    /// ScriptableObjectのデータに基づいてクレジットコンテンツを生成する。
    /// </summary>
    private void BuildFromData()
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
                    // スペーサーの高さをパースして生成
                    if (float.TryParse(e.title, out float h))
                        CreateSpacer(h);
                    break;
                case "END":
                    return; // データ終端
            }
        }
    }

    /// <summary>
    /// ロゴ行を生成し、Resourcesから対応するPrefabをロードする。
    /// </summary>
    private void CreateLogo(string prefabName)
    {
        // 拡張子を取り除き、パスを構築
        prefabName = StripExtension(prefabName);
        string path = $"CreditData/{prefabName}";

        var logoPrefab = Resources.Load<GameObject>(path);
        if (logoPrefab == null)
        {
            Debug.LogError($"[Credits] Logo prefab not found: Resources/{path}");
            return;
        }

        Instantiate(logoPrefab, content);
    }


    /// <summary>
    /// 役職名と担当者名の行を生成し、テキストとレイアウトを設定する。
    /// </summary>
    private void CreateRole(string role, string names)
    {
        var row = Instantiate(roleRowPrefab, content);

        var roleText = row.transform.Find("RoleText")?.GetComponent<TextMeshProUGUI>();
        var nameText = row.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();

        if (roleText)
        {
            roleText.text = role;
            // その他のTMP設定
            roleText.textWrappingMode = TextWrappingModes.Normal;
            roleText.margin = Vector4.zero;
            roleText.lineSpacing = 0;
            roleText.paragraphSpacing = 0;
            roleText.verticalAlignment = VerticalAlignmentOptions.Top;
        }

        if (nameText)
        {
            // 名前をカンマ区切りから改行区切りに変換
            nameText.text = (names ?? "")
                .Replace(", ", "\n")
                .Replace(",", "\n")
                .Trim();

            // その他のTMP設定
            nameText.textWrappingMode = TextWrappingModes.Normal;
            nameText.margin = Vector4.zero;
            nameText.lineSpacing = nameLineSpacing; // フィールド値を使用
            nameText.paragraphSpacing = 0;
            nameText.verticalAlignment = VerticalAlignmentOptions.Top;
        }

        // RoleRowに縦レイアウトを設定（役職と名前の配置）
        var v = row.GetComponent<VerticalLayoutGroup>() ?? row.AddComponent<VerticalLayoutGroup>();
        v.childControlWidth = true;
        v.childControlHeight = true;
        v.childForceExpandWidth = true;
        v.childForceExpandHeight = false;
        v.spacing = roleNameGap; // フィールド値を使用
        v.childAlignment = TextAnchor.UpperCenter;

        // TextMeshProUGUIの計算結果に基づいて行の高さを設定
        ApplyPreferredHeightFromTMP(row, padding: 0f);
    }

    /// <summary>
    /// 'Thank You' などの単一メッセージ行を生成する。
    /// </summary>
    private void CreateThankYou(string text)
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

    /// <summary>
    /// 指定された高さのスペーサー（空行）を生成する。
    /// </summary>
    private void CreateSpacer(float height)
    {
        var row = Instantiate(spacerRowPrefab, content);
        var le = EnsureLayoutElement(row);
        le.minHeight = height;
        le.preferredHeight = height;
        le.flexibleHeight = 0f;
    }

    /// <summary>
    /// 指定されたGameObjectにLayoutElementが存在することを保証し、存在しない場合は追加して返す。
    /// </summary>
    private LayoutElement EnsureLayoutElement(GameObject row)
    {
        var le = row.GetComponent<LayoutElement>();
        if (le == null) le = row.AddComponent<LayoutElement>();
        return le;
    }

    /// <summary>
    /// 子要素のTextMeshProUGUIの計算された高さに基づき、LayoutElementのminHeight/preferredHeightを設定する。
    /// </summary>
    private void ApplyPreferredHeightFromTMP(GameObject row, float padding)
    {
        var le = EnsureLayoutElement(row);

        float max = 0f;
        var tmps = row.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var t in tmps)
        {
            // レイアウトを強制更新して正確な高さを取得
            t.ForceMeshUpdate();
            max = Mathf.Max(max, t.preferredHeight);
        }

        le.minHeight = max + padding;
        le.preferredHeight = max + padding;
        le.flexibleHeight = 0f;
    }

    /// <summary>
    /// レイアウトの再構築が完了した後、スクロールを開始するコルーチン。
    /// </summary>
    private IEnumerator BeginScrollAfterLayout()
    {
        // 1. レイアウトの初期再構築
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        yield return null;

        // 2. 幅の強制設定と再構築（折り返し対策）
        float viewW = viewport ? viewport.rect.width : ((RectTransform)content.parent).rect.width;
        ForceFullWidth(viewW);
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        yield return null;

        // 3. スクロールに必要な高さを取得
        contentHeight = content.rect.height;
        float viewH = viewport ? viewport.rect.height : Screen.height;
        viewportHeight = viewH;

        // 4. スクロール開始位置を設定（画面外の下方）
        content.anchoredPosition = new Vector2(0f, -contentHeight - startPadding);

        // 5. 画像コントローラーを初期化し、非表示状態にする
        InitializeImages(viewW, viewH, contentHeight);

        // 最後の画像コントローラーにイベントを登録
        RegisterLastImageHiddenEvent();

        // スクロール開始フラグを立てる
        scrolling = true;

        Debug.Log($"startPadding={startPadding}, viewH={viewH}, contentHeight={contentHeight}, startY={content.anchoredPosition.y}");
    }

    /// <summary>
    /// 最後の画像コントローラーのイベントを購読する
    /// </summary>
    private void RegisterLastImageHiddenEvent()
    {
        // imageControllers配列に要素があり、かつ、有効なインスタンスが設定されているか確認
        if (imageControllers != null && imageControllers.Length > 0)
        {
            // 配列の最後の要素を取得 (これがクレジットの最後にフェードアウトする画像だと仮定)
            CreditImageController lastImage = imageControllers[imageControllers.Length - 1];

            if (lastImage != null)
            {
                // イベントハンドラを登録
                // 画像が完全に消えたら GoToTitleScene メソッドを呼び出す
                lastImage.OnFullyHidden -= GoToTitleScene; // 二重登録防止のため
                lastImage.OnFullyHidden += GoToTitleScene;
            }
        }
    }

    /// <summary>
    /// 最後の画像が完全に非表示になったときに呼び出される。タイトルシーンへ遷移する。
    /// </summary>
    private void GoToTitleScene()
    {
        // スクロールを停止
        scrolling = false;

        Debug.Log($"最後の画像が非表示になりました。'{titleSceneName}' シーンに遷移します。");

        // シーン遷移を実行
        SceneManager.LoadScene(titleSceneName);
    }

    /// <summary>
    /// 登録されている全ての画像コントローラーを初期化する。
    /// </summary>
    private void InitializeImages(float viewW, float viewH, float contentH)
    {
        if (imageControllers == null) return;

        foreach (var controller in imageControllers)
        {
            if (controller)
            {
                controller.Initialize(viewW, viewH, contentH);
            }
        }
    }

    /// <summary>
    /// コンテンツとそのすべての子要素の幅を強制的に指定された幅にする。
    /// </summary>
    private void ForceFullWidth(float width)
    {
        // コンテンツ自身の幅を設定
        var contentLE = content.GetComponent<LayoutElement>() ?? content.gameObject.AddComponent<LayoutElement>();
        contentLE.minWidth = width;
        contentLE.preferredWidth = width;
        contentLE.flexibleWidth = 0f;

        // 子要素（行）の幅を設定
        for (int i = 0; i < content.childCount; i++)
        {
            var row = content.GetChild(i) as RectTransform;
            if (!row) continue;

            var rowLE = row.GetComponent<LayoutElement>() ?? row.gameObject.AddComponent<LayoutElement>();
            rowLE.minWidth = width;
            rowLE.preferredWidth = width;
            rowLE.flexibleWidth = 0f;

            // さらに、TextMeshProUGUIコンポーネントの幅も設定
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

    private void Update()
    {
        // スクロール中でなければ処理をスキップ
        if (!scrolling) return;

        // 1. スクロール処理: 毎フレーム、上方向に移動
        content.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

        // 2. 画像フェード処理: 全ての画像コントローラーに現在のコンテンツY座標を渡す
        float currentY = content.anchoredPosition.y;
        if (imageControllers != null)
        {
            foreach (var controller in imageControllers)
            {
                if (controller)
                {
                    // CreditImageControllerが透明度の計算と更新を行う
                    controller.UpdateImageState(currentY, contentHeight);
                }
            }
        }

        // 3. スクロール終了判定: コンテンツの上端がViewportの上端を越えたら停止
        float viewH = viewportHeight;
        if (content.anchoredPosition.y >= viewH + endOffset)
            scrolling = false;

    }

    /// <summary>
    /// ファイル名から拡張子（.prefabなど）を取り除く。
    /// </summary>
    private static string StripExtension(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        int dot = name.LastIndexOf('.');
        return (dot > 0) ? name.Substring(0, dot) : name;
    }
}