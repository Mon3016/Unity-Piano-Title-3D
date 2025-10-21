using UnityEngine;
using System.Collections; 

public class Tile : MonoBehaviour
{
    // Các biến cần được gán từ TileSpawner
    [HideInInspector] public float speed;
    [HideInInspector] public bool isBlackTile;
    
    // Tham chiếu (Cần gắn từ Inspector trên Prefab PianoTile)
    public Material flashRedMat; 
    
    private MeshRenderer meshRenderer;
    private GameController gameController;
    
    // --- KHAI BÁO BIẾN ĐÃ BỊ THIẾU TRƯỚC ĐÓ ---
    private bool isHit = false; 

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        gameController = FindFirstObjectByType<GameController>(); 
    }

    void Update()
    {
        if (Time.timeScale > 0)
        {
            // Di chuyển ô về phía Camera (trục Z âm)
            transform.Translate(Vector3.back * speed * Time.deltaTime);

            // KIỂM TRA MISS (ĐÃ SỬA: Thêm điều kiện !isHit)
            if (isBlackTile && !isHit && transform.position.z < -5f)
            {
                isHit = true; // Đánh dấu là đã xử lý
                StartFlash(true); 
                gameController.GameOver();
            }
        }
    }

    public void OnHit()
    {
        // Bảo vệ: Ngăn chặn xử lý nếu đã chạm hoặc game controller không tồn tại
        if (gameController == null || meshRenderer == null || isHit) return; 

        if (isBlackTile)
        {
            // CHẠM ĐÚNG Ô ĐEN
            isHit = true; // <--- ĐÁNH DẤU LÀ ĐÃ CHẠM!

            gameController.AddScore();

            // 2. Đổi màu sang xám và loại bỏ Collider
            meshRenderer.material = gameController.hitTileMat;
            GetComponent<Collider>().enabled = false;

            // 3. Hủy đối tượng sau 1 giây
            Destroy(gameObject, 1f); 
        }
        else
        {
            // CHẠM SAI Ô TRẮNG
            
            // 1. Bắt đầu nhấp nháy trên ô trắng này (chớp màu đỏ)
            StartFlash(false); 
            
            // 2. Kết thúc game
            gameController.GameOver(); 
        }
    }
    
    // --- HÀM XỬ LÝ NHẤP NHÁY ---
    public void StartFlash(bool isMiss)
    {
        StopAllCoroutines(); 
        StartCoroutine(FlashEffect(flashRedMat, isMiss));
    }

    IEnumerator FlashEffect(Material flashMaterial, bool destroyAfterFlash)
    {
        Material originalMaterial = meshRenderer.material;
        int flashes = 3; 

        for (int i = 0; i < flashes; i++)
        {
            meshRenderer.material = flashMaterial;
            yield return new WaitForSecondsRealtime(0.1f); // Dùng Realtime vì Time.timeScale có thể bằng 0

            meshRenderer.material = originalMaterial;
            yield return new WaitForSecondsRealtime(0.1f);
        }

        if (destroyAfterFlash) 
        {
            Destroy(gameObject); 
        }
    }
}