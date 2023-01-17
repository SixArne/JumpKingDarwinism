using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    [SerializeField] private float _cameraDisplacement;
    [SerializeField] private DarwinPopulation _population;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_population.BestHeight > 4f)
        {
            transform.position = new Vector3(transform.position.x, _population.BestHeight, transform.position.z);
        }
        
    }
}
