using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraResolution : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Camera.main.aspect = 4f / 3;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
