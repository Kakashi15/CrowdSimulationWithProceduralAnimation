using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Player movement script

    public float MoveSpeed = 3.8f;
    public float RotSpeed = 80.0f;
    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Handle keyboard control
        // This loop competes with AdjustBodyTransform() in LegController script to properly postion the body transform

        float ws = Input.GetAxis("Vertical");
        //transform.Translate(0, 0, ws);

        float ad = Input.GetAxis("Horizontal");
        //transform.Translate(ad, 0, 0);

        Vector3 movdeDir = new Vector3(ad, 0f, ws);
        rb.AddForce(transform.GetChild(0).forward * ws * MoveSpeed * Time.deltaTime, ForceMode.Impulse);


        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(0, -RotSpeed * Time.deltaTime, 0);
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(0, RotSpeed * Time.deltaTime, 0);
        }
    }
}
