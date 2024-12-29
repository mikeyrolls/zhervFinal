using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMovement : MonoBehaviour {
    public float moveDistance = 3.2f; // tilesize
    private Vector2 targetPosition;
    private PlayerStatus status;
    private CustomTile lastTile; // Track the last tile stepped on
    private bool isSliding = false;
    private bool stopNextTile = false;
    

    private void Start() {
        targetPosition = transform.position;
        status = GetComponent<PlayerStatus>();
    }

    private void Update()
    {
        if ((Vector2)transform.position == targetPosition) {
            handleInput();
        } else {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, 10f * Time.deltaTime);
        }
    }

    public void setTargetPosition(Vector3 newPosition) {
        targetPosition = newPosition;
    }

    private bool isWall(TileBase tile, Tilemap tilemap) {
        if (tile == null) return true;
        if (tile is CustomTile customTile && customTile.tileType == CustomTile.TileType.Wall) return true; // wall tile
        return false;
    }

    private Vector3Int getPlayerDirection(KeyCode lastInput) {
        if (lastInput == KeyCode.W) return Vector3Int.up;
        if (lastInput == KeyCode.A) return Vector3Int.left;
        if (lastInput == KeyCode.S) return Vector3Int.down;
        if (lastInput == KeyCode.D) return Vector3Int.right;
        return Vector3Int.zero;
    }

public void startSliding(Vector3Int direction) {
    if (isSliding) return; // one slide
    StartCoroutine(slide(direction));
}

private IEnumerator slide(Vector3Int direction) {
    isSliding = true;
    Tilemap tilemap = FindObjectOfType<Tilemap>();
    Vector3Int currentCell = tilemap.WorldToCell(transform.position);
    TileBase currentTile = tilemap.GetTile(currentCell);
    Vector3Int nextCell = currentCell + direction;
    TileBase nextTile = tilemap.GetTile(nextCell);
    stopNextTile = false;

    while (true) {
        if(stopNextTile) {
            stopNextTile = false;
            break;
        }
        
        if (!(currentTile is CustomTile customTile) || customTile.tileType != CustomTile.TileType.Ice) { // not on ice
            stopNextTile = true; 
        }
        
        if (!(nextTile is CustomTile nextCustomTile) || !nextCustomTile.isAccessible(nextCustomTile, status)) { //inaccessible tile
            stopNextTile = false;
            break;
        }

        targetPosition = tilemap.CellToWorld(nextCell) + new Vector3(0.8f, 1.3f, 0); //centering + Y offset

        while ((Vector2)transform.position != targetPosition) {
            yield return null;
        }

        nextCell = currentCell + direction;
        currentCell = nextCell;
        currentTile = tilemap.GetTile(currentCell);
        nextTile = tilemap.GetTile(nextCell);
    }
    isSliding = false;
}

public void jump(Vector3Int startCell, Vector3Int direction, int maxDistance = 2) {
    Tilemap tilemap = FindObjectOfType<Tilemap>();
    Vector3Int currentCell = startCell;
    Vector3 targetPosition = transform.position;
    PlayerStatus status = GetComponent<PlayerStatus>();

    for (int i = 1; i <= maxDistance; i++) {
        Vector3Int nextCell = currentCell + direction;
        TileBase nextTile = tilemap.GetTile(nextCell);
        CustomTile customTile = nextTile as CustomTile;

        if (customTile != null && customTile.tileType == CustomTile.TileType.Wall) { //wall check
            break;
        }

        if (customTile == null || !customTile.isAccessible(customTile, status)) { //jumping over inaccessible tiles
            if (i == maxDistance) break;
            currentCell = nextCell;
            continue;
        }

        currentCell = nextCell;
        targetPosition = tilemap.CellToWorld(currentCell) + new Vector3(0.8f, 1.3f, 0);
    }
    StartCoroutine(performjump(targetPosition));
}

    private IEnumerator performjump(Vector3 targetPosition) {
        // animation?
        yield return new WaitForSeconds(0.2f); //delay
        setTargetPosition(targetPosition);
    }

    private void handleInput() {
        status.UpdateSprite();
        if (status.isStuck) {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D)) {
                status.isStuck = false;
                status.freshlyUnstuck = true;
                return; // dont move, just unstick
            } else {
                return;
            }
        }

        if (status.preventMovement) {
            status.resetPreventMove();
            return;
        }

        Vector2 newPosition = targetPosition;

        if (Input.GetKeyDown(KeyCode.W)) {
            status.lastInput = Vector2.up;
            status.UpdateSprite();
            newPosition += Vector2.up * moveDistance;
            
        }
        if (Input.GetKeyDown(KeyCode.S)) {
            status.lastInput = Vector2.down;
            status.UpdateSprite();
            newPosition += Vector2.down * moveDistance;
            
        }
        if (Input.GetKeyDown(KeyCode.A)) {
            status.lastInput = Vector2.left;
            status.UpdateSprite();
            newPosition += Vector2.left * moveDistance;
            
        }
        if (Input.GetKeyDown(KeyCode.D)) {
            status.lastInput = Vector2.right;
            status.UpdateSprite();
            newPosition += Vector2.right * moveDistance;
            
        }

        Tilemap tilemap = FindObjectOfType<Tilemap>();
        Vector3Int cellPosition = tilemap.WorldToCell(newPosition);
        TileBase tile = tilemap.GetTile(cellPosition);

        if (tile is CustomTile customTile) {
            customTile.onPlayerStep(gameObject, lastTile);
            if (status.preventMovement) {
                return;
            }
            lastTile = customTile; // Update the last tile
        }
        targetPosition = newPosition;
    }

}
