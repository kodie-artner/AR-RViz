using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointTowards : MonoBehaviour
{
    public Transform target;
    // public float minAngleDifference = 10.0f; // Minimum angle difference to trigger rotation
    public float rotationSpeed = 5;

    void Update()
    {
        if (target != null)
        {
            Vector3 targetPosition = target.position;
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);

                float angleDifference = Quaternion.Angle(transform.rotation, toRotation);

                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, Time.deltaTime * rotationSpeed);
                // if (angleDifference > minAngleDifference)
                // {
                //     transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, Time.deltaTime * rotationSpeed);
                // }
                // else
                // {
                //     transform.LookAt(target);
                // }
            }
        }
    }
}

