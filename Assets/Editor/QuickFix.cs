using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;

public class QuickFix : EditorWindow
{
    [MenuItem("Tools/Quick Fix UI")]
    public static void FixUI()
    {
        // Fix EventSystem
        EventSystem es = FindFirstObjectByType<EventSystem>();
        if (es != null)
        {
            if (es.GetComponent<StandaloneInputModule>() == null)
            {
                es.gameObject.AddComponent<StandaloneInputModule>();
                Debug.Log("✅ Added StandaloneInputModule to EventSystem");
            }
            else
            {
                Debug.Log("✅ EventSystem already has StandaloneInputModule");
            }
            EditorUtility.SetDirty(es.gameObject);
        }
        else
        {
            Debug.LogError("❌ No EventSystem found!");
        }
    }
}
