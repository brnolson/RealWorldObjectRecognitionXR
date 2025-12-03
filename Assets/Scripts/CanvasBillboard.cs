using UnityEngine;

public class CanvasBillboard : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main != null)
        {
            var cam = Camera.main.transform;
            transform.LookAt(cam.position);
            transform.Rotate(0, 180f, 0);
        }
    }
}
