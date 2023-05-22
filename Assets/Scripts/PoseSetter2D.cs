using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.Visualizations;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARPlaneManager))]
public class PoseSetter2D: MonoBehaviour
{
    public enum State 
    {
        Off,
        SelectingPosition,
        SelectingRotation
    }

    private const TrackableType trackableTypes = TrackableType.FeaturePoint | TrackableType.PlaneWithinPolygon;
    private const float floorOffset = 0.01f;
    private GameObject target2d;
    private GameObject arrow;

    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;
    private static readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private Vector2 raycastPosition;
    private Drawing3d arrowDrawing;

    private State state = State.Off;

    void Awake()
    {
        target2d = Instantiate(Resources.Load("Prefabs/target2d") as GameObject,  
            Vector3.zero, Quaternion.identity);
        arrow = new GameObject();
    }

    void Start()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();

        Camera camera = Camera.main;
        raycastPosition = new Vector2(camera.pixelWidth / 2, camera.pixelHeight / 2);  // Center of Screen
        arrowDrawing = Drawing3dManager.CreateDrawing(-1, Resources.Load("ArrowMaterial") as Material);
        arrowDrawing.transform.SetParent(arrow.transform);
        arrowDrawing.DrawArrow(arrow.transform.position, arrow.transform.position + Vector3.forward * 0.3f, Color.white, 0.02f, 1.5f, 0.5f);
        Array.ForEach(arrow.GetComponentsInChildren<MeshRenderer>(), x => x.enabled = false);
    }

    void Update()
    {
        switch(state)
        {
            case State.Off:
                raycastManager.enabled = false;
                planeManager.enabled = false;
                break;
            case State.SelectingPosition:
                raycastManager.enabled = true;
                planeManager.enabled = true;
                PerformRaycast();
                break;
            case State.SelectingRotation:
                PerformRaycast();
                SetArrowDirection();
                break;
        }
    }

    public void SetState(State state)
    {
        this.state = state;
        switch(state)
        {
            case State.Off:
                EnableTarget(false);
                EnableArrow(false);
                break;
            case State.SelectingPosition:
                EnableTarget(true);
                EnableArrow(false);
                break;
            case State.SelectingRotation:
                EnableArrow(true);
                // Disable outside of target
                target2d.transform.Find("outer").GetComponent<MeshRenderer>().enabled = false;
                break;
        }
    }

    public (Vector3, float) GetTargetTransform()
    {
        Vector3 heading = arrow.transform.forward;
        Vector2 heading2d = new Vector2(heading.x, heading.z); 
        var direction = Vector2.SignedAngle(heading2d, Vector2.up);
        return (arrow.transform.position, direction);
    }

    private void EnableTarget(bool enabled)
    {
        Array.ForEach(target2d.GetComponentsInChildren<MeshRenderer>(), x => x.enabled = enabled);
    }

    private void EnableArrow(bool enabled)
    {
        if (enabled)
        {
            arrow.transform.position = target2d.transform.position;
        }
        Array.ForEach(arrow.GetComponentsInChildren<MeshRenderer>(), x => x.enabled = enabled);
    }

    private void SetArrowDirection()
    {
        // Calculate the direction from the object's position to the target's position
        Vector3 direction = target2d.transform.position - arrow.transform.position;

        // Project the direction onto the XZ plane
        direction.y = 0f;

        // Rotate the object towards the projected direction using LookAt
        if (direction != Vector3.zero)
        {
            arrow.transform.LookAt(arrow.transform.position + direction);
        }
    }

    private void PerformRaycast()
    {
        // Checks if the raycast hit a plane
        if (raycastManager.Raycast(raycastPosition, hits, trackableTypes))
        {
            // Raycast hits are sorted by distance, so the first one will be the closest hit.
            foreach (var hit in hits)
            {
                if (hit.trackable != null && hit.trackable.GetComponent<ARPlane>() != null)
                {
                    Pose hitPose = hit.pose;
                    hitPose.position = hitPose.position + Vector3.up * floorOffset;

                    target2d.transform.position = hitPose.position;
                    target2d.transform.rotation = hitPose.rotation;
                    return;
                }
            }
        }
    }
}
