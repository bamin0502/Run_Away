using System.Collections.Generic;
using UnityEngine;

public partial class MagnetEffect : MonoBehaviour
{
    public float magnetRadius = 5f;
    public float magnetForce = 10f;

    private Collider[] itemBuffer = new Collider[15];
    private GameManager gameManager;
    private Transform playerTransform;
    private BoxCollider playerCollider;
    private Tile tile;
    
    private void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        playerCollider = playerTransform.GetComponent<BoxCollider>();
        tile = GameObject.FindGameObjectWithTag("TileManager").GetComponent<Tile>();
        if (playerCollider == null)
        {
            Debug.LogError("Player BoxCollider not found!");
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
        int numItems = Physics.OverlapSphereNonAlloc(transform.position, magnetRadius, itemBuffer);
        for (int i = 0; i < numItems; i++)
        {
            var item = itemBuffer[i].GetComponent<Item>();
            if (item)
            {
                var itemTransform = item.transform;
                Vector3 targetPosition = playerCollider.bounds.center;
                var position = itemTransform.position;
                Vector3 direction = (targetPosition - position).normalized;

                // 이동 거리 계산
                float distanceThisFrame = magnetForce * Time.deltaTime;
                float distanceToTarget = Vector3.Distance(position, targetPosition);

                // 아이템이 플레이어와 너무 가까워지면 아이템을 먹음
                if (distanceToTarget < distanceThisFrame)
                {
                    gameManager.AddCoin(); // 코인 획득 처리 (혹은 적절한 아이템 획득 처리)
                    GameObject o;
                    (o = item.gameObject).SetActive(false); // 아이템 비활성화
                    tile.itemPool.Enqueue(o); // 아이템을 풀에 반환
                }
                else
                {
                    // 타일의 움직임 보정
                    Vector3 tileMovement = Vector3.back * (gameManager.stageSpeed * Time.deltaTime);
                    itemTransform.position += direction * distanceThisFrame + tileMovement;
                }
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
