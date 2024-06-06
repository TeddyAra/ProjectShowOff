using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
    private float timer; 
    void Update()
    {
        timer += Time.deltaTime; 

        if (timer > 3)
        {
            Destroy(gameObject);
        }
    }
}
