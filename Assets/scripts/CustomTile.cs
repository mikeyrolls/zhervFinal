using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Threading.Tasks;


[CreateAssetMenu(menuName = "Tiles/Custom Tile")]
public class CustomTile : Tile {

    public enum TileType { Normal, Wall, Fire, FireSmall, Electric, Gas, Ice, Water, Bounce, Mud, Hole, Win }
    public TileType tileType;
    public CustomTile replacementTile;
    [SerializeField] private Sprite[] animatedSprites = new Sprite[3];
    [SerializeField] private float animationSpeed = 1f;
    [SerializeField] private bool isAnimated = true;
    [SerializeField] private Vector3 cameraPositionOverride;
    public GameObject explosionAnim;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData) {
        base.GetTileData(position, tilemap, ref tileData);

        if (isAnimated && animatedSprites.Length > 0) {
            tileData.sprite = animatedSprites[0]; // animation
        } else if (animatedSprites.Length > 0) {
            int randomIndex = Random.Range(0, animatedSprites.Length);
            tileData.sprite = animatedSprites[randomIndex]; // mild wall and ground randomizing
        }
    }

    public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData) {
        if (isAnimated && animatedSprites.Length == 3) { // forced 3 before
            tileAnimationData.animatedSprites = animatedSprites;
            tileAnimationData.animationSpeed = animationSpeed;
            tileAnimationData.animationStartTime = 0;
            return true;
        }
        return false;
    }

    private Vector3Int getPlayerDirection(Vector2 lastInput) {
        if (lastInput == Vector2.up) return Vector3Int.up;
        if (lastInput == Vector2.down) return Vector3Int.down;
        if (lastInput == Vector2.left) return Vector3Int.left;
        if (lastInput == Vector2.right) return Vector3Int.right;
        return Vector3Int.zero; // Default: no direction
    }

    public bool isAccessible(CustomTile tile, PlayerStatus status)  {
        switch (tile.tileType)  {
            case TileType.Normal:
                return true;
            case TileType.Wall:
                return false;
            case TileType.Fire:
                return status.isWet;
            case TileType.FireSmall:
                return true;
            case TileType.Electric:
                return !status.isWet;
            case TileType.Gas:
                return status.isOnFire;
            case TileType.Ice:
                return true;
            case TileType.Water:
                return true;
            case TileType.Bounce:
                return true;
            case TileType.Mud:
                return true;
            case TileType.Hole:
                return false;
            default:
                return false;
        }
    }

private async void ExplodeGas(Vector3Int position, Tilemap tilemap) {
    TileBase currentTile = tilemap.GetTile(position);
    if (currentTile == this && tileType == TileType.Gas) {
        tilemap.SetTile(position, replacementTile);

        Vector3 worldPosition = tilemap.CellToWorld(position) + tilemap.tileAnchor;
        worldPosition += new Vector3(0.3f, 0.3f, 0); // making up for cell offset
        GameObject explosionInstance = Instantiate(explosionAnim, worldPosition, Quaternion.identity);
        Destroy(explosionInstance, 0.4f);

        Vector3Int[] directions = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

        foreach (Vector3Int direction in directions) { // checking surrounding tiles for gas
            Vector3Int adjacentPosition = position + direction;
            TileBase adjacentTile = tilemap.GetTile(adjacentPosition);

            if (adjacentTile is CustomTile adjacentCustomTile && adjacentCustomTile.tileType == TileType.Gas) {
                adjacentCustomTile.ExplodeGas(adjacentPosition, tilemap);
            }
        }
    }
}

    public void onPlayerStep(GameObject player, CustomTile lastTile) {

        PlayerStatus status = player.GetComponent<PlayerStatus>();
        Tilemap tilemap = FindObjectOfType<Tilemap>();
        Vector3Int playerCellPosition = tilemap.WorldToCell(player.transform.position);

        if (status.freshlyUnstuck) {
            if (status.lastStuckTile != playerCellPosition) {
                status.freshlyUnstuck = false; //moved from mud
            }
        }

        switch (tileType) {
            // normal tile, nothing happens
            case TileType.Normal:
                break;

            // wall tile, cannot be jumped over
            case TileType.Wall:
                status.preventMove();
                break;

            // fire tile, cannot be entered unless wet
            // if wet, changes to small fire
            case TileType.Fire:
                if (status.isWet) {
                    TileBase currentTile = tilemap.GetTile(playerCellPosition);
                    if (currentTile == this) { // change to small fire
                        tilemap.SetTile(playerCellPosition, replacementTile);
                    }
                } else  {
                    status.preventMove();
                }
                break;

            // fire tile walkable, sets player on fire (safely)
            case TileType.FireSmall:
                status.setOnFire(true);
                break;

            // electric tile, nothing happens if no status
            // shocked and dried if wet
            // destroy electric tile and put out fire if on fire
            case TileType.Electric:
                if (status.isWet) {
                    //shocked anim?
                    status.setWet(false);
                    status.preventMove();
                } else if (status.isOnFire) {
                    TileBase currentTile = tilemap.GetTile(playerCellPosition);
                    if (currentTile == this) { // destroy electric
                        tilemap.SetTile(playerCellPosition, replacementTile);
                        status.setOnFire(false);
                    }
                }
                break;

            // cannot walk through
            // can set on fire including all surrounding gas tiles
            case TileType.Gas:
                if (status.isOnFire) {
                    ExplodeGas(playerCellPosition, tilemap);
                } else {
                    status.preventMove();
                }
                break;

            // slip on ice until not on ice or move prevented (wall, fire, ...)
            // melt if on fire
            case TileType.Ice:
                if (status.isOnFire) {
                    TileBase currentTile = tilemap.GetTile(playerCellPosition);
                    if (currentTile == this) { // melting ice 
                        tilemap.SetTile(playerCellPosition, replacementTile);
                    }
                } else {
                    Vector3Int slideDirection = getPlayerDirection(status.lastInput);
                    player.GetComponent<PlayerMovement>().startSliding(slideDirection);
                }
                break;

            // make player wet, put out fire
            case TileType.Water:
                status.setWet(true);
                break;
                
            // bounce over a tile (not wall)
            case TileType.Bounce:
                Vector3Int bounceDirection = getPlayerDirection(status.lastInput);
                player.GetComponent<PlayerMovement>().jump(tilemap.WorldToCell(player.transform.position), bounceDirection, maxDistance: 2);
                break;

            // get stuck once, remove statuses
            case TileType.Mud:
                status.setWet(false);
                status.setOnFire(false);
                if (!status.isStuck && !status.freshlyUnstuck)  {
                    status.isStuck = true;
                    status.lastStuckTile = playerCellPosition;
                }
                if (status.isStuck) {
                    if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D)) {
                        status.isStuck = false;
                        status.freshlyUnstuck = true;
                    }
                }
                break;

            // hole, blocks path but not jumping
            case TileType.Hole:
                status.preventMove();
                break;

            case TileType.Win:
                Debug.Log("level finished, hapi hapi haaapi");
                CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();
                if (cameraFollow != null) {
                    cameraFollow.DisableFollow(cameraPositionOverride);
                }
                break;

            default:
                status.preventMove();
                break;
        }
    }
}
