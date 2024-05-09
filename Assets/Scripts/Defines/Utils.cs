using UnityEngine;

public class Utils : MonoBehaviour
{
    public static Vector3 ScreenToWorld(Camera camera, Vector3 screenPosition)
    {
        // 스크린 좌표 유효성 검사
        if (float.IsInfinity(screenPosition.x) || float.IsInfinity(screenPosition.y) || 
            float.IsNaN(screenPosition.x) || float.IsNaN(screenPosition.y))
        {
            Debug.LogWarning("Invalid screen position: " + screenPosition);
            return Vector3.zero;  // 오류가 발생했을 때의 대체 값
        }

        var ray = camera.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.point;
        }
        else
        {
            return Vector3.zero; // Raycast가 실패했을 때 반환할 기본 값
        }
    }
}
