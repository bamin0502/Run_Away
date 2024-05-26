using UnityEngine;

public class SpeedManager : MonoBehaviour
{
    public float moveSpeedIncreaseDistance = 20f;
    public float moveSpeedMultiplier = 1.05f;
    public float initialMoveSpeed = 5f;
    public float maxMoveSpeed = 15f;

    private float moveSpeed;
    private float distanceTravelled = 0f;
    private float nextSpeedIncreaseDistance;

    void Start()
    {
        moveSpeed = initialMoveSpeed;
        nextSpeedIncreaseDistance = moveSpeedIncreaseDistance;
    }

    public float UpdateSpeed(float deltaTime)
    {
        distanceTravelled += moveSpeed * deltaTime;

        if (distanceTravelled > nextSpeedIncreaseDistance)
        {
            moveSpeed = Mathf.Min(moveSpeed * moveSpeedMultiplier, maxMoveSpeed);
            nextSpeedIncreaseDistance += moveSpeedIncreaseDistance;
        }

        return moveSpeed;
    }
}