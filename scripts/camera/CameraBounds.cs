using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBounds : MonoBehaviour {
    public Camera targetCamera;

    public bool loopX;
    public bool loopY;
    
    public Bounds cameraBounds;

    private void LateUpdate() {
        float newX = EvaluateAxis(targetCamera.transform.position.x, targetCamera.orthographicSize * targetCamera.aspect, cameraBounds.min.x, cameraBounds.max.x, loopX);
        float newY = EvaluateAxis(targetCamera.transform.position.y, targetCamera.orthographicSize, cameraBounds.min.y, cameraBounds.max.y, loopY);
        targetCamera.transform.position = new Vector3(newX, newY, targetCamera.transform.position.z);
    }

    private float EvaluateAxis(float input, float camHalfBounds, float min, float max, bool loop) {
        if (loop) {
            if (input > max) {
                input = min + (input - max);
            }
            else if (input < min) {
                input = max + (input - min);
            }
        }
        else {
            if (input + camHalfBounds > max) {
                input = max - camHalfBounds;
            }
            if (input - camHalfBounds < min) {
                input = min + camHalfBounds;
            }
        }

        return input;
    }
}
