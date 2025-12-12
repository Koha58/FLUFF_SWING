using UnityEngine;

[System.Serializable]
public class CreditEntry
{
    // A列: Type (ROLE, TITLE, LINEBREAK, LOGO, THANKYOU などの種類)
    public string type;

    // B列: Title / Value (役職名、グループタイトル、空白の高さなど)
    public string title;

    // C列: Name (氏名、カンマ区切りで複数名に対応)
    public string name;

    // D列: SpecialData (予備データ、フォントサイズや色など)
    public string specialData;
}
