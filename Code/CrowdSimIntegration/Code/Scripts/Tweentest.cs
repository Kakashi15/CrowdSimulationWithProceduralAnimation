using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tweentest : MonoBehaviour
{
    public AnimationCurve tweenCurve;
    public Transform endPos;
    float timer;

    Vector3 startPos;
    public float lerpTime;
    public float speed;
    public float speed2;
    Vector3 initialPos;
    public Vector3 dir;
    // Start is called before the first frame update
    void Start()
    {
        initialPos = transform.position;
        startPos = initialPos;
        dir = endPos.position - transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (timer < lerpTime)
        {
            timer += Time.deltaTime;
            float lerpValue = tweenCurve.Evaluate(timer / lerpTime);
            transform.position = initialPos + (dir * lerpValue);
        }


    }

    public void StartTween()
    {
        initialPos = transform.position;
        dir = endPos.position - transform.position;
        timer = 0;
    }
}
