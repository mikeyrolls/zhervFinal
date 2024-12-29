using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    public bool preventMovement = false;
    public bool isWet = false;
    public bool isOnFire = false;
    public bool isStuck = false;
    public bool freshlyUnstuck = false;
    public Vector3Int lastStuckTile = new Vector3Int(-1, -1, -1);
    public Vector2 lastInput = Vector2.zero;

    public Sprite def_front;
    public Sprite def_right;
    public Sprite def_left;
    public Sprite def_back;
    public Sprite wet_front;
    public Sprite wet_right;
    public Sprite wet_left;
    public Sprite wet_back;
    public Sprite fire_front;
    public Sprite fire_right;
    public Sprite fire_left;
    public Sprite fire_back;
    public Sprite stuck_front;
    public Sprite stuck_right;
    public Sprite stuck_left;
    public Sprite stuck_back;

    private SpriteRenderer spriteRenderer;

    private void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        lastInput = Vector2.down;
        //UpdateSprite();
    }

    public void preventMove() {
        preventMovement = true;
    }

    public void resetPreventMove() {
        preventMovement = false;
    }

    public void setWet(bool wet) {
        isWet = wet;
        if (isWet) {
            isOnFire = false;
        }
        UpdateSprite();
    }

    public void setOnFire(bool fire) {
        isOnFire = fire;
        if (isOnFire) {
            isWet = false;
        }
        UpdateSprite();
    }

    public void UpdateSprite() {
        if (isStuck) {
            if (lastInput == Vector2.up) {
                spriteRenderer.sprite = stuck_back;
            } else if (lastInput == Vector2.left) {
                spriteRenderer.sprite = stuck_left;
            } else if (lastInput == Vector2.right) {
                spriteRenderer.sprite = stuck_right;
            } else {
                spriteRenderer.sprite = stuck_front;
            }
        } else if (isWet) {
            if (lastInput == Vector2.up) {
                spriteRenderer.sprite = wet_back;
            } else if (lastInput == Vector2.left) {
                spriteRenderer.sprite = wet_left;
            } else if (lastInput == Vector2.right) {
                spriteRenderer.sprite = wet_right;
            } else {
                spriteRenderer.sprite = wet_front;
            }
        } else if (isOnFire) {
            if (lastInput == Vector2.up) {
                spriteRenderer.sprite = fire_back;
            } else if (lastInput == Vector2.left) {
                spriteRenderer.sprite = fire_left;
            } else if (lastInput == Vector2.right) {
                spriteRenderer.sprite = fire_right;
            } else {
                spriteRenderer.sprite = fire_front;
            }
        } else {
            if (lastInput == Vector2.up) {
                spriteRenderer.sprite = def_back;
            } else if (lastInput == Vector2.left) {
                spriteRenderer.sprite = def_left;
            } else if (lastInput == Vector2.right) {
                spriteRenderer.sprite = def_right;
            } else {
                spriteRenderer.sprite = def_front;
            }
        }
    }
}
