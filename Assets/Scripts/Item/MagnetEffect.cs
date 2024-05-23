using System.Collections.Generic;
using UnityEngine;

public partial class MagnetEffect : MonoBehaviour
{
    public float magnetRadius = 5f;
    public float magnetForce = 10f;

    private Collider[] itemBuffer = new Collider[15];
    private GameManager gameManager;
    public Transform playerTransform;
    private BoxCollider playerCollider;
    private Tile tile;

    private void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();
        playerCollider = playerTransform.GetComponent<BoxCollider>();
        tile = GameObject.FindGameObjectWithTag("TileManager").GetComponent<Tile>();

        if (playerCollider == null)
        {
            return;
#if UNITY_EDITOR
            Debug.LogError("Player BoxCollider not found!");
#endif
            
            
        }
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
        int numItems = Physics.OverlapSphereNonAlloc(playerTransform.position, magnetRadius, itemBuffer);
        for (int i = 0; i < numItems; i++)
        {
            var item = itemBuffer[i].GetComponent<Item>();
            if (item)
            {
                var itemTransform = item.transform;
                Vector3 targetPosition = playerCollider.bounds.center;
                var position = itemTransform.position;
                Vector3 direction = (targetPosition - position).normalized;
                
                float distanceThisFrame = magnetForce * Time.deltaTime;
                float distanceToTarget = Vector3.Distance(position, targetPosition);
                
                if (distanceToTarget < distanceThisFrame)
                {
                    item.Use();
                    GameObject o;
                    (o = item.gameObject).SetActive(false);
                    tile.itemPool.Enqueue(o);
                }
                else
                {
                    Vector3 tileMovement = -Vector3.forward * (gameManager.stageSpeed * Time.deltaTime);
                    itemTransform.position += direction * distanceThisFrame + tileMovement;
                }
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (playerTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, magnetRadius);
        }
    }
#endif
}
