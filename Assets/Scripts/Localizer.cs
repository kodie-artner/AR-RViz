using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

public class Localizer : MonoBehaviour
{
    public Transform mapTransform;

    public void SetMapTransformWithTracker(GameObject tracker, string link)
    {
        var trackerHeading = GetHeading(tracker.transform.forward);
        SetMapTransform(tracker.transform.position, trackerHeading, link);
    }
    
    // TODO: Clean up function
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

    // public void SetMapTransformWithTracker(GameObject tracker, string link, string baseLink)
    // {
    //     // project on plane
    //     Vector3 heading = Vector3.ProjectOnPlane(tracker.transform.forward, Vector3.up);
    //     // turn into vector2 and rotate 180 deg
    //     Vector2 heading2d = new Vector2(heading.x, heading.z); 
    //     var trackerHeading = Vector2.SignedAngle(heading2d, Vector2.up);
    //     var trackerPosition = tracker.transform.position;
        
    //     SetMapTransform(trackerPosition, trackerHeading, link, baseLink);
    // }
    
    // // TODO: Clean up function
    // public void SetMapTransform(Vector3 position, float heading, string link, string baseLink){
    //     // Assumption: base_link isn't rotate around x,z
    //     // The base link to use for rotations
    //     TFFrame T_map_base = TFSystem.instance.GetTransform(frame_id: baseLink, time: 0);
    //     Debug.Log("base_link position" + T_map_base.translation);

    //     // The link associated with the given position
    //     TFFrame T_map_link = TFSystem.instance.GetTransform(frame_id: link, time: 0);

    //     Debug.Log("tf: " + T_map_link.translation);
        
    //     // Rotation to be set to
    //     Quaternion rotation = Quaternion.Euler(0, heading, 0);
        
    //     mapTransform.position = position - rotation * Quaternion.Inverse(T_map_base.rotation) * T_map_link.translation;
    //     mapTransform.rotation = rotation * Quaternion.Inverse(T_map_base.rotation);
    // }
