using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using System.Threading.Tasks;

[System.Serializable]
public class ItemData
{
    public int id;
    public string name;
    public string description;
    public string iconPath; // Addressable
    public string sceneTag;
}

// 用于JSON反序列化的包装器类
[System.Serializable]
public class ItemDataListWrapper
{
    public List<ItemData> items;
}


[CreateAssetMenu(fileName = "New Package Database", menuName = "ScriptableObjects/Package Database")]
public class PackageSO : ScriptableObject
{
    [Header("Item Database")]
    [SerializeField] private List<ItemData> itemList = new List<ItemData>();

    // 分层缓存：只缓存当前场景需要的数据
    private Dictionary<int, ItemData> activeItemDatabase = new Dictionary<int, ItemData>();
    private Dictionary<string, AsyncOperationHandle<Sprite>> loadedSprites = new Dictionary<string, AsyncOperationHandle<Sprite>>();
    private string currentSceneTag = "";

    /// <summary>
    /// 根据场景标签加载对应的物品数据
    /// </summary>
    public async Task LoadSceneItems(string sceneTag)
    {
        // 卸载之前场景的数据
        if (!string.IsNullOrEmpty(currentSceneTag))
        {
            UnloadSceneItems();
        }

        currentSceneTag = sceneTag;
        activeItemDatabase.Clear();

        // 只加载当前场景需要的物品
        foreach (var item in itemList)
        {
            if (item.sceneTag == sceneTag || item.sceneTag == "Common") // Common表示通用物品
            {
                activeItemDatabase.Add(item.id, item);

                // 预加载图标（可选）
                if (!string.IsNullOrEmpty(item.iconPath))
                {
                    await LoadSpriteAsync(item.iconPath);
                }
            }
        }

        Debug.Log($"已加载场景 {sceneTag} 的 {activeItemDatabase.Count} 个物品");
    }

    /// <summary>
    /// 卸载当前场景的数据
    /// </summary>
    public void UnloadSceneItems()
    {
        // 释放Addressable资源
        foreach (var handle in loadedSprites.Values)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }

        loadedSprites.Clear();
        activeItemDatabase.Clear();
        currentSceneTag = "";

        Debug.Log("已卸载场景物品数据");
    }

    /// <summary>
    /// 异步加载图标
    /// </summary>
    private async Task<Sprite> LoadSpriteAsync(string addressableKey)
    {
        if (loadedSprites.ContainsKey(addressableKey))
        {
            return loadedSprites[addressableKey].Result;
        }

        var handle = Addressables.LoadAssetAsync<Sprite>(addressableKey);
        loadedSprites.Add(addressableKey, handle);

        await handle.Task;
        return handle.Result;
    }

    /// <summary>
    /// 获取物品数据（只从当前场景缓存中查找）
    /// </summary>
    public ItemData GetItemData(int itemId)
    {
        return activeItemDatabase.TryGetValue(itemId, out ItemData item) ? item : null;
    }

    /// <summary>
    /// 异步获取物品图标
    /// </summary>
    public async Task<Sprite> GetItemIconAsync(int itemId)
    {
        var itemData = GetItemData(itemId);
        if (itemData == null || string.IsNullOrEmpty(itemData.iconPath))
            return null;

        return await LoadSpriteAsync(itemData.iconPath);
    }

    /// <summary>
    /// 获取当前场景的所有物品
    /// </summary>
    public IReadOnlyDictionary<int, ItemData> GetActiveItems()
    {
        return activeItemDatabase;
    }
}
