using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class FakeTerminal : MonoBehaviour
{
    [Header("终端文本")]
    public TMP_Text textArea;

    [Header("显示设置")]
    public int maxLines = 50;
    public string prompt = "> ";
    public string cursorChar = "_";      // 光标符号

    [Header("当前是否有输入焦点（只读）")]
    public bool hasFocus = false;        // 必须点击窗口才输入

    private readonly List<string> lines = new List<string>();
    private string currentLine = "";

    private float cursorTimer = 0f;
    private bool cursorVisible = true;

    private Collider2D col;
    private Camera cam;

    void Awake()
    {
        cam = Camera.main;
        col = GetComponent<Collider2D>();

        if (textArea == null)
            textArea = GetComponentInChildren<TMP_Text>();

        ResetTerminal();
    }

    /// <summary>
    /// 清空终端
    /// </summary>
    public void ResetTerminal()
    {
        lines.Clear();
        currentLine = "";
        Redraw();
    }

    void Update()
    {
        HandleCursorBlink();

        // ① 鼠标左键点击：决定有没有焦点
        if (Input.GetMouseButtonDown(0))
        {
            UpdateFocusByClick();
        }

        // ② 没有焦点就不接收键盘输入，但光标可以继续闪烁
        if (!hasFocus)
        {
            Redraw();
            return;
        }

        // ③ 有焦点时才响应键盘（ESC 不再处理）
        HandleInput();
        Redraw();
    }

    /// <summary>
    /// 根据本次鼠标点击位置判断：点击在窗口内 → hasFocus = true；否则 false
    /// </summary>
    void UpdateFocusByClick()
    {
        if (cam == null || col == null)
        {
            hasFocus = false;
            return;
        }

        Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);

        // 用 RaycastAll 检测所有命中的 Collider2D
        RaycastHit2D[] hits = Physics2D.RaycastAll(mouseWorld, Vector2.zero);

        bool hitSelf = false;
        foreach (var hit in hits)
        {
            if (hit.collider == col)
            {
                hitSelf = true;
                break;
            }
        }

        hasFocus = hitSelf;
    }

    void HandleInput()
    {
        foreach (char c in Input.inputString)
        {
            if (c == '\b') // Backspace
            {
                if (currentLine.Length > 0)
                    currentLine = currentLine.Substring(0, currentLine.Length - 1);
            }
            else if (c == '\n' || c == '\r') // Enter
            {
                CommitLine();
            }
            else if (!char.IsControl(c)) // 普通字符
            {
                currentLine += c;
            }
        }

        // ? 不再处理 ESC
        // if (Input.GetKeyDown(KeyCode.Escape)) { hasFocus = false; }
    }

    void CommitLine()
    {
        lines.Add(prompt + currentLine);

        if (lines.Count > maxLines)
            lines.RemoveAt(0);

        // 这里以后可以扩展命令解析：
        // RunCommand(currentLine);

        currentLine = "";
    }

    void Redraw()
    {
        if (textArea == null) return;

        StringBuilder sb = new StringBuilder();

        // 历史行
        foreach (string line in lines)
            sb.AppendLine(line);

        // 当前行
        sb.Append(prompt);
        sb.Append(currentLine);

        // 光标只在有焦点时显示
        if (hasFocus && cursorVisible)
            sb.Append(cursorChar);

        textArea.text = sb.ToString();
    }

    void HandleCursorBlink()
    {
        cursorTimer += Time.deltaTime;
        if (cursorTimer >= 0.5f)
        {
            cursorVisible = !cursorVisible;
            cursorTimer = 0f;
        }
    }

    public void PrintSystem(string msg)
    {
        lines.Add(msg);
        if (lines.Count > maxLines)
            lines.RemoveAt(0);
        Redraw();
    }
}
