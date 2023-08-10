using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dragable : MonoBehaviour
{
    Vector3 mousePositionOffset;
    private Vector3 MouseWorldPosition{
        get => Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    protected virtual void OnMouseDown() {
        if (enabled)
            mousePositionOffset = transform.position - MouseWorldPosition;
    }
    protected virtual void OnMouseDrag() {
        if (enabled)
            transform.position = MouseWorldPosition + mousePositionOffset;
    }
}
