using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralAnimationTest : MonoBehaviour
{
    public float speed = 1.0f;
    public float stepHeight = 0.2f;

    public Transform[] bones;

    private void Start()
    {
        //bones = GetComponentsInChildren<Transform>(true);
    }

    private void Update()
    {
        // Update the position of the feet.
        Vector3 footPosition = transform.position + (Vector3.forward * speed);
        bones[0].position = footPosition;
        bones[1].position = footPosition + (Vector3.down * stepHeight);

        // Update the position of the rest of the bones.
        for (int i = 2; i < bones.Length; i++)
        {
            bones[i].position = Vector3.Lerp(bones[i - 2].position, bones[i - 1].position, 0.5f);
        }
    }
}
