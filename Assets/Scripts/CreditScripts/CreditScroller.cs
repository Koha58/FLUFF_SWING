using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class CreditScroller : MonoBehaviour
{
    public RectTransform content;
    public CreditDataSO creditData;

    public GameObject logoRowPrefab;
    public GameObject roleRowPrefab;
    public GameObject thankYouRowPrefab;
    public GameObject spacerRowPrefab;

    public float scrollSpeed = 80f;

    float totalHeight;
    bool scrolling;

    void Start()
    {
        BuildFromData();
        StartCoroutine(StartScroll());
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

    void CreateLogo(string spriteName)
    {
        var row = Instantiate(logoRowPrefab, content);
        row.GetComponentInChildren<Image>().sprite =
            Resources.Load<Sprite>(spriteName);

        AddHeight(row);
    }

    void CreateRole(string role, string names)
    {
        var row = Instantiate(roleRowPrefab, content);
        row.transform.Find("RoleText").GetComponent<TextMeshProUGUI>().text = role;
        row.transform.Find("NameText").GetComponent<TextMeshProUGUI>().text =
            names.Replace(",", "\n");

        AddHeight(row);
    }

    void CreateThankYou(string text)
    {
        var row = Instantiate(thankYouRowPrefab, content);
        row.GetComponentInChildren<TextMeshProUGUI>().text = text;

        AddHeight(row);
    }

    void CreateSpacer(float height)
    {
        var row = Instantiate(spacerRowPrefab, content);
        row.GetComponent<LayoutElement>().preferredHeight = height;

        AddHeight(row);
    }

    void AddHeight(GameObject row)
    {
        totalHeight += row.GetComponent<LayoutElement>().preferredHeight;
    }

    IEnumerator StartScroll()
    {
        yield return null;

        content.anchoredPosition =
            new Vector2(0, -totalHeight / 2f - Screen.height / 2f);

        scrolling = true;
    }

    void Update()
    {
        if (!scrolling) return;

        content.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
    }
}
