using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshProを使う場合は必須
using System.Collections;
using System.Linq;

public class CreditScroller : MonoBehaviour
{
    [Header("Data & Prefabs")]
    // 実行時にResourcesフォルダからロードするアセット名
    public string creditDataAssetName = "CreditData";

    // Prefab化したクレジット一行のひな型 (ROLE用。役職と名前を縦に持つ)
    public GameObject creditItemPrefab;

    // タイトルなどの特殊な行の中央寄せテキストPrefab
    public GameObject titleTextPrefab;

    [Header("Scroll Settings")]
    public float scrollSpeed = 50f; // 画面上の移動速度 (px/秒)
    public float initialDelay = 1f;  // 開始前の待機時間

    private RectTransform contentRectTransform;
    private CreditDataSO creditData;
    private bool isScrolling = false;

    void Start()
    {
        contentRectTransform = GetComponent<RectTransform>();

        // ResourcesフォルダからCreditDataSOをロード
        creditData = Resources.Load<CreditDataSO>("CreditData/" + creditDataAssetName);

        if (creditData == null)
        {
            Debug.LogError($"CreditDataSO（{creditDataAssetName}）が見つかりません。パスを確認してください。");
            return;
        }

        // Contentの子要素を一旦クリア (エディタで配置ミスがないように)
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // データの生成とスクロールを開始
        StartCoroutine(InitializeCreditsAndStartScroll());
    }

    private IEnumerator InitializeCreditsAndStartScroll()
    {
        // データの生成
        GenerateAllCreditItems();

        // レイアウトグループがUI要素のサイズを計算し終えるのを待つ
        // 複数フレーム待機することで、より確実にサイズ調整を完了させる
        yield return null;
        yield return null;

        // Contentの開始位置を画面の下端に設定
        float contentHeight = contentRectTransform.rect.height;
        RectTransform viewportRect = contentRectTransform.parent.GetComponent<RectTransform>();
        float viewportHeight = viewportRect ? viewportRect.rect.height : Screen.height;

        // Contentの上端がViewportの下端に接する位置からスタート
        float startY = -contentHeight / 2 - viewportHeight / 2;

        // Contentのアンカーポイントが中央(0.5, 0.5)だと仮定して計算
        contentRectTransform.anchoredPosition = new Vector2(0, startY);

        yield return new WaitForSeconds(initialDelay);
        isScrolling = true;
    }

    void Update()
    {
        if (!isScrolling) return;

        // Contentオブジェクトを毎フレーム上に移動させる
        contentRectTransform.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

        // エンド判定
        float contentTopY = contentRectTransform.anchoredPosition.y + contentRectTransform.rect.height / 2;
        RectTransform viewportRect = contentRectTransform.parent.GetComponent<RectTransform>();
        float viewportTopY = viewportRect ? viewportRect.rect.height / 2 : Screen.height / 2;

        // Content全体が画面の上端を通り過ぎたら終了
        if (contentTopY > viewportTopY)
        {
            isScrolling = false;
            Debug.Log("エンドロール終了。タイトル画面へ遷移...");
            // SceneManager.LoadScene("TitleScene"); // 終了後のシーン遷移
        }
    }

    /// <summary>
    /// CreditDataSOのデータを元に、HierarchyにUI要素を動的に生成する
    /// </summary>
    private void GenerateAllCreditItems()
    {
        foreach (var entry in creditData.entries)
        {
            GameObject newObject = null;

            switch (entry.type)
            {
                case "ROLE":
                    if (creditItemPrefab == null) continue;
                    newObject = Instantiate(creditItemPrefab, transform);
                    SetupRoleItem(newObject, entry);
                    break;
                case "TITLE":
                    if (titleTextPrefab == null) continue;
                    newObject = Instantiate(titleTextPrefab, transform);
                    SetupTitleItem(newObject, entry);
                    break;
                case "LINEBREAK":
                    // Title列に記述された値をFloatに変換して高さを設定
                    if (float.TryParse(entry.title, out float height))
                    {
                        newObject = CreateSpacer(height);
                    }
                    break;
                case "LOGO":
                    // ロゴ画像用の専用Prefabを用意してください
                    // 例: newObject = Instantiate(logoImagePrefab, transform);
                    break;
                case "THANKYOU":
                    // 特別なメッセージ用Prefabを用意してください
                    // 例: newObject = Instantiate(specialThanksPrefab, transform);
                    break;
                case "END":
                    // 終了マークが来たら生成を停止
                    return;
            }
            if (newObject != null)
            {
                newObject.transform.SetParent(transform, false); // Contentの子として配置
            }
        }
    }

    // ROLEタイプの行をセットアップするメソッド
    private void SetupRoleItem(GameObject item, CreditEntry entry)
    {
        // 役職テキストの設定
        TextMeshProUGUI roleText = item.transform.Find("RoleText").GetComponent<TextMeshProUGUI>();
        roleText.text = entry.title; // 役職名

        // 複数名対応: NamesContainerの子に名前テキストを動的生成
        Transform namesContainer = item.transform.Find("NamesContainer");

        if (namesContainer != null && namesContainer.GetComponent<VerticalLayoutGroup>() != null)
        {
            // 名前をカンマ区切りで分割
            string[] names = entry.name.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray();

            // 名前ごとにTextコンポーネントをNamesContainerの子として生成する処理
            foreach (string n in names)
            {
                // ここで名前表示用のPrefab（NameTextPrefabなど）を用意し、生成することが理想的
                // 仮として、既存のRoleTextを複製して使用する簡易的な方法:
                GameObject nameObj = Instantiate(roleText.gameObject, namesContainer);
                nameObj.name = "NameText_" + n.Replace(" ", "_");
                nameObj.GetComponent<TextMeshProUGUI>().text = n;
                // フォントサイズなどを調整する
                nameObj.GetComponent<TextMeshProUGUI>().fontSize = roleText.fontSize * 0.8f;
            }

            // 最初のRoleTextはそのまま残すか、Prefabから削除してください
            // (ここではPrefabの構造次第で調整が必要です)
        }
    }

    // TITLEタイプの行をセットアップするメソッド
    private void SetupTitleItem(GameObject item, CreditEntry entry)
    {
        TextMeshProUGUI titleText = item.GetComponent<TextMeshProUGUI>();
        if (titleText != null)
        {
            titleText.text = entry.title;
        }
    }

    // LINEBREAK（空白行）を生成するメソッド
    private GameObject CreateSpacer(float height)
    {
        GameObject spacer = new GameObject("Spacer");

        // Rect Transform の高さ設定
        RectTransform rt = spacer.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 0);

        // Layout Element で高さを固定する
        LayoutElement le = spacer.AddComponent<LayoutElement>();
        le.preferredHeight = height;

        return spacer;
    }
}