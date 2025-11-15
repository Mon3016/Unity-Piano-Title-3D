using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;

public class FixUIAndLanes : EditorWindow
{
    [MenuItem("Tools/Fix UI and Extend Lanes")]
    public static void FixAll()
    {
        // 1. Tìm MenuCanvas (Canvas riêng cho menu)
        Canvas menuCanvas = null;
        Canvas[] allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();
        foreach (Canvas c in allCanvases)
        {
            if (c.name == "MenuCanvas")
            {
                menuCanvas = c;
                break;
            }
        }
        
        // Nếu không có MenuCanvas, tìm Canvas đầu tiên (fallback)
        if (menuCanvas == null)
        {
            menuCanvas = FindFirstObjectByType<Canvas>();
            Debug.LogWarning("⚠️ MenuCanvas not found, using first Canvas found");
        }
        
        if (menuCanvas != null)
        {
            Debug.Log($"📋 Current Menu Canvas settings ({menuCanvas.name}):");
            Debug.Log($"   - Render Mode: {menuCanvas.renderMode}");
            Debug.Log($"   - Override Sorting: {menuCanvas.overrideSorting}");
            Debug.Log($"   - Sorting Order: {menuCanvas.sortingOrder}");
            Debug.Log($"   - Plane Distance: {menuCanvas.planeDistance}");
            
            // SỬ DỤNG Screen Space Overlay để menu LUÔN render trên cùng (không bị 3D objects che)
            if (menuCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                Debug.Log("🔄 Changing to Screen Space Overlay (menu ALWAYS on top, not covered by 3D objects)...");
                menuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            
            // BẮT BUỘC override sorting để sortingOrder có hiệu lực
            menuCanvas.overrideSorting = true;
            menuCanvas.sortingOrder = 32767; // Max sorting order để menu LUÔN render trên cùng
            Debug.Log($"✅ {menuCanvas.name} overrideSorting = true, sortingOrder = 32767 (MAX)");
            Debug.Log("   → Menu will ALWAYS render on top (not covered by 3D objects or game UI)");
            EditorUtility.SetDirty(menuCanvas.gameObject);
        }
        
        // 2. Make menu panels VERY transparent so lanes are visible behind
        // Tìm và sửa TẤT CẢ panel (kể cả duplicate) để tránh bị đè lên nhau
        string[] panelNames = { "MainMenuPanel", "PauseMenuPanel", "SongSelectionPanel", "GameOverPanel" };
        foreach (string panelName in panelNames)
        {
            // Tìm tất cả panel có cùng tên
            List<GameObject> panels = new List<GameObject>();
            
            // Tìm trong MenuCanvas children (recursive)
            if (menuCanvas != null)
            {
                Transform canvasTransform = menuCanvas.transform;
                FindAllChildrenRecursive(canvasTransform, panelName, panels);
            }
            
            // Nếu không tìm thấy, thử GameObject.Find (chỉ tìm 1)
            if (panels.Count == 0)
            {
                GameObject found = GameObject.Find(panelName);
                if (found != null)
                {
                    panels.Add(found);
                }
            }
            
            if (panels.Count > 0)
            {
                if (panels.Count > 1)
                {
                    Debug.LogWarning($"⚠️ Found {panels.Count} duplicate {panelName} panels! Fixing all...");
                }
                
                // Sửa tất cả panel tìm được
                foreach (GameObject panel in panels)
                {
                    Image image = panel.GetComponent<Image>();
                    if (image != null)
                    {
                        Color color = image.color;
                        color.a = 0.3f; // Set alpha to 0.3 (vừa phải - không chói, vẫn thấy vạch nốt)
                        image.color = color;
                        Debug.Log($"✅ Made {panelName} semi-transparent (alpha = 0.3)");
                        EditorUtility.SetDirty(panel);
                    }
                    
                    // Cũng kiểm tra CanvasGroup
                    CanvasGroup cg = panel.GetComponent<CanvasGroup>();
                    if (cg != null)
                    {
                        Debug.Log($"   - CanvasGroup found on {panelName}, alpha = {cg.alpha}");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ Panel {panelName} not found!");
            }
        }
        
        // Helper function để tìm tất cả children có cùng tên
        static void FindAllChildrenRecursive(Transform parent, string name, List<GameObject> results)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                {
                    results.Add(child.gameObject);
                }
                FindAllChildrenRecursive(child, name, results);
            }
        }
        
        // Helper function để tìm child recursively
        Transform FindChildRecursive(Transform parent, string name)
        {
            Transform found = parent.Find(name);
            if (found != null) return found;
            
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;
                found = FindChildRecursive(child, name);
                if (found != null) return found;
            }
            return null;
        }
        
        // 3. Extend LaneDivider length
        GameObject[] laneDividers = new GameObject[]
        {
            GameObject.Find("LaneDivider"),
            GameObject.Find("VachChia_Right"),
            GameObject.Find("VachChia_Center"),
            GameObject.Find("VachChia_Left")
        };
        
        foreach (GameObject divider in laneDividers)
        {
            if (divider != null)
            {
                Transform t = divider.transform;
                Vector3 scale = t.localScale;
                
                // Tăng độ dài (Z scale) lên gấp 3 lần
                scale.z *= 3f;
                t.localScale = scale;
                
                // Di chuyển về phía sau để kéo dài
                Vector3 pos = t.position;
                pos.z += 10f; // Dịch về phía sau
                t.position = pos;
                
                Debug.Log($"✅ Extended lane: {divider.name}");
                EditorUtility.SetDirty(divider);
            }
        }
        
        Debug.Log("🎉 All fixes complete!");
        Debug.Log("→ UI is now on top of lanes");
        Debug.Log("→ Menu panels are semi-transparent (lanes visible behind)");
        Debug.Log("→ Lanes are 3x longer");
    }
}

