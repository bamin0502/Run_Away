using System.Collections.Generic;
using UnityEngine;

public class TileMovement : MonoBehaviour
{
    public float initialMoveSpeed = 5f;
    public float moveSpeedIncreaseDistance = 20f;
    public float moveSpeedMultiplier = 1.05f;
    public float maxMoveSpeed = 30f;

    private float moveSpeed;
    private float distanceTravelled = 0f;
    private float nextSpeedIncreaseDistance;
    private GameManager gameManager;

    private void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();
    }

    public void InitializeMovement()
    {
        moveSpeed = initialMoveSpeed;
        nextSpeedIncreaseDistance = moveSpeedIncreaseDistance;
    }

    public void MoveTiles(List<Transform> tiles, float speed)
    {
        moveSpeed = speed;

        foreach (var tile in tiles)
        {
            tile.Translate(-Vector3.forward * (moveSpeed * Time.deltaTime), Space.World);
        }

        distanceTravelled += moveSpeed * Time.deltaTime;

        if (distanceTravelled > nextSpeedIncreaseDistance)
        {
            moveSpeed = Mathf.Min(moveSpeed * moveSpeedMultiplier, maxMoveSpeed);
            nextSpeedIncreaseDistance += moveSpeedIncreaseDistance;
        }

        gameManager.stageSpeed = moveSpeed;
    }
}
