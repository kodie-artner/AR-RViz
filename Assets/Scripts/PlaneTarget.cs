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
    public GameObject target2d;
    public GameObject arrow;
    public GameObject target3d;
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
    private bool setting3dDirection = false;

    void Awake()
    {
        target2d = Instantiate(Resources.Load("Prefabs/target2d") as GameObject,  
            Vector3.zero, Quaternion.identity);
        target3d = Instantiate(Resources.Load("Prefabs/target3d") as GameObject,  
            Vector3.zero, Quaternion.identity);
            HideTarget(true);
        arrow = new GameObject();
    }

    // Start is called before the first frame update
    void Start()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();
        camera = Camera.main;

        raycastPosition = new Vector2(camera.pixelWidth / 2, camera.pixelHeight / 2);
        drawingManager = gameObject.AddComponent<Drawing3dManager>();
        arrowDrawing = Drawing3dManager.CreateDrawing(-1, Resources.Load("ArrowMaterial") as Material);
        arrowDrawing.transform.SetParent(arrow.transform);
        arrowDrawing.DrawArrow(arrow.transform.position, arrow.transform.position + Vector3.forward * 0.3f, Color.white, 0.02f, 1.5f, 0.5f);
        Array.ForEach(arrow.GetComponentsInChildren<MeshRenderer>(), x => x.enabled = false);

        target3d.transform.SetParent(camera.transform);
        target3d.transform.localPosition = Vector3.forward;
    }

    // Update is called once per frame
    void Update()
    {
        DetectRaycastHit();
        if (settingDirection)
        {
            SetDirection();
        }
        else if (setting3dDirection)
        {
            Set3dDirection();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public void ShowTarget(bool is3d=false)
    {
        var target = is3d ? target3d : target2d;
        Array.ForEach(target.GetComponentsInChildren<MeshRenderer>(), x => x.enabled = true);
    }
    
    public void HideTarget(bool is3d=false)
    {
        var target = is3d ? target3d : target2d;
        Array.ForEach(target2d.GetComponentsInChildren<MeshRenderer>(), x => x.enabled = false);
    }


    public void StartSetting3dDirection()
    {
        Debug.Log("start 3d direction");
        arrow.transform.position = target3d.transform.position;
        Array.ForEach(arrow.GetComponentsInChildren<MeshRenderer>(), x => x.enabled = true);
        setting3dDirection = true;
    }

    public (Vector3, Quaternion) EndSetting3dDirection()
    {
        Debug.Log("end 3d direction");
        Array.ForEach(arrow.GetComponentsInChildren<MeshRenderer>(), x => x.enabled = false);
        setting3dDirection = false;
        return (target3d.transform.position, target3d.transform.rotation);
    }

    public void StartSettingDirection()
    {
        target2d.transform.Find("outer").GetComponent<MeshRenderer>().enabled = false;
        arrow.transform.position = target2d.transform.position;
        Array.ForEach(arrow.GetComponentsInChildren<MeshRenderer>(), x => x.enabled = true);
        settingDirection = true;
    }
    public (Vector3, float) EndSettingDirection()
    {
        target2d.transform.Find("outer").GetComponent<MeshRenderer>().enabled = true;
        Vector3 heading = arrow.transform.forward;
        Vector2 heading2d = new Vector2(heading.x, heading.z); 
        var direction = Vector2.SignedAngle(heading2d, Vector2.up);
        Array.ForEach(arrow.GetComponentsInChildren<MeshRenderer>(), x => x.enabled = false);
        settingDirection = false;
        return (arrow.transform.position, direction);
    }

    public void SetDirection()
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

    public void Set3dDirection()
    {
        Debug.Log("setting 3d direction");
        arrow.transform.LookAt(target3d.transform);
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

                    target2d.transform.position = hitPose.position;
                    target2d.transform.rotation = hitPose.rotation;
                    
                    lastHit = hit;
                    break;
                }
            }
        }
    }
}
