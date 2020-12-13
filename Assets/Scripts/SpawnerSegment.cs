using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerSegment : MonoBehaviour
{
    //Variables
    private GameManager gameManager;
    private int emptySpawner;
    private int numberLane;
    int i;

    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        numberLane = gameManager.numberLane;
    }

    //Generate new feathers
    void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.name == "Follower")
        {
            //There is always 1 empty space to go through
            emptySpawner = Random.Range(0, numberLane);

            foreach (Transform child in transform)
            {
                if (child.tag == "Spawner")
                {
                    if(i != emptySpawner)
                    {
                        child.gameObject.GetComponent<Spawner>().Spawn();
                    }
                    i++;
                }
            }

            i = 0;
        }
    }
}
