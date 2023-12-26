using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sachin
{
    public class PlayerController : MonoBehaviour
    {
        public float horizontalInput;
        public float verticalInput;
        public Camera mainCamera;
        public float speed;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");

            Vector3 modifiedPosition = transform.position;
            modifiedPosition.x += horizontalInput * speed * Time.deltaTime;
            modifiedPosition.z += verticalInput * speed * Time.deltaTime;
            Vector3 moveDir = new Vector3(horizontalInput, 0, verticalInput) + mainCamera.transform.forward;
            //transform.LookAt(moveDir);
            transform.position = modifiedPosition;

        }
    }
}