using UnityEngine;

public class Tile : MonoBehaviour
{
    public float speed; 
    private GameController gameController;

    private int tileType = 0; // 0 = White (Miss), 1 = Black (Hit)
    private bool isHit = false; // Đã được chạm hay chưa

    // Tham chiếu Material (GÁN TRONG INSPECTOR của Prefab Tile)
    public Material blackTileMat; 
    public Material whiteTileMat; 
    public Material hitTileMat; // Màu xanh lá khi chạm đúng

    public Material errorTileMat; // Màu đỏ khi Miss/Sai
    
    void Start()
    {
        gameController = FindFirstObjectByType<GameController>();
        if (gameController == null)
        {
            Debug.LogError("Tile: Không tìm thấy GameController.");
        }
    }

    public void SetTileType(int type)
    {
        tileType = type;
        // Đổi màu ngay khi được sinh ra
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            // Vì TileSpawner chỉ sinh ra ô đen (type=1), nên ô này sẽ luôn có màu đen
            renderer.material = (tileType == 1) ? blackTileMat : whiteTileMat;
        }
    }

    void Update()
    {
        // Di chuyển ô gạch
        if (gameController != null && Time.timeScale == 1f)
        {
            transform.Translate(Vector3.back * speed * Time.deltaTime);
        }

        // Kiểm tra điều kiện Miss (Ô đen trôi qua)
        if (gameController != null && transform.position.z < gameController.hitZPosition && tileType == 1 && !isHit)
        {
            // 1. Đổi màu thành đỏ (Nháy đỏ)
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = errorTileMat; 
            }

            // 2. Game Over
            gameController.GameOver(); 
            
            enabled = false; 
        }
        
        // Hủy ô gạch khi nó đi khuất tầm nhìn (Nếu game chưa dừng)
        if (transform.position.z < -10f) 
        {
            Destroy(gameObject);
        }
    }

    // Xử lý khi người chơi chạm vào ô (Được gọi từ InputManager.cs)
    public void OnMouseDown()
    {
        if (Time.timeScale == 0f) return; 
        
        if (tileType == 1 && !isHit) // CHẠM ĐÚNG: Đổi màu xanh lá và hủy
        {
            isHit = true;
            gameController.AddScore();
            
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = hitTileMat; // Đổi màu xanh lá
            }

            // Hủy ô gạch sau 0.1 giây (tạo hiệu ứng biến mất)
            Destroy(gameObject, 0.1f); 
        }
        else if (tileType == 0) // CHẠM SAI: Nháy đỏ và Game Over
        {
            // Nếu bạn chạm vào vị trí không có ô gạch, logic này sẽ không được kích hoạt.
            // Nhưng nếu bạn chạm vào một ô gạch có TileType=0 (ô trắng) (theo code TileSpawner cũ)
            
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                 // Đổi màu ô bị chạm thành đỏ
                 renderer.material = errorTileMat; 
            }
            
            gameController.GameOver();
        }
    }
}