using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class LeverPlatform : MonoBehaviour {
    [SerializeField] private GameObject door;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private float maxRotation;
    [SerializeField] private GameObject vine; 

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Player")) {
            Destroy(vine); 
            Collider collider = gameObject.GetComponent<Collider>();
            collider.enabled = false;
            StartCoroutine(DoorRotation()); 
        }
    }

    IEnumerator DoorRotation() {
        while (Mathf.Rad2Deg * door.transform.rotation.z > maxRotation) {
            door.transform.Rotate(Vector3.forward, Time.deltaTime * -rotateSpeed); 
            yield return null;  
        }
    }
}