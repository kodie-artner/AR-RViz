using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.Visualizations;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARPlaneManager))]
public class PlaneTarget : MonoBehaviour
{
    // Constants
    private const TrackableType trackableTypes = TrackableType.FeaturePoint | TrackableType.PlaneWithinPolygon;

    // Member variables
    public float floorOffset = 0.01f;
    public GameObject anchorTarget;
    public GameObject arrow;
    private Camera camera;

    // Raycasts
    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;
    private static readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private ARRaycastHit lastHit;
    private Pose hitPose;
    private Vector2 raycastPosition;
    private Drawing3dManager drawingManager;
    private Drawing3d arrowDrawing;

    // State variables
    private bool settingDirection = false;

    // Start is called before the first frame update
    void Start()
    {
        anchorTarget = Instantiate(Resources.Load("Prefabs/AnchorTarget") as GameObject,  
            Vector3.zero, Quaternion.identity);
        arrow = new GameObject();

        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();
        camera = Camera.main;

        raycastPosition = new Vector2(camera.pixelWidth / 2, camera.pixelHeight / 3);
        drawingManager = gameObject.AddComponent<Drawing3dManager>();
        arrowDrawing = Drawing3dManager.CreateDrawing(-1, Resources.Load("ArrowMaterial") as Material);
        arrowDrawing.transform.SetParent(arrow.transform);
        arrowDrawing.DrawArrow(arrow.transform.position, arrow.transform.position + Vector3.forward * 0.3f, Color.white, 0.02f, 1.5f, 0.5f);
        Array.ForEach(arrow.GetComponentsInChildren<MeshRenderer>(), x => x.enabled = false);
    }

    // Update is called once per frame
    void Update()
    {
        DetectRaycastHit();

        if (settingDirection)
        {
            SetDirection();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public void ShowTarget()
    {
        Array.ForEach(anchorTarget.GetComponentsInChildren<MeshRenderer>(), x => x.enabled = true);
    }
    
    public void HideTarget()
    {
        Array.ForEach(anchorTarget.GetComponentsInChildren<MeshRenderer>(), x => x.enabled = false);
    }

    public void StartSettingDirection()
    {
        arrow.transform.position = anchorTarget.transform.position;
        Array.ForEach(arrow.GetComponentsInChildren<MeshRenderer>(), x => x.enabled = true);
        settingDirection = true;
    }
    public (Vector3, float) EndSettingDirection()
    {
        Vector3 heading = arrow.transform.forward;
        Vector2 heading2d = new Vector2(heading.x, heading.z); 
        var direction = Vector2.SignedAngle(heading2d, Vector2.up);
        Array.ForEach(arrow.GetComponentsInChildren<MeshRenderer>(), x => x.enabled = false);
        settingDirection = false;
        return (arrow.transform.position, direction);
    }

    public void SetDirection()
    {
        arrow.transform.LookAt(anchorTarget.transform);
    }

    // Detects raycast hit a plane
    private void DetectRaycastHit()
    {
        if (raycastManager.Raycast(raycastPosition, hits, trackableTypes))
        {
            // Raycast hits are sorted by distance, so the first one will be the closest hit.
            foreach (var hit in hits)
            {
                if (hit.trackable != null && hit.trackable.GetComponent<ARPlane>() != null)
                {
                    // To check if floor: hit.trackable.GetComponent<ARPlane>().classification != PlaneClassification.Floor)

                    hitPose = hit.pose;
                    // TODO: Update to handle vertical planes
                    hitPose.position = hitPose.position + Vector3.up * floorOffset;

                    anchorTarget.transform.position = hitPose.position;
                    anchorTarget.transform.rotation = hitPose.rotation;
                    
                    lastHit = hit;
                    break;
                }
            }
        }
    }
}
