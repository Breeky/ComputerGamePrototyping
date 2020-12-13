using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{   
    
    private GameObject _projectileRemover;
    

    void Start() {
        _projectileRemover = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().projectileRemover;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {   

        if (other.CompareTag("Player"))
        {   
            // Throw picked projectile
            other.GetComponent<PlayerController>().ThrowProjectile(this);

            // Enable remover collider to remove all other projectiles on the same line
            _projectileRemover.GetComponent<Collider2D>().enabled = true;

            // Destroy initial gameObject
            // TODO: Insert animation of destruction here
            Destroy(gameObject);                    
            
        }
    }
    
}
