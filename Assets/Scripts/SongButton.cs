using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SongButton : MonoBehaviour
{
    public int songIndex;
    public TextMeshProUGUI songNameText;
    public TextMeshProUGUI bpmText;
    public Image coverImage;
    
    public MenuManager menuManager;
    private Button button;
    
    void Start()
    {
        menuManager = FindFirstObjectByType<MenuManager>();
        button = GetComponent<Button>();
        
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
    }
    
    public void SetupButton(SongData songData, int index)
    {
        songIndex = index;
        
        if (songNameText != null)
        {
            songNameText.text = songData.songName;
        }
        
        if (bpmText != null)
        {
            bpmText.text = "BPM: " + songData.bpm.ToString();
        }
        
        if (coverImage != null && songData.coverImage != null)
        {
            coverImage.sprite = songData.coverImage;
        }
    }
    
    void OnClick()
    {
        if (menuManager != null)
        {
            menuManager.SelectSong(songIndex);
        }
    }
}
