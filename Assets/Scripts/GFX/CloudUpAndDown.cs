using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Animator))]
public class CloudUpAndDown : MonoBehaviour
{
    public float minSpeed = 0.95f;
    public float maxSpeed = 1.05f;
    public float delay = 0f;
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("Delay");
    }
    
    IEnumerator Delay() 
    {
        yield return new WaitForSeconds(delay);
        Animator animator = GetComponent<Animator>();
        animator.SetFloat("Speed", Random.Range(minSpeed, maxSpeed));
        animator.SetTrigger("Start");
    }
}
