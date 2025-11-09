using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "RandomSpriteTile", menuName = "Tiles/Random Sprite Tile")]
public class RandomSpriteTile : TileBase
{
    public Sprite[] sprites;
    public Color color = Color.white;
    public Tile.ColliderType colliderType = Tile.ColliderType.None;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        if (sprites == null || sprites.Length == 0) return;

        // 讓同一座標穩定地拿到同一張（不會每次刷圖變）
        int hash = position.x * 73856093 ^ position.y * 19349663;
        if (hash < 0) hash = -hash;
        int idx = hash % sprites.Length;

        tileData.sprite = sprites[idx];
        tileData.color = color;
        tileData.flags = TileFlags.LockTransform;
        tileData.colliderType = colliderType;
    }
}
