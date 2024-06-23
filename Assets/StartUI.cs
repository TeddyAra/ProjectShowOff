using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartUI : MonoBehaviour
{
    [SerializeField] private float sizeChange;
    [SerializeField] private float sizeChangeSpeed;
    private Vector3 startSize;


    private void Start()
    {
        startSize = transform.localScale;
    }
    private void FixedUpdate()
    {
        float newSizeY = startSize.x + sizeChange * Mathf.Sin(sizeChangeSpeed * Time.time);
        transform.localScale = new Vector3(newSizeY, newSizeY, newSizeY);

    }

}
