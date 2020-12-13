using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class FollowerController : MonoBehaviour
{
    public Transform player;
    public Transform planet;

    // Must be under 180°
    public float distanceAngle;

    // Update is called once per frame
    void Update()
    {
        Vector3 gravityDirection = planet.position - transform.position;
        
        // Rotation radial
        if (transform.position.x > planet.position.x)
            transform.rotation =  Quaternion.Euler(0, 0, -Vector2.Angle(Vector2.up, -gravityDirection));
        else
            transform.rotation = Quaternion.Euler(0, 0, Vector2.Angle(Vector2.up, -gravityDirection));

        // Follow player
        float currentAngle = Vector2.Angle(transform.position - planet.position, player.position - planet.position);
        transform.RotateAround(planet.position, Vector3.forward, distanceAngle - currentAngle);
    }
}
