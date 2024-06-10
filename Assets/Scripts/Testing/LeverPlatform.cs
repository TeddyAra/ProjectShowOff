using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeverPlatform : MonoBehaviour
{

    [SerializeField] private GameObject door;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private float maxRotation;
    [SerializeField] private GameObject vine; 


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Levering!"); 
            Destroy(vine); 
            StartCoroutine(DoorRotation()); 
        }
    }

    IEnumerator DoorRotation()
    {
        while (Mathf.Rad2Deg * door.transform.rotation.z > maxRotation)
        {
            Debug.Log(Mathf.Rad2Deg * door.transform.localRotation.z); 
            door.transform.Rotate(Vector3.forward, Time.deltaTime * -rotateSpeed); 
            yield return null;  
        }
    }


}
