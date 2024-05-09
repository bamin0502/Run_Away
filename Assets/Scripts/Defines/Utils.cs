using UnityEngine;

public class Utils : MonoBehaviour
{
    public static Vector3 ScreenToWorld(Camera camera, Vector3 screenPosition)
    {
        // 유효하지 않은 값 처리
        if (float.IsInfinity(screenPosition.x) || float.IsInfinity(screenPosition.y) || float.IsNaN(screenPosition.x) || float.IsNaN(screenPosition.y))
        {
            Debug.LogWarning("Invalid screen position: " + screenPosition);
            return Vector3.zero;  // 유효하지 않은 값이면 기본 위치로 (0, 0, 0) 반환
        }

        screenPosition.z = 5f;
        return camera.ScreenToWorldPoint(screenPosition);
    }
}
