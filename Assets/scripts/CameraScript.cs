using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {
    public Transform player;
    public Vector3 offset;
    public float smoothSpeed = 0.125f;
    private bool onPlayer = true;
    private Vector3 staticPosition;

    void LateUpdate() {
        if (onPlayer && player != null) {
            Vector3 desiredPosition = player.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        } else {
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, staticPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }

    public void DisableFollow(Vector3 newStaticPosition) {
        onPlayer = false;
        staticPosition = newStaticPosition;
    }

    public void EnableFollow() {
        onPlayer = true;
    }
}
