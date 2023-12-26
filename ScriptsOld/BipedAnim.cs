using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BipedAnim : MonoBehaviour
{
    public Transform root;
    public Transform[] feet;

    public float rootToFootOffset = 1f;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Mathf.Abs(root.transform.position.y - feet[0].position.y) < rootToFootOffset)
        {
            root.transform.position = new Vector3(root.transform.position.x,
                                                  root.transform.position.y + rootToFootOffset * Time.deltaTime,
                                                  root.transform.position.z);
        }
    }
}
