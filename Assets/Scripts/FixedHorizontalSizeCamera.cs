using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 锁定相机的“世界横向宽度”，
/// 以一个设计用的 16:9 分辨率为基准（比如 1920x1080）。
/// 不同屏幕比例时，通过调整 orthographicSize 来保持世界宽度一致。
/// </summary>
[RequireComponent(typeof(Camera))]
public class FixedHorizontalSizeCamera : MonoBehaviour
{
    [Header("设计用参数（在 16:9 下调好的值）")]
    public float designOrthoSize = 5f;         // 你现在在 16:9 下用的 orthographicSize
    public float designAspect = 16f / 9f;      // 设计时的参考比例

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        ApplyFixedWidth();
    }

    void OnValidate()
    {
        if (cam == null) cam = GetComponent<Camera>();
        ApplyFixedWidth();
    }

    void ApplyFixedWidth()
    {
        if (cam == null) return;

        // 当前屏幕真实宽高比
        float currentAspect = (float)Screen.width / Screen.height;

        // 设计下的“世界宽度” = designOrthoSize * 2 * designAspect
        // 要在当前屏幕上保持同样宽度：orthoSize = 设计Ortho * (设计Aspect / 当前Aspect)
        cam.orthographicSize = 17; //designOrthoSize * (designAspect / currentAspect);
    }

    void Update()
    {
        // 如果你允许运行时改分辨率，可每帧更新一次
        ApplyFixedWidth();
    }
}

