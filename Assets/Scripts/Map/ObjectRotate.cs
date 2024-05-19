using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRotate : MonoBehaviour
{
    public float rotateSpeed = 10f;

    private void Update()
    {
        transform.Rotate(Vector3.up * (rotateSpeed * Time.deltaTime));
    }
}
