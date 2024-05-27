using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private PlayerAni playerAni;
    private GameManager gameManager;
    private Tile tile;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerAni = GetComponent<PlayerAni>();
        gameManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();
        tile = GameObject.FindGameObjectWithTag("TileManager").GetComponent<Tile>();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Ground") || other.collider.CompareTag("WalkBy"))
        {
            playerMovement.isJumping = false;
            if (!playerMovement.isSliding) playerAni.SetRunAnimation();
        }

        if (other.collider.CompareTag("Obstacle"))
        {
#if UNITY_EDITOR
            Debug.Log("Hit an obstacle, game over!");
#endif
            if (gameManager.IsFeverModeActive.Value)
            {
                LaunchObstacle(other.gameObject);
                return;
            }
            else if (!playerMovement.isInvincible)
            {
                gameManager.GameOver();
                playerMovement.Die();
            }
        }

        if (other.collider.CompareTag("Wall"))
        {
#if UNITY_EDITOR
            Debug.Log("Hit a wall, returning to last position.");
#endif
            if (gameManager.IsFeverModeActive.Value)
            {
                LaunchObstacle(other.gameObject);
                return;
            }
            else
            {
                playerMovement.targetPosition = playerMovement.lastPosition;
                playerMovement.rb.position = playerMovement.lastPosition;
                playerMovement.currentLaneIndex = playerMovement.lastLaneIndex;
            }
        }

        if (other.collider.CompareTag("WalkBy"))
        {
            if (!gameManager.IsFeverModeActive.Value)
            {
                playerMovement.isJumping = false;
                playerAni.SetRunAnimation();
            }
            else
            {
                LaunchObstacle(other.gameObject);
            }
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.collider.CompareTag("Ground") || other.collider.CompareTag("WalkBy"))
        {
            playerMovement.isJumping = true;
        }
        if (other.collider.CompareTag("Obstacle"))
        {
            playerMovement.isCollidingFront = false;
        }
        if (other.collider.CompareTag("Wall"))
        {
            playerMovement.isCollidingFront = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            GameObject o;
            (o = other.gameObject).SetActive(false);
            tile.itemPool.Enqueue(o);
            other.GetComponent<Item>().Use();
        }
    }

    private void LaunchObstacle(GameObject obstacle)
    {
        Rigidbody obstacleRigidbody = obstacle.GetComponent<Rigidbody>();
        if (obstacleRigidbody != null)
        {
            SetLayerRecursively(obstacle, LayerMask.NameToLayer("IgnorePlayerCollision"));

            Vector3 launchDirection = (obstacle.transform.position - transform.position).normalized + Vector3.up;
            float launchForce = 500f;
            obstacleRigidbody.isKinematic = false;
            obstacleRigidbody.AddForce(launchDirection * launchForce);
        }
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;

        Stack<Transform> stack = new Stack<Transform>();
        stack.Push(obj.transform);

        while (stack.Count > 0)
        {
            Transform current = stack.Pop();
            current.gameObject.layer = newLayer;

            foreach (Transform child in current)
            {
                stack.Push(child);
            }
        }
    }
}
