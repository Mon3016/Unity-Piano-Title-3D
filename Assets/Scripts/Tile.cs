using UnityEngine; 
using System.Collections; 

public class Tile : MonoBehaviour
{
    // Cài đặt Script
    private GameController gameController;
    private Renderer tileRenderer;

    // Cài đặt Prefab (Cấu hình trong Inspector)
    public Material blackTileMat;
    public Material hitTileMat;
    public Material errorTileMat;
    public Material whiteTileMat; 
    public AudioSource sfxSource; 

    // Logic Chơi game
    [HideInInspector] public int tileType = 1; // 1: Đen, 0: Trắng
    private bool isHit = false;

    public void SetTileType(int type)
    {
        tileType = type;
        if (tileRenderer == null) tileRenderer = GetComponent<Renderer>();
        
        if (tileType == 1) // Ô Đen
        {
            if (tileRenderer != null && blackTileMat != null)
            {
                tileRenderer.material = blackTileMat;
            }
        }
        else // tileType == 0 (Ô Trắng)
        {
            if (tileRenderer != null && whiteTileMat != null)
            {
                tileRenderer.material = whiteTileMat;
            }
        }
    }

    void Awake()
    {
        gameController = FindFirstObjectByType<GameController>(); 
        tileRenderer = GetComponent<Renderer>();
        
        // Buộc gán Material trắng mặc định 
        if (tileRenderer != null && whiteTileMat != null)
        {
             tileRenderer.material = whiteTileMat;
        }
    }

    void Update()
    {
        if (Time.timeScale == 0f || gameController == null) return;
        
        // Di chuyển ô gạch
        transform.position += Vector3.back * gameController.currentSpeed * Time.deltaTime;

        // Kiểm tra MISS (Ô gạch đen trôi qua HitBar)
        if (tileType == 1 && !isHit && transform.position.z <= gameController.hitZPosition)
        {
            if (tileRenderer != null)
            {
                // Dùng hiệu ứng chớp nháy khi MISS
                StartCoroutine(MissFeedback());
            }
        }
    }

    private void OnMouseDown()
    {
        if (Time.timeScale == 0f) return;
        
        if (tileType == 1 && !isHit) // CHẠM ĐÚNG (Black Tile)
        {
            isHit = true;
            gameController.AddScore(); 
            
            if (sfxSource != null) sfxSource.Play();

            if (tileRenderer != null)
            {
                tileRenderer.material = hitTileMat;
            }
            Destroy(gameObject, 0.1f);
        }
        else // CHẠM SAI (Ô Trắng hoặc Khoảng trống)
        {
            // Bắt đầu hiệu ứng chớp nháy
            StartCoroutine(WrongTapFeedback());
        }
    }

    // Xử lý Hủy Gạch (Tự động xóa gạch sau khi trôi qua HitBar)
    void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem gạch có chạm vào vật thể có Tag là DestroyZone không
        if (other.CompareTag("DestroyZone"))
        {
            // Hủy gạch nếu gạch đã được nhấn (hit) hoặc là ô trắng (tileType=0)
            if (isHit || tileType == 0) 
            {
                Destroy(gameObject);
            }
        }
    }

    // Coroutine cho hiệu ứng chớp đỏ khi CHẠM SAI
    IEnumerator WrongTapFeedback()
    {
        int blinkCount = 3; 
        float blinkDuration = 0.08f; 

        // Màu gốc luôn là màu trắng vì chỉ chạm sai vào khoảng trống/ô trắng
        Material originalMat = whiteTileMat; 

        for (int i = 0; i < blinkCount; i++)
        {
            if (tileRenderer != null && errorTileMat != null)
            {
                tileRenderer.material = errorTileMat;
            }
            yield return new WaitForSecondsRealtime(blinkDuration); 

            if (tileRenderer != null && originalMat != null)
            {
                tileRenderer.material = originalMat;
            }
            yield return new WaitForSecondsRealtime(blinkDuration);
        }
        
        // Sau khi chớp xong, dừng game
        gameController.GameOver();
    }
    
    // Coroutine cho hiệu ứng chớp đỏ khi MISS (trôi qua vạch)
    IEnumerator MissFeedback()
    {
        int blinkCount = 3; 
        float blinkDuration = 0.08f; 

        // Màu gốc là màu đen
        Material originalMat = blackTileMat; 
        
        // Đặt tileType về 0 để tránh kích hoạt MissFeedback lần nữa trong Update
        tileType = 0; 

        for (int i = 0; i < blinkCount; i++)
        {
            if (tileRenderer != null && errorTileMat != null)
            {
                tileRenderer.material = errorTileMat;
            }
            yield return new WaitForSecondsRealtime(blinkDuration); 

            if (tileRenderer != null && originalMat != null)
            {
                tileRenderer.material = originalMat;
            }
            yield return new WaitForSecondsRealtime(blinkDuration);
        }
        
        // Sau khi chớp xong, dừng game
        gameController.GameOver();
    }
}