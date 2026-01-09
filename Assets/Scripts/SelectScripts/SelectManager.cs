using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;


// ステージセレクトシーンで使用するUI管理クラス
// ・ステージ選択
// ・各種パネルの表示/非表示
// ・SE再生
// ・連打によるSE多重再生・遷移多重実行の防止
// を担当する

public class SelectManager : MonoBehaviour
{
    #region === Inspector ===

    // =========================
    // Panels
    // =========================

    [Header("音量設定パネル")]
    [SerializeField] private GameObject setPanel;

    [Header("データリセットパネル")]
    [SerializeField] private GameObject resetPanel;

    // =========================
    // Audio
    // =========================

    [Header("クリック音設定")]

    [Tooltip("ステージ選択・設定ボタンを押したときに鳴らすSE")]
    public AudioClip onClickSE;

    [Tooltip("ホーム・閉じるボタンを押したときに鳴らすSE")]
    public AudioClip offClickSE;

    [Tooltip("データリセットボタンを押したときに鳴らすSE")]
    public AudioClip resetClickSE;

    [Tooltip("ステージのロックを解除するときに鳴らすSE")]
    public AudioClip unlockSE;

    [Tooltip("接続するAudioMixerのSEグループ（※AudioManager側で使う想定）")]
    public AudioMixerGroup seMixerGroup;

    // =========================
    // Stage Data
    // =========================

    /// <summary>
    /// 次に遷移するステージ名（SE再生後に使う）
    /// </summary>
    private string nextStageName;

    [Header("各ステージのロックオブジェクト")]
    [Tooltip("各ステージボタンのLock（子オブジェクト）を順番に設定")]
    [SerializeField] private GameObject[] stageLocks;

    #endregion

    #region === 連打対策 ===

    /// <summary>
    /// シーン遷移を予約済みかどうか。
    /// true になった時点で、以降の入力はすべて無視する。
    ///
    /// 目的：
    /// ・SE多重再生防止
    /// ・Invokeの多重予約防止
    /// ・TransitionManagerの多重呼び出し防止
    /// </summary>
    private bool _isSceneMoveReserved = false;

    #endregion

    #region === デバッグ設定 ===

    [Header("▼ デバッグ設定")]
    [SerializeField]
    private bool enableDebugInput = false;

    [Header("▼ デバッグUI")]
    [SerializeField] private GameObject debugUIRoot;

    #endregion

    #region === セーブデータ管理（バージョン管理） ===

    /// <summary>
    /// セーブデータのバージョン管理用キー。
    /// 初回起動判定や、セーブ仕様変更時の初期化に使用する。
    /// </summary>
    private const string SaveVersionKey = "SAVE_VERSION";

    /// <summary>
    /// 現在のセーブデータ仕様のバージョン。
    /// 値を変更すると、古いセーブデータは「初回起動扱い」となり、
    /// ステージ解放状態などを安全に初期化できる。
    /// </summary>
    private const int CurrentSaveVersion = 1;

    #endregion


    #region === 初期化 ===

    private void Start()
    {
        // 初回起動 or セーブ仕様更新時だけ初期化
        if (!PlayerPrefs.HasKey(SaveVersionKey) || PlayerPrefs.GetInt(SaveVersionKey) != CurrentSaveVersion)
        {
            PlayerPrefs.SetInt("ClearedStage", 0);
            PlayerPrefs.SetInt("LastUnlockedStage", -1);
            PlayerPrefs.SetInt("UnlockedMaxStage", 1);

            PlayerPrefs.SetInt(SaveVersionKey, CurrentSaveVersion);
            PlayerPrefs.Save();
        }

        // 起動時はパネルを閉じた状態にする（誤表示防止）
        if (setPanel != null) setPanel.SetActive(false);

        // デバッグ設定によって表示するUI
        if (debugUIRoot != null)
        {
            debugUIRoot.SetActive(enableDebugInput);
            if (resetPanel != null) resetPanel.SetActive(false);
        }

        // ステージロック状態を反映
        UpdateStageLocks();

        // 連打対策フラグ初期化（念のため）
        _isSceneMoveReserved = false;

        // もし前回の予約が残っていた場合に備えてキャンセル
        CancelInvoke(nameof(LoadNextScene));
    }

    #endregion

    #region === ステージロック管理 ===

    /// <summary>
    /// PlayerPrefsの状態に応じてステージロック表示を更新する
    /// </summary>
    private void UpdateStageLocks()
    {
        // 前回「新規解放演出を出したステージ番号」（無ければ-1）
        int lastUnlockedStage = PlayerPrefs.GetInt("LastUnlockedStage", -1);

        // stageLocks 配列を順に見て、表示を更新
        for (int i = 0; i < stageLocks.Length; i++)
        {
            // 該当ロックオブジェクト（無ければスキップ）
            GameObject lockObj = stageLocks[i];
            if (lockObj == null) continue;

            // ステージ番号は配列 i(0始まり) + 1
            int stageNumber = i + 1;

            // --------------------------
            // 1) すでに解放済みのステージ
            // --------------------------
            // lastUnlockedStage より前は「演出対象よりも前＝既に解放済み」とみなし、ロック表示を消す
            if (stageNumber < lastUnlockedStage)
            {
                lockObj.SetActive(false);
                continue;
            }

            // --------------------------
            // 2) 今回新しく解放されたステージ（演出あり）
            // --------------------------
            if (stageNumber == lastUnlockedStage)
            {
                // いったんロックを表示して、解除アニメを再生する
                lockObj.SetActive(true);

                // ロック解除アニメーション用コンポーネント取得
                var anim = lockObj.GetComponent<LockOpen>();

                if (anim != null)
                {
                    // アニメ再生後にSE＆ロック非表示
                    anim.PlayUnlockAnimation(() =>
                    {
                        // 解放SE
                        if (AudioManager.Instance != null)
                            AudioManager.Instance.PlaySE(unlockSE);

                        // ロックを消す
                        lockObj.SetActive(false);
                    });
                }
                else
                {
                    // アニメが無いなら即ロックを消す
                    lockObj.SetActive(false);
                }
                continue;
            }

            // --------------------------
            // 3) 未解放ステージ
            // --------------------------
            // ここは通常のロック判定：UnlockedMaxStage 以下なら解放済み
            int unlockedMax = PlayerPrefs.GetInt("UnlockedMaxStage", 1);
            bool isUnlocked = stageNumber <= unlockedMax;

            // 解放済み→ロック非表示 / 未解放→ロック表示
            lockObj.SetActive(!isUnlocked);
        }

        // 「今回の新規解放演出」は一度見せたらリセットして次回に持ち越さない
        if (lastUnlockedStage != -1)
        {
            PlayerPrefs.SetInt("LastUnlockedStage", -1);
            PlayerPrefs.Save();
        }
    }

    #endregion

    #region === ステージ遷移 ===

    /// <summary>
    /// ステージボタンを押したときに呼ばれる
    /// </summary>
    public void SelectStage(string stageName)
    {
        // --------------------------
        // 連打対策：すでに遷移予約済みなら何もしない
        // --------------------------
        // ここで return することで
        // ・SEの多重再生
        // ・Invokeの多重予約
        // を根本的に防ぐ
        if (_isSceneMoveReserved) return;

        // --------------------------
        // ロックチェック：ロック中なら遷移しない
        // --------------------------
        int index = GetStageIndex(stageName);

        // index が妥当 かつ ロックオブジェクトが active ならロック中
        if (index >= 0 && stageLocks[index] != null && stageLocks[index].activeSelf)
        {
            // 失敗系のSE（押せたが遷移しない感を出す）
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySE(offClickSE);

            Debug.Log($"{stageName} はロック中です。");
            return;
        }

        // --------------------------
        // 遷移先名を保持（SE再生後に使う）
        // --------------------------
        nextStageName = stageName;

        // --------------------------
        // ここで「遷移予約済み」にする（この行が連打の肝）
        // --------------------------
        _isSceneMoveReserved = true;

        // --------------------------
        // 決定SE（最初の1回のみ）
        // --------------------------
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(onClickSE);

        // --------------------------
        // 多重Invoke対策：念のため前回予約があれば消す
        // --------------------------
        CancelInvoke(nameof(LoadNextScene));

        // --------------------------
        // SEの長さ分待ってからシーン遷移
        // --------------------------
        float delay = onClickSE != null ? onClickSE.length : 0.1f;
        Invoke(nameof(LoadNextScene), delay);
    }

    /// <summary>
    /// 実際のシーン遷移処理（Invokeから呼ばれる）
    /// </summary>
    private void LoadNextScene()
    {
        // TransitionManager が無ければ最低限のフォールバックで遷移する
        if (TransitionManager.Instance == null)
        {
            SceneManager.LoadScene(nextStageName);
            return;
        }

        // Try版を使うことで、TransitionManager側でも多重遷移を防止できる
        TransitionManager.Instance.TryPlayTransitionAndLoadScene(nextStageName);
    }

    /// <summary>
    /// "Stage1" → 0, "Stage2" → 1 ... のように配列Indexに変換する
    /// （命名規則が崩れると -1 を返す）
    /// </summary>
    private int GetStageIndex(string stageName)
    {
        // "Stage" で始まる場合のみ数字を取り出す
        if (stageName.StartsWith("Stage") &&
            int.TryParse(stageName.Replace("Stage", ""), out int num))
        {
            // 配列は0始まりなので -1
            return num - 1;
        }
        return -1;
    }

    /// <summary>
    /// タイトルに戻る（ホームボタン等）
    /// </summary>
    public void TitleBack(string stageName)
    {
        // 連打対策：すでに予約済みなら無視
        if (_isSceneMoveReserved) return;

        // 遷移先を保持
        nextStageName = stageName;

        // 予約済みにして以降の入力を遮断
        _isSceneMoveReserved = true;

        // 戻るSE（最初の1回のみ）
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(offClickSE);

        // 多重Invoke対策
        CancelInvoke(nameof(LoadNextScene));

        // SEの長さ分待ってから遷移
        float delay = offClickSE != null ? offClickSE.length : 0.1f;
        Invoke(nameof(LoadNextScene), delay);
    }

    #endregion

    #region === パネル表示制御 ===

    public void OnSetPanel()
    {
        // 参照が無ければ何もしない
        if (setPanel == null) return;

        // パネルを表示
        setPanel.SetActive(true);

        // 決定SE
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(onClickSE);
    }

    public void OnResetPanel()
    {
        if (resetPanel == null) return;

        // リセット確認パネルを表示
        resetPanel.SetActive(true);

        // リセット系SE
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(resetClickSE);
    }

    public void OffSetPanel()
    {
        if (setPanel == null) return;

        // 設定パネルを閉じる
        setPanel.SetActive(false);

        // 戻るSE
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(offClickSE);
    }

    public void OffResetPanel()
    {
        if (resetPanel == null) return;

        // リセットパネルを閉じる
        resetPanel.SetActive(false);

        // 戻るSE
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(offClickSE);
    }

    #endregion

    #region デバッグ用アンロック・データリセット

    private void Update()
    {
        if (!enableDebugInput) return;

        CheckDebugUnlockInput();
    }

    /// <summary>
    /// デバッグ用（全解除 / リセット）
    /// Ctrl + Shift + U → 全解除
    /// Ctrl + Shift + R → リセット
    /// </summary>
    private void CheckDebugUnlockInput()
    {
        // --- 全ステージ解放 ---
        if (Input.GetKey(KeyCode.LeftControl) &&
            Input.GetKey(KeyCode.LeftShift) &&
            Input.GetKeyDown(KeyCode.U))
        {
            DebugUnlockAllStages();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySE(onClickSE);
        }

        // --- ロック初期化（ステージ1だけ解放）---
        if (Input.GetKey(KeyCode.LeftControl) &&
            Input.GetKey(KeyCode.LeftShift) &&
            Input.GetKeyDown(KeyCode.R))
        {
            DebugResetStages();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySE(offClickSE);
        }
    }

    private void DebugUnlockAllStages()
    {
        int totalStages = stageLocks.Length;

        PlayerPrefs.SetInt("ClearedStage", totalStages);
        PlayerPrefs.SetInt("UnlockedMaxStage", totalStages);
        PlayerPrefs.Save();

        Debug.Log("<b><color=orange>【デバッグ】Ctrl+Shift+U→全ステージを解放しました</color></b>");

        UpdateStageLocks();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(resetClickSE);
    }

    private void DebugResetStages()
    {
        PlayerPrefs.SetInt("ClearedStage", 0);
        PlayerPrefs.SetInt("LastUnlockedStage", -1);
        PlayerPrefs.SetInt("UnlockedMaxStage", 1);
        PlayerPrefs.Save();

        Debug.Log("<b><color=orange>【デバッグ】Ctrl+Shift+R→ステージロックを初期状態に戻しました（ステージ1のみ解放）</color></b>");

        UpdateStageLocks();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(resetClickSE);
    }

    /// <summary>
    /// データリセット（ステージ1のみ解放の初期状態）
    /// UIボタンから呼び出す用
    /// </summary>
    public void ResetStageData()
    {
        PlayerPrefs.SetInt("ClearedStage", 0);
        PlayerPrefs.SetInt("LastUnlockedStage", -1);
        PlayerPrefs.SetInt("UnlockedMaxStage", 1);
        PlayerPrefs.Save();

        Debug.Log("<b><color=orange>ステージデータをリセットしました（ステージ1のみ解放）</color></b>");

        UpdateStageLocks();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE(resetClickSE);
    }

    #endregion
}
