using UnityEngine;

/// <summary>
/// 固定相机垂直可视范围（vertical FOV）。
/// 这样无论屏幕比例如何，相机看到的内容都保持为设计时的 16:9 世界。
/// </summary>
[RequireComponent(typeof(Camera))]
public class FixedHorizontalSizeCamera : MonoBehaviour
{
    public float designOrthoSize = 5f; // 你设计时在 16:9 下的值

    private Camera cam;

    void Update() { }
    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographicSize = designOrthoSize;
    }
}
