using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ResetMenuUI
{
    [MenuItem("Tools/Reset Menu UI (Delete All)")]
    public static void ResetAll()
    {
        if (!EditorUtility.DisplayDialog(
            "Reset Menu UI",
            "This will DELETE:\n" +
            "- MenuCanvas (if exists)\n" +
            "- All menu panels (MainMenuPanel, PauseMenuPanel, etc.)\n" +
            "- MenuManager GameObject\n\n" +
            "Your game Canvas and game UI (ScoreText, ComboText, etc.) will NOT be deleted.\n\n" +
            "Continue?",
            "Yes, Delete All",
            "Cancel"))
        {
            Debug.Log("❌ Reset cancelled by user");
            return;
        }

        Debug.Log("🗑️ Starting Menu UI reset...");

        // 1. Delete MenuCanvas
        Canvas[] allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();
        foreach (Canvas canvas in allCanvases)
        {
            if (canvas.name == "MenuCanvas")
            {
                Debug.Log($"🗑️ Deleting MenuCanvas: {canvas.name}");
                Object.DestroyImmediate(canvas.gameObject);
            }
        }

        // 2. Delete all menu panels (tìm trong tất cả Canvas)
        string[] panelNames = { 
            "MainMenuPanel", 
            "PauseMenuPanel", 
            "SongSelectionPanel", 
            "GameOverPanel" 
        };

        foreach (string panelName in panelNames)
        {
            List<GameObject> panelsToDelete = new List<GameObject>();
            
            // Tìm trong tất cả objects
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name == panelName)
                {
                    // Kiểm tra xem có phải là UI panel không (có RectTransform)
                    if (obj.GetComponent<RectTransform>() != null)
                    {
                        panelsToDelete.Add(obj);
                    }
                }
            }
            
            // Xóa tất cả panel tìm được
            foreach (GameObject panel in panelsToDelete)
            {
                Debug.Log($"🗑️ Deleting panel: {panelName}");
                Object.DestroyImmediate(panel);
            }
            
            if (panelsToDelete.Count > 1)
            {
                Debug.LogWarning($"⚠️ Found {panelsToDelete.Count} duplicate {panelName} panels - all deleted!");
            }
        }

        // 3. Delete MenuManager
        MenuManager[] menuManagers = Resources.FindObjectsOfTypeAll<MenuManager>();
        foreach (MenuManager mm in menuManagers)
        {
            if (mm != null && mm.gameObject != null)
            {
                Debug.Log($"🗑️ Deleting MenuManager: {mm.gameObject.name}");
                Object.DestroyImmediate(mm.gameObject);
            }
        }

        // 4. Clean up any orphaned UI elements (buttons, text, etc. that might be left)
        GameObject[] allUIObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        List<GameObject> toDelete = new List<GameObject>();
        
        foreach (GameObject obj in allUIObjects)
        {
            // Check if it's a menu-related UI element
            if (obj.name.Contains("PlayButton") || 
                obj.name.Contains("QuitButton") || 
                obj.name.Contains("ResumeButton") || 
                obj.name.Contains("RestartButton") || 
                obj.name.Contains("MainMenuButton") ||
                obj.name.Contains("BackButton") ||
                obj.name.Contains("Song_") ||
                obj.name.Contains("SongScrollView") ||
                obj.name == "Piano Tiles 3D" ||
                obj.name == "PAUSED" ||
                obj.name == "SELECT SONG" ||
                obj.name == "GAME OVER")
            {
                RectTransform rect = obj.GetComponent<RectTransform>();
                if (rect != null)
                {
                    // Kiểm tra xem có phải là child của Canvas không
                    Transform parent = obj.transform.parent;
                    while (parent != null)
                    {
                        if (parent.GetComponent<Canvas>() != null)
                        {
                            toDelete.Add(obj);
                            break;
                        }
                        parent = parent.parent;
                    }
                }
            }
        }
        
        foreach (GameObject obj in toDelete)
        {
            Debug.Log($"🗑️ Deleting orphaned UI element: {obj.name}");
            Object.DestroyImmediate(obj);
        }

        Debug.Log("✅ Menu UI reset complete!");
        Debug.Log("📝 Your game Canvas and game UI (ScoreText, ComboText, etc.) are preserved.");
        Debug.Log("📝 You can now run 'Tools → Create Menu UI' to create a fresh menu system.");
        
        // Force refresh
        EditorUtility.SetDirty(Selection.activeGameObject);
    }
}

