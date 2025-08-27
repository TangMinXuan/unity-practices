using UnityEngine;
using LiteDB;
using System.Collections.Generic;
using System.Linq;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; }

    private LiteDatabase database;

    void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 场景切换时不销毁
            InitializeDatabase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeDatabase()
    {
        var path = System.IO.Path.Combine(Application.persistentDataPath, "game.db");
        using var db = new LiteDatabase($"Filename={path};Password=secret");
        var col = db.GetCollection<Player>("players");
        col.EnsureIndex(x => x.Id, true);
        col.Upsert(new Player { Id = 1, Name = "Hero", Level = 5 });
        Debug.Log("LiteDB OK: " + col.Count());
    }

    void OnDestroy()
    {
        database?.Dispose();
    }

    class Player
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
    }
}