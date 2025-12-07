using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Collider2D))]
public class FakeTerminal : MonoBehaviour
{
    [Header("终端文本")]
    public TMP_Text textArea;

    [Header("显示设置")]
    public int maxVisibleLines = 30;      // 屏幕上最多显示多少行
    public int maxHistoryLines = 500;     // 内部最多保留多少行历史
    public string prompt = "> ";
    public string cursorChar = "_";

    [Header("默认启动文本（Inspector 里直接多行输入）")]
    [TextArea(3, 10)]
    public string defaultBootText;
    public bool prependPromptToDefault = true;

    [Header("当前是否有输入焦点（只读）")]
    public bool hasFocus = false;

    [Header("DIR 命令显示用标题前缀")]
    public string directoryTitlePrefix = " Directory of ";

    [Header("FORMAT 目标世界（Grid 根节点，可空）")]
    public Grid gridRoot;
    public float formatTileDelay = 0.02f;   // 每删一个 tile 等待的时间（秒）

    private readonly List<string> lines = new List<string>();  // 全部历史行
    private string currentLine = "";

    private float cursorTimer = 0f;
    private bool cursorVisible = true;

    private Collider2D col;
    private Camera cam;

    private bool isFormatting = false;

    void Awake()
    {
        cam = Camera.main;
        col = GetComponent<Collider2D>();

        if (textArea == null)
            textArea = GetComponentInChildren<TMP_Text>();

        if (gridRoot == null)
            gridRoot = FindObjectOfType<Grid>();

        ResetTerminal();
    }

    public void ResetTerminal()
    {
        lines.Clear();
        currentLine = "";

        if (!string.IsNullOrEmpty(defaultBootText))
        {
            string[] rawLines = defaultBootText.Split('\n');
            foreach (var raw in rawLines)
            {
                string l = raw.TrimEnd('\r');

                if (prependPromptToDefault && l.Length > 0)
                    lines.Add(prompt + l);
                else
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

        if (Input.GetMouseButtonDown(0))
        {
            UpdateFocusByClick();
        }

        if (!hasFocus)
        {
            Redraw();
            return;
        }

        HandleInput();
        Redraw();
    }

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
            if (c == '\b')
            {
                if (currentLine.Length > 0)
                    currentLine = currentLine.Substring(0, currentLine.Length - 1);
            }
            else if (c == '\n' || c == '\r')
            {
                CommitLine();
            }
            else if (!char.IsControl(c))
            {
                currentLine += c;
            }
        }
    }

    void CommitLine()
    {
        lines.Add(prompt + currentLine);
        if (lines.Count > maxHistoryLines)
            lines.RemoveAt(0);

        RunCommand(currentLine);

        currentLine = "";
    }

    void RunCommand(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return;

        string trimmed = input.Trim();
        string[] parts = trimmed.Split(' ');
        string cmd = parts[0].ToUpperInvariant();

        switch (cmd)
        {
            case "HELP":
                ExecuteHelp();
                break;
            case "DIR":
                ExecuteDir();
                break;
            case "CLS":
                ExecuteCls();
                break;
            case "FORMAT":
                ExecuteFormat();
                break;
            case "EXIT":
                ExecuteExit();
                break;
            default:
                PrintSystem("Unknown command: " + trimmed);
                PrintSystem("Type HELP for available commands.");
                break;
        }
    }

    void ExecuteHelp()
    {
        PrintSystem("Available commands:");
        PrintSystem("DIR       Displays the files and folders in the current directory.");
        PrintSystem("CLS       Clears the screen.");
        PrintSystem("FORMAT    Formats a drive or virtual world directory (use with caution).");
        PrintSystem("EXIT      Exits the console.");
    }

    // ====== 修改后的 DIR ======
    void ExecuteDir()
    {
        PrintSystem("");

        // 当前游戏所在目录（Editor：工程/Assets，Build：xxx_Data）
        string folder = Application.dataPath;
        PrintSystem(directoryTitlePrefix + folder);
        PrintSystem("");
    }

    void ExecuteCls()
    {
        lines.Clear();
        currentLine = "";
        Redraw();
    }

    void ExecuteFormat()
    {
        if (isFormatting)
        {
            PrintSystem("FORMAT is already in progress...");
            return;
        }

        PrintSystem("");
        PrintSystem("WARNING: This will erase the virtual world.");
        PrintSystem("Formatting...");
        PrintSystem("");

        StartCoroutine(FormatWorldRoutine());
    }

    System.Collections.IEnumerator FormatWorldRoutine()
    {
        isFormatting = true;

        Tilemap[] maps;
        if (gridRoot != null)
            maps = gridRoot.GetComponentsInChildren<Tilemap>();
        else
            maps = FindObjectsOfType<Tilemap>();

        foreach (var tm in maps)
        {
            if (tm == null) continue;

            BoundsInt bounds = tm.cellBounds;
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    var pos = new Vector3Int(x, y, 0);
                    if (tm.HasTile(pos))
                    {
                        tm.SetTile(pos, null);
                        yield return new WaitForSeconds(formatTileDelay);
                    }
                }
            }
        }

        PrintSystem("Format complete.");
        isFormatting = false;
    }

    // ====== 修改后的 EXIT ======
    void ExecuteExit()
    {
        PrintSystem("Exiting ScrollScape console...");
        Redraw();

        // 只退出游戏本身；在 Editor 里这行不会停止 Play，只是无效果
        Application.Quit();
    }

    void Redraw()
    {
        if (textArea == null) return;

        StringBuilder sb = new StringBuilder();

        int start = Mathf.Max(0, lines.Count - maxVisibleLines);
        for (int i = start; i < lines.Count; i++)
        {
            sb.AppendLine(lines[i]);
        }

        sb.Append(prompt);
        sb.Append(currentLine);

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
        if (lines.Count > maxHistoryLines)
            lines.RemoveAt(0);
        Redraw();
    }
}
