using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// プレイヤーのワイヤー操作に関する入力を管理するクラス。
/// Input Systemの"ConnectWire"（左クリック）と"CutWire"（右クリック）アクションを監視し、
/// イベントを通じて他クラスへ通知する。
/// </summary>
public class WireInputHandler : MonoBehaviour
{
    /// <summary>ワイヤー接続（左クリック）イベント</summary>
    public event Action OnLeftClick;

    /// <summary>ワイヤー切断（右クリック）イベント</summary>
    public event Action OnRightClick;

    // Input Systemのアクション参照（左クリック用）
    private InputAction leftClickAction;

    // Input Systemのアクション参照（右クリック用）
    private InputAction rightClickAction;

    /// <summary>
    /// 初期化処理。Input Systemからアクションを取得し、
    /// コールバック登録と有効化を行う。
    /// </summary>
    private void Awake()
    {
        // "ConnectWire"アクション（左クリック）をInput Systemから取得
        leftClickAction = InputSystem.actions.FindAction("ConnectWire");

        // "CutWire"アクション（右クリック）をInput Systemから取得
        rightClickAction = InputSystem.actions.FindAction("CutWire");

        // 左クリックアクションが取得できていればイベント登録と有効化
        if (leftClickAction != null)
        {
            // アクションが実行されたらOnLeftClickイベントを呼び出す（nullチェック付き）
            leftClickAction.performed += ctx => OnLeftClick?.Invoke();

            // アクションを有効化し、入力受付開始
            leftClickAction.Enable();
        }
        else
        {
            Debug.LogWarning("ConnectWire action not found in Input System.");
        }

        // 右クリックアクションが取得できていればイベント登録と有効化
        if (rightClickAction != null)
        {
            // アクションが実行されたらOnRightClickイベントを呼び出す（nullチェック付き）
            rightClickAction.performed += ctx => OnRightClick?.Invoke();

            // アクションを有効化し、入力受付開始
            rightClickAction.Enable();
        }
        else
        {
            Debug.LogWarning("CutWire action not found in Input System.");
        }
    }
}
