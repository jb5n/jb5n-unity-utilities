using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIElementAutoScaler : MonoBehaviour {
    public float referenceCameraOrthoSize;

    private void LateUpdate() {
        float curCamOrthoScale = Camera.main.orthographicSize / referenceCameraOrthoSize;
        transform.localScale = Vector3.one * curCamOrthoScale;
    }
}
