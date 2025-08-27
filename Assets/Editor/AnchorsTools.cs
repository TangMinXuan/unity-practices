using UnityEngine;
using UnityEditor;

public static class AnchorsTools
{
    [MenuItem("Tools/UI/Anchors → Match Current Rect (to percent) %#a")]
    static void AnchorsToCurrentRect()
    {
        foreach (var obj in Selection.transforms)
        {
            var t = obj as RectTransform;
            if (!t) continue;
            var p = t.parent as RectTransform;
            if (!p) continue;

            Undo.RecordObject(t, "Anchors → Match Current Rect");

            // 用父本地坐标更稳（兼容父有旋转/缩放）
            Vector3[] child = new Vector3[4];
            Vector3[] parent = new Vector3[4];
            t.GetWorldCorners(child);
            p.GetWorldCorners(parent);

            // 转到父的本地坐标
            for (int i = 0; i < 4; i++)
            {
                child[i] = p.InverseTransformPoint(child[i]);
                parent[i] = p.InverseTransformPoint(parent[i]);
            }

            var pRect = p.rect; // 父的本地矩形
            // 子左下角 = child[0]，子右上角 = child[2]（RectTransform 的约定）
            float minX = Mathf.InverseLerp(pRect.xMin, pRect.xMax, child[0].x);
            float maxX = Mathf.InverseLerp(pRect.xMin, pRect.xMax, child[2].x);
            float minY = Mathf.InverseLerp(pRect.yMin, pRect.yMax, child[0].y);
            float maxY = Mathf.InverseLerp(pRect.yMin, pRect.yMax, child[2].y);

            t.anchorMin = new Vector2(minX, minY);
            t.anchorMax = new Vector2(maxX, maxY);
            t.offsetMin = Vector2.zero; // Left/Bottom = 0
            t.offsetMax = Vector2.zero; // Right/Top = 0
        }
    }

    [MenuItem("Tools/UI/Anchors → Reset Offsets (stick to anchors) %#s")]
    static void ResetOffsetsToZero()
    {
        foreach (var obj in Selection.transforms)
        {
            var t = obj as RectTransform;
            if (!t) continue;
            Undo.RecordObject(t, "Anchors → Reset Offsets");
            t.offsetMin = Vector2.zero;
            t.offsetMax = Vector2.zero;
        }
    }
}