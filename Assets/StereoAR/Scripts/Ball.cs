using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public float distMove;

    GameObject goSphere;

    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * distMove;
    }
}
