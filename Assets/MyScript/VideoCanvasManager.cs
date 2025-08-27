using UnityEngine;

public class VideoCanvasManager : MonoBehaviour
{
    public Canvas videoCanvas;                       // 拖 VideoCanvas
    public AddressableVideoController controller;    // 拖 VideoSurface 上的脚本
    public KeyCode toggleKey = KeyCode.Tab;

    bool open;

    void Start()
    {
        if (videoCanvas) videoCanvas.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey)) Toggle();
    }

    public void Toggle()
    {
        open = !open;
        if (open)
        {
            if (videoCanvas) videoCanvas.gameObject.SetActive(true);
            if (controller) controller.Open();
        }
        else
        {
            if (controller) controller.Close();
            if (videoCanvas) videoCanvas.gameObject.SetActive(false);
        }
    }
}