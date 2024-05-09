using UnityEngine;

public class Utils : MonoBehaviour
{
    public static Vector3 ScreenToWorld(Camera camera, Vector3 screenPosition)
    {
        screenPosition.z = 5f;
        return camera.ScreenToWorldPoint(screenPosition);
    }
}
