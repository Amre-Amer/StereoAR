using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightCamera : MonoBehaviour
{
    public Camera camAR;
    Camera camRight;

    // Start is called before the first frame update
    void Start()
    {
        camAR = GameObject.Find("ARCamera").GetComponent<Camera>();
        camRight = GetComponent<Camera>();
        InvokeRepeating("UpdateCam", 1, 1);  
    }

    // Update is called once per frame
    void UpdateCam()
    {
        camRight.fieldOfView = camAR.fieldOfView;
        //camRight.transform.eulerAngles = camAR.transform.eulerAngles;
        //camRight.transform.position = camAR.transform.position + camAR.transform.right * .06f;
    }
}
