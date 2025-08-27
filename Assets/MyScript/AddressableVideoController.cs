using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.EventSystems;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableVideoController : MonoBehaviour, IPointerClickHandler
{
    [Header("Addressables")]
    public string videoKey = "demoVideo";

    [Header("UI")]
    public RawImage targetImage;   // 建议拖自己

    [Header("Options")]
    public bool loop = false;

    private VideoPlayer player;
    private RenderTexture rt;
    private AsyncOperationHandle<VideoClip> handle;
    private bool loaded, prepared;

    void Awake()
    {
        if (targetImage == null) targetImage = GetComponent<RawImage>();
        player = GetComponent<VideoPlayer>();
        if (player == null) player = gameObject.AddComponent<VideoPlayer>();

        player.playOnAwake = false;
        player.isLooping = loop;
        player.renderMode = VideoRenderMode.RenderTexture;

        // 确保有 EventSystem（用于接收点击）
        if (FindObjectOfType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!prepared) return;
        if (player.isPlaying) player.Pause(); else player.Play();
    }

    public void Open()  // 打开 Canvas 时调用：加载 + 准备
    {
        if (!loaded) StartCoroutine(LoadAndPrepare());
    }

    System.Collections.IEnumerator LoadAndPrepare()
    {
        var op = Addressables.LoadAssetAsync<VideoClip>(videoKey);
        yield return op;
        if (op.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"Load VideoClip failed: {videoKey}");
            yield break;
        }

        handle = op; loaded = true;

        var clip = op.Result;
        player.clip = clip;

        // 创建 RenderTexture 并接到 UI
        int w = (int)(clip.width  > 0 ? clip.width  : 1920);
        int h = (int)(clip.height > 0 ? clip.height : 1080);
        rt = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32);
        rt.Create();
        player.targetTexture = rt;
        if (targetImage) targetImage.texture = rt;

        player.Prepare();
        while (!player.isPrepared) yield return null;
        prepared = true;   // 等你点击后再播放
    }

    public void Close()  // 关闭 Canvas 时调用：停止 + 卸载
    {
        StopAllCoroutines();
        prepared = false;

        if (player != null)
        {
            player.Stop();
            player.targetTexture = null;
            player.clip = null;
        }
        if (targetImage != null) targetImage.texture = null;

        if (rt != null) { rt.Release(); Destroy(rt); rt = null; }

        if (loaded) { Addressables.Release(handle); loaded = false; }
    }

    void OnDisable() { Close(); } // 防止有人直接 SetActive(false) Canvas
}
