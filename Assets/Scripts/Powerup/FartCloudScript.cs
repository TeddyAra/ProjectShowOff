using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FartCloudScript : MonoBehaviour {
    private float stunTime;

    public void ApplyVariables(float stunTime, float fartCloudTime, float startupTime) { 
        this.stunTime = stunTime;
        StartCoroutine(Despawn(startupTime, fartCloudTime));
    }

    IEnumerator Despawn(float startupTime, float fartCloudTime) {
        yield return new WaitForSeconds(startupTime);
        GetComponent<SphereCollider>().enabled = true;

        yield return new WaitForSeconds(fartCloudTime);
        Destroy(gameObject);
    }

    private void OnTriggerStay(Collider other) {
        Debug.Log("Trigger!");
        if (other.tag == "Player") {
            Debug.Log("Player!");
            other.GetComponent<PlayerControllerTestScript>().Stun(stunTime);
        }
    }
}