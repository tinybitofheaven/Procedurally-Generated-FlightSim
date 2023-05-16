using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Perch : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Invoke("DestroySelf", 2f);
        }
    }

    void DestroySelf()
    {
        Destroy(gameObject);
    }
}
