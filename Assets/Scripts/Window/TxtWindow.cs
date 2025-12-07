using System.Collections;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class TxtWindow : MonoBehaviour
{
    [Header("文本显示")]
    public TMP_Text textArea;

    [Header("显示设置")]
    public int maxVisibleLines = 11;      // 屏幕上最多显示多少行
    public int maxHistoryLines = 500;     // 内部最多保留多少行历史
    public string cursorChar = "_";       // 光标字符

    [Header("默认启动文本（Inspector 里直接多行输入）")]
    [TextArea(3, 10)]
    public string defaultBootText;        // 记事本默认内容（可空）

    [Header("当前是否有输入焦点（只读）")]
    public bool hasFocus = false;         // 必须点击窗口才输入

    private readonly List<string> lines = new List<string>();  // 历史行
    private string currentLine = "";                           // 正在编辑的这一行

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

        ResetWindow();
    }

    /// <summary>
    /// 清空并写入默认文本
    /// </summary>
    public void ResetWindow()
    {
        lines.Clear();
        currentLine = "";

        if (!string.IsNullOrEmpty(defaultBootText))
        {
            string[] rawLines = defaultBootText.Split('\n');
            foreach (var raw in rawLines)
            {
                string l = raw.TrimEnd('\r'); // 去掉 Windows 的 \r
                lines.Add(l);

                if (lines.Count > maxHistoryLines)
                    lines.RemoveAt(0);
            }
        }

        Redraw();
    }

    void Update()
    {
        HandleCursorBlink();

        // 鼠标点击 -> 决定有没有焦点
        if (Input.GetMouseButtonDown(0))
        {
            UpdateFocusByClick();
        }

        // 没焦点只刷新光标，不吃键盘
        if (!hasFocus)
        {
            Redraw();
            return;
        }

        // 有焦点才接收输入
        HandleInput();
        Redraw();
    }

    /// <summary>
    /// 点击是否在窗口 Collider 内，决定 hasFocus
    /// </summary>
    void UpdateFocusByClick()
    {
        if (cam == null || col == null)
        {
            hasFocus = false;
            return;
        }

        Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
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
            else if (c == '\n' || c == '\r') // Enter 换行
            {
                CommitLine();
            }
            else if (!char.IsControl(c)) // 普通可见字符
            {
                currentLine += c;
            }
        }
    }

    /// <summary>
    /// 当前编辑行回车 -> 入历史，清空 currentLine
    /// </summary>
    void CommitLine()
    {
        lines.Add(currentLine);
        if (lines.Count > maxHistoryLines)
            lines.RemoveAt(0);

        currentLine = "";
    }

    void Redraw()
    {
        if (textArea == null) return;

        // Step 1: 先构造完整文本给 TMP 处理，让它自己软换行
        StringBuilder full = new StringBuilder();

        foreach (var l in lines)
            full.AppendLine(l);

        string current = hasFocus && cursorVisible ? currentLine + cursorChar : currentLine;
        full.Append(current);

        textArea.text = full.ToString();
        textArea.ForceMeshUpdate();

        // Step 2: 获取可见“视觉行”数量
        TMP_TextInfo info = textArea.textInfo;
        int visualLines = info.lineCount;

        // Step 3: 如果视觉行数不超过允许数量 => 直接显示
        if (visualLines <= maxVisibleLines)
            return;

        // Step 4: 按“视觉行”裁剪底部 maxVisibleLines
        int cut = visualLines - maxVisibleLines;

        int startChar = info.lineInfo[cut].firstCharacterIndex;
        string finalText = full.ToString().Substring(startChar);

        // Step 5: 显示裁剪后的文本
        textArea.text = finalText;
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

    /// <summary>
    /// 其他系统想往记事本里打印一行也可以用
    /// </summary>
    public void PrintLine(string msg)
    {
        lines.Add(msg);
        if (lines.Count > maxHistoryLines)
            lines.RemoveAt(0);
        Redraw();
    }
}
