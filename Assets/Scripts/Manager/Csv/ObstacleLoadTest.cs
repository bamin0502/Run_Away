using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleLoadTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ObstacleTable obstacleTable = new ObstacleTable();
        obstacleTable.Load("RunAway_Obstacle"); // CSV 파일 이름 (확장자 없음)
    }

    // Update is called once per frame
    void Update()
    {
    }
}