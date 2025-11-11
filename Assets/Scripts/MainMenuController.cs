using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // Gọi khi nhấn nút Play
    public void OnPlayButton()
    {
        SceneManager.LoadScene("SampleScene"); 
    }

    // Gọi khi nhấn nút Settings
    public void OnSettingsButton()
    {
        // Hiện panel cài đặt 
        settingsPanel.SetActive(true);
    }

    // Gọi khi nhấn nút Back trong Settings
    public void OnBackButton()
    {
        settingsPanel.SetActive(false);
    }

    [Header("References")]
    public GameObject settingsPanel;
}
