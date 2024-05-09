using UnityEngine;

public class Utils : MonoBehaviour
{
    public static Vector3 ScreenToWorld(Camera camera, Vector3 screenPosition)
    {
        var ray = camera.ScreenPointToRay(screenPosition);
        return Physics.Raycast(ray, out RaycastHit hit) ? hit.point : Vector3.zero;
    }
}
