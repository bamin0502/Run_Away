using UnityEngine;

public class CameraEffect : MonoBehaviour
{
    public Transform target;
    public float followSpeed = 10f;
    public Vector3 offset;

    private void LateUpdate()
    {
        if(target)
        {
            Vector3 targetPosition = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        }
    }

}
