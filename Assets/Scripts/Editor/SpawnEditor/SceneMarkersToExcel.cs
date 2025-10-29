using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using OfficeOpenXml;
using UnityEngine.SceneManagement;

/// <summary>
/// シーン内の SpawnMarker データを Excel に書き出し、
/// ScriptableObject にも保存するエディタ拡張。
/// 処理フロー:
/// 1. シーン内の SpawnMarker を取得
/// 2. ID順にソート
/// 3. Excel にデータを書き込む
///    ├─ ヘッダー行作成
///    └─ データ行書き込み
/// 4. シートを Stage番号順に並び替え
/// 5. ScriptableObject を更新
/// 6. AssetDatabase.Refresh() でエディタに反映
/// </summary>
public class SceneMarkersToExcel : EditorWindow
{
    /// <summary>Excel ファイルの保存先パス</summary>
    string savePath = "Assets/Data/SpawnData.xlsx";

    /// <summary>ScriptableObject 保存フォルダ</summary>
    string scriptableFolder = "Assets/Resources/SpawnData/";

    /// <summary>メニューからウィンドウを開く</summary>
    [MenuItem("Tools/Scene Markers → Excel Export")]
    public static void ShowWindow()
    {
        GetWindow<SceneMarkersToExcel>("Scene Markers → Excel");
    }

    /// <summary>
    /// エディタウィンドウの GUI を描画
    /// - 保存パス入力欄
    /// - ScriptableObject フォルダ入力欄
    /// - 実行ボタン
    /// </summary>
    void OnGUI()
    {
        GUILayout.Label("Export Path (.xlsx)");
        savePath = EditorGUILayout.TextField(savePath);

        GUILayout.Label("ScriptableObject Folder");
        scriptableFolder = EditorGUILayout.TextField(scriptableFolder);

        if (GUILayout.Button("Export Scene Markers to Excel + Update SO"))
        {
            ExportAndUpdateSO();
        }
    }

    /// <summary>
    /// Excel出力＆ScriptableObject更新のメイン処理
    /// </summary>
    private void ExportAndUpdateSO()
    {
        // ---------------- 1. 現在のシーン取得 ----------------
        var scene = SceneManager.GetActiveScene();

        // ---------------- 2. SpawnMarker取得 ----------------
        // シーン内のすべての SpawnMarker を取得
        var markers = Object.FindObjectsByType<SpawnMarker>(FindObjectsSortMode.None);
        if (markers.Length == 0)
        {
            Debug.LogWarning("シーンに SpawnMarker が存在しません。");
            return; // マーカーがなければ処理終了
        }

        // ---------------- 3. ID順にソート ----------------
        // Excel でも ScriptableObject でも ID 順に並べるため
        System.Array.Sort(markers, (a, b) => a.id.CompareTo(b.id));

        // ---------------- 4. Excel 書き込み ----------------
        FileInfo file = new FileInfo(savePath);
        using (var package = new ExcelPackage(file))
        {
            // 既存シートを取得、なければ新規作成
            var sheet = package.Workbook.Worksheets[scene.name] ?? package.Workbook.Worksheets.Add(scene.name);
            sheet.Cells.Clear(); // 既存データをクリア

            // ヘッダー行作成
            sheet.Cells[1, 1].Value = "ID";        // Marker ID
            sheet.Cells[1, 2].Value = "Type";      // Marker タイプ
            sheet.Cells[1, 3].Value = "PrefabName"; // プレハブ名
            sheet.Cells[1, 4].Value = "PosX";      // X座標
            sheet.Cells[1, 5].Value = "PosY";      // Y座標
            sheet.Cells[1, 6].Value = "PosZ";      // Z座標

            // データ行を書き込み
            for (int i = 0; i < markers.Length; i++)
            {
                var m = markers[i];
                int row = i + 2; // ヘッダー下から開始

                sheet.Cells[row, 1].Value = m.id;
                sheet.Cells[row, 2].Value = m.type;
                sheet.Cells[row, 3].Value = m.prefabName;
                sheet.Cells[row, 4].Value = m.transform.position.x;
                sheet.Cells[row, 5].Value = m.transform.position.y;
                sheet.Cells[row, 6].Value = m.transform.position.z;
            }

            // 列幅自動調整
            sheet.Cells.AutoFitColumns();
            package.Save();

            // ---------------- 5. シート並び替え ----------------
            SortSheetsByStageNumber(savePath);
            package.Save();
        }

        // エディタに変更を反映
        AssetDatabase.Refresh();
        Debug.Log($"✅ Excel出力完了: {savePath} にシーン「{scene.name}」を書き出しました（{markers.Length}件）");

        // ---------------- 6. ScriptableObject更新 ----------------
        string path = Path.Combine(scriptableFolder, $"{scene.name}.asset");
        var dataSO = AssetDatabase.LoadAssetAtPath<SpawnDataSO>(path);

        // ScriptableObjectが存在しなければ新規作成
        if (dataSO == null)
        {
            dataSO = ScriptableObject.CreateInstance<SpawnDataSO>();
            if (!Directory.Exists(scriptableFolder)) Directory.CreateDirectory(scriptableFolder);
            AssetDatabase.CreateAsset(dataSO, path);
        }

        // Marker データを ScriptableObject に変換
        List<SpawnDataEntry> entries = new List<SpawnDataEntry>();
        foreach (var m in markers)
        {
            entries.Add(new SpawnDataEntry()
            {
                id = m.id,
                type = m.type,
                prefabName = m.prefabName,
                position = m.transform.position
            });
        }

        // ScriptableObject に反映
        dataSO.entries = entries.ToArray();

        // 変更をマークして保存
        EditorUtility.SetDirty(dataSO);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✅ ScriptableObject更新完了: {path} に {markers.Length}件のデータを保存");
    }

    /// <summary>
    /// Stage番号をシート名から抽出
    /// 例: "Stage12" → 12
    /// 番号が無ければ int.MaxValue を返す
    /// </summary>
    private int ExtractStageNumber(string sheetName)
    {
        var match = System.Text.RegularExpressions.Regex.Match(sheetName, @"\d+");
        if (match.Success && int.TryParse(match.Value, out int num))
            return num;
        return int.MaxValue; // 数字がなければ最後尾扱い
    }

    /// <summary>
    /// Excel のシートを Stage番号順に並び替える
    /// </summary>
    private void SortSheetsByStageNumber(string path)
    {
        FileInfo file = new FileInfo(path);

        using (var package = new ExcelPackage(file))
        {
            // 元のシートをリスト化
            var originalSheets = new List<ExcelWorksheet>();
            foreach (var sheet in package.Workbook.Worksheets)
                originalSheets.Add(sheet);

            // Stage番号でソート
            originalSheets.Sort((a, b) => ExtractStageNumber(a.Name).CompareTo(ExtractStageNumber(b.Name)));

            // 新規パッケージにシートを順番にコピー
            using (var newPackage = new ExcelPackage())
            {
                foreach (var sheet in originalSheets)
                    newPackage.Workbook.Worksheets.Add(sheet.Name, sheet);

                // 元ファイルを上書き
                newPackage.SaveAs(file);
            }
        }
    }
}
