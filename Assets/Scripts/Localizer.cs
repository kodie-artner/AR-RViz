using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

// Localizer is responsible for handling the localization of the mobile device to the ROS coordinate system.
public class Localizer
{
    public Transform mapTransform;

    public Localizer()
    {
        mapTransform = GameObject.FindGameObjectWithTag("Map").transform;
    }

    public void SetMapTransformWithTracker(GameObject tracker, string link)
    {
        var trackerHeading = GetHeading(tracker.transform.forward);
        SetMapTransform(tracker.transform.position, trackerHeading, link);
    }

    public void SetMapTransform(Vector3 targetPosition, float targetHeading, string link)
    {
        // The link associated with the given position
        TFFrame T_map_link = TFSystem.instance.GetTransform(frame_id: link, time: 0);

        // Get heading of the chosen link in the map frame
        float link_heading = GetHeading(T_map_link.rotation * Vector3.forward);
        Quaternion linkRotation = Quaternion.Euler(0, link_heading, 0);

        // Convert targetHeading into quaternion
        Quaternion rotation = Quaternion.Euler(0, targetHeading, 0);
        mapTransform.position = targetPosition - rotation * Quaternion.Inverse(linkRotation) * T_map_link.translation;
        mapTransform.rotation = rotation * Quaternion.Inverse(linkRotation);
    }

    private float GetHeading(Vector3 forwardDirection)
    {
        // Project on x,z plane
        Vector3 heading = Vector3.ProjectOnPlane(forwardDirection, Vector3.up);
        // Get angle between heading and Vector2.up which is the vector (0, 1) which corresponds to
        // out zero angle
        return Vector2.SignedAngle(new Vector2(heading.x, heading.z), Vector2.up);
    }
}
