using UnityEngine;

// Mantém a arena em 600 × 480, inclusive quando a janela muda de tamanho.
[RequireComponent(typeof(Camera))]
public class CameraFrame : MonoBehaviour
{
    void Start() { UpdateFrame(); }
    void Update() { UpdateFrame(); }
    void UpdateFrame()
    {
        float ratio = ((float)Screen.width / Mathf.Max(1, Screen.height)) / (600f / 480f);
        GetComponent<Camera>().rect = ratio < 1
            ? new Rect(0, (1 - ratio) / 2, 1, ratio)
            : new Rect((1 - 1 / ratio) / 2, 0, 1 / ratio, 1);
    }
}
