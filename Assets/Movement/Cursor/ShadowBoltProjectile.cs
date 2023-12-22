using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowBoltProjectile : MonoBehaviour
{
    public float speed = 5f;
    private GameObject target;
    private bool targetAssigned = false; 

    // Initialize the shadow bolt with a target
    public void Initialize(GameObject target)
    {
        this.target = target;
        targetAssigned = true; 
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            // Move the shadow bolt towards the target
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);

            // Check if the shadow bolt has reached the target
            if (Vector3.Distance(transform.position, target.transform.position) < 0.1f)
            {
                // Stop rendering the shadow bolt
                GetComponent<SpriteRenderer>().enabled = false;
            }
        }
        else if (targetAssigned) 
        {
            // Destroy the shadow bolt if the target is null and has been assigned before
            Destroy(gameObject);
        }
    }
}
