using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateClouds : MonoBehaviour
{
    public Transform planet;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform cloud = transform.GetChild(i);
            
            var direction = (planet.transform.position - cloud.position).normalized;
            Vector3 rotation = cloud.rotation.eulerAngles;
            float angle = -Mathf.Sign(cloud.position.x - planet.transform.position.x) * 
                          (Vector3.Angle(new Vector3(0, 1, 0), -direction));
            cloud.rotation = Quaternion.Euler(rotation.x, rotation.y, angle);

            SpriteRenderer spriteRenderer = cloud.GetComponent<SpriteRenderer>();
            // spriteRenderer.color = Color.black;
            spriteRenderer.color = new Color(1f, 1f, 1f, Random.Range(0.8f, 1f));
        }
    }
}
