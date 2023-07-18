using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float rotateSpeed = 10f;
    public float centerPointY = 0f;
    // Start is called before the first frame update
    void Start()
    {
        
    
    }

    // Update is called once per frame
    void Update()
    {
        float xPress = Input.GetAxis("Horizontal");


        transform.RotateAround(Vector3.zero, Vector3.up, rotateSpeed * xPress * Time.deltaTime);
        transform.LookAt(Vector3.up * centerPointY);
            
    }
}
