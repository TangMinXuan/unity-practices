using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class PackageSOImporter : EditorWindow
{
    private string jsonFilePath = "";
    private PackageSO targetPackageSO;

    [MenuItem("Tools/Package Data Importer")]
    public static void ShowWindow()
    {
        GetWindow<PackageSOImporter>("Package Data Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Package Data Importer", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        // JSON文件路径选择
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("JSON File:", GUILayout.Width(80));
        jsonFilePath = EditorGUILayout.TextField(jsonFilePath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            jsonFilePath = EditorUtility.OpenFilePanel("Select JSON File", Application.dataPath, "json");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // 目标ScriptableObject选择
        targetPackageSO = (PackageSO)EditorGUILayout.ObjectField("Target PackageSO:", targetPackageSO, typeof(PackageSO), false);

        EditorGUILayout.Space();

        // 导入按钮
        GUI.enabled = !string.IsNullOrEmpty(jsonFilePath) && targetPackageSO != null;
        if (GUILayout.Button("Import from JSON"))
        {
            ImportFromJSON();
        }

        EditorGUILayout.Space();

        // 创建新的ScriptableObject按钮
        GUI.enabled = !string.IsNullOrEmpty(jsonFilePath);
        if (GUILayout.Button("Create New PackageSO from JSON"))
        {
            CreateNewPackageSOFromJSON();
        }
        GUI.enabled = true;
    }

    private void ImportFromJSON()
    {
        try
        {
            if (!File.Exists(jsonFilePath))
            {
                throw new System.Exception("JSON file does not exist!");
            }

            string jsonContent = File.ReadAllText(jsonFilePath);
            Debug.Log($"JSON Content: {jsonContent}");

            var itemDataWrapper = JsonUtility.FromJson<ItemDataListWrapper>(jsonContent);

            if (itemDataWrapper == null)
            {
                throw new System.Exception("Failed to parse JSON content!");
            }

            if (itemDataWrapper.items == null)
            {
                throw new System.Exception("Items array is null in JSON!");
            }

            // 使用SerializedObject来修改ScriptableObject
            SerializedObject serializedObject = new SerializedObject(targetPackageSO);
            SerializedProperty itemListProperty = serializedObject.FindProperty("itemList");

            if (itemListProperty == null)
            {
                throw new System.Exception("Cannot find 'itemList' property in PackageSO!");
            }

            itemListProperty.ClearArray();

            for (int i = 0; i < itemDataWrapper.items.Count; i++)
            {
                itemListProperty.InsertArrayElementAtIndex(i);
                SerializedProperty itemProperty = itemListProperty.GetArrayElementAtIndex(i);

                var itemData = itemDataWrapper.items[i];

                // 修复字段名匹配问题
                var idProp = itemProperty.FindPropertyRelative("id");
                var nameProp = itemProperty.FindPropertyRelative("name");
                var descProp = itemProperty.FindPropertyRelative("description");
                var iconPathProp = itemProperty.FindPropertyRelative("iconPath");
                var sceneTagProp = itemProperty.FindPropertyRelative("sceneTag");

                if (idProp != null) idProp.intValue = itemData.id;
                if (nameProp != null) nameProp.stringValue = itemData.name;
                if (descProp != null) descProp.stringValue = itemData.description;
                if (iconPathProp != null) iconPathProp.stringValue = itemData.iconPath;
                if (sceneTagProp != null) sceneTagProp.stringValue = itemData.sceneTag;

                Debug.Log($"Imported item {i}: ID={itemData.id}, Name={itemData.name}");
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(targetPackageSO);

            Debug.Log($"Successfully imported {itemDataWrapper.items.Count} items from JSON");
            EditorUtility.DisplayDialog("Success", $"Imported {itemDataWrapper.items.Count} items successfully!", "OK");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to import JSON: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
            EditorUtility.DisplayDialog("Error", $"Import failed: {e.Message}", "OK");
        }
    }

    private void CreateNewPackageSOFromJSON()
    {
        string savePath = EditorUtility.SaveFilePanelInProject(
            "Save PackageSO",
            "NewPackageDatabase",
            "asset",
            "Save the new PackageSO asset"
        );

        if (!string.IsNullOrEmpty(savePath))
        {
            PackageSO newPackageSO = CreateInstance<PackageSO>();
            AssetDatabase.CreateAsset(newPackageSO, savePath);
            AssetDatabase.SaveAssets();

            targetPackageSO = newPackageSO;
            ImportFromJSON();
        }
    }
}