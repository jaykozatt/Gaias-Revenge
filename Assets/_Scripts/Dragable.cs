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
        print($"Draging: {gameObject.name}");
        mousePositionOffset = transform.position - MouseWorldPosition;
    }
    protected virtual void OnMouseDrag() {
        transform.position = MouseWorldPosition + mousePositionOffset;
    }
}
