using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class PlayerCollision : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private PlayerAni playerAni;
    private GameManager gameManager;
    private Tile tile;

    private CompositeDisposable disposables = new CompositeDisposable();

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerAni = GetComponent<PlayerAni>();
        gameManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();
        tile = GameObject.FindGameObjectWithTag("TileManager").GetComponent<Tile>();
    }

    private void OnCollisionEnter(Collision other)
    {
        switch (other.collider.tag)
        {
            case "Ground":
            case "WalkBy":
                playerMovement.isJumping = false;
                if (!playerMovement.isSliding) playerAni.SetRunAnimation();
                break;
            case "Obstacle":
#if UNITY_EDITOR
                Debug.Log("Hit an obstacle, game over!");
#endif
                if (gameManager.IsFeverModeActive.Value)
                {
                    LaunchObstacle(other.gameObject);
                }
                else if (!playerMovement.isInvincible)
                {
                    gameManager.GameOver();
                    playerMovement.Die();
                }
                break;
            case "Wall":
#if UNITY_EDITOR
                Debug.Log("Hit a wall, returning to last position.");
#endif
                if (gameManager.IsFeverModeActive.Value)
                {
                    LaunchObstacle(other.gameObject);
                }
                else
                {
                    playerMovement.targetPosition = playerMovement.lastPosition;
                    playerMovement.rb.position = playerMovement.lastPosition;
                    playerMovement.currentLaneIndex = playerMovement.lastLaneIndex;
                }
                break;
        }
    }

    private void OnCollisionExit(Collision other)
    {
        switch (other.collider.tag)
        {
            case "Ground":
            case "WalkBy":
                playerMovement.isJumping = true;
                break;
            case "Obstacle":
            case "Wall":
                playerMovement.isCollidingFront = false;
                break;
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
        if (!gameManager.IsFeverModeActive.Value) return;

        Rigidbody obstacleRb = obstacle.GetComponent<Rigidbody>();
        if (obstacleRb != null)
        {
            SetLayerRecursively(obstacle, LayerMask.NameToLayer("IgnorePlayerCollision"));
            Vector3 playerToObstacleDirection = (obstacle.transform.position - transform.position).normalized;
            Vector3 launchDirection = Vector3.right * Mathf.Sign(playerToObstacleDirection.x) + Vector3.up * 0.5f;

            float launchForce = 500f;
            obstacleRb.isKinematic = false;
            obstacleRb.AddForce(launchDirection * launchForce);
        }

        ParticleSystem obstacleParticle = obstacle.GetComponentInChildren<ParticleSystem>();
        if (obstacleParticle != null)
        {
            obstacleParticle.Play();
            Observable.Timer(System.TimeSpan.FromSeconds(2.0f))
                .Subscribe(_ => obstacleParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear))
                .AddTo(disposables);
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
