using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileRemover : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D other)
    {   
        if (other.CompareTag("Projectile"))
        {
            if (other.GetComponent<ProjectileController>() != null){
                // Destroy all unpicked projectiles on line
                // TODO: Insert animation of destruction here
                Destroy(other.gameObject);

                // Wait for one second and disabled remover collider
                StartCoroutine("LateCall");   
            }              
        }
    }

    IEnumerator LateCall()
     {  
        yield return new WaitForSeconds(0.5f);   
        this.GetComponent<Collider2D>().enabled = false;           
        
     }

}
