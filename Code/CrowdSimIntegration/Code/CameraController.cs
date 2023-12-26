using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float movementSpeed = 5.0f;
    public float rotationSpeed = 2.0f;

    void Update()
    {
        // Translation (movement) controls
        float horizontalMovement = Input.GetAxis("Horizontal");
        float verticalMovement = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontalMovement, 0, verticalMovement);
        moveDirection = Quaternion.Euler(0, transform.eulerAngles.y, 0) * moveDirection;
        transform.position += moveDirection * movementSpeed * Time.deltaTime;

        Camera.main.fieldOfView += Input.GetAxis("Horizontal2");
        Camera.main.fieldOfView -= Input.GetAxis("Vertical2");

        // Rotation controls
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if(Input.GetKey(KeyCode.LeftControl))
        {
            Vector3 rotation = new Vector3(-mouseY, mouseX, 0) * rotationSpeed;
            transform.eulerAngles += rotation;
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            Vector3 rotation = new Vector3(0, mouseX, 0) * rotationSpeed;
            transform.eulerAngles += rotation;
        }

    }
}
