using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MagnetEffect : MonoBehaviour
{
    public float magnetRadius = 5f;
    public float magnetForce = 10f;

    private Collider[] itemBuffer = new Collider[15];
     
    private GameManager gameManager;

    private void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();
    }

    private void Update()
    {
        if (gameManager.IsMagnetEffectActive.Value)
        {
            AttractItems();
        }
    }

    private void AttractItems()
    {
        int numItems=Physics.OverlapSphereNonAlloc(transform.position, magnetRadius, itemBuffer);
        for (int i = 0; i < numItems; i++)
        {
            var item =itemBuffer[i].GetComponent<Item>();
            if (item)
            {
                var transform1 = item.transform;
                var position = transform1.position;
                var direction = transform.position - position;
                position += direction.normalized * (magnetForce * Time.deltaTime);
                transform1.position = position;
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, magnetRadius);
    }
#endif
    
}
