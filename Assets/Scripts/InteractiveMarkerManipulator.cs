using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveMarkerManipulator : MonoBehaviour
{
    Vector3 raycastLocation = new Vector3(0.5f, 0.5f, 0f);
    bool raycastActive = true;
    InteractiveMarker hoveredMarker = null;
    InteractiveMarker selectedMarker = null;
    Vector3 markerPositionOffset = Vector3.zero;
    Quaternion markerRotationOffset = Quaternion.identity;

    // Update is called once per frame
    void Update()
    {
        if (raycastActive)
        {
            PerformRaycast();
        }
    }

    public bool MarkerHovered()
    {
        return hoveredMarker != null;
    }

    public bool MarkerSelected()
    {
        return selectedMarker != null;
    }

    public void SetRaycastActive(bool active)
    {
        raycastActive = active;
    }

    public void StartMovingMarker(Transform controllerTransform)
    {
        if (hoveredMarker != null)
        {
            selectedMarker = hoveredMarker;
            markerPositionOffset = selectedMarker.transform.position - controllerTransform.position;
            markerRotationOffset = Quaternion.Inverse(controllerTransform.rotation) * selectedMarker.transform.rotation;
        }
    }

    public void MoveMarker(Transform controllerTransform)
    {
        if (selectedMarker != null)
        {
            selectedMarker.Move(controllerTransform.position + markerPositionOffset, controllerTransform.rotation * markerRotationOffset);
        }
    }

    public void StopMovingMarker()
    {
        selectedMarker = null;
    }

    public void PerformRaycast()
    {
        Ray ray = Camera.main.ViewportPointToRay(raycastLocation);
        RaycastHit hit;

        // Perform the Raycast
        if (Physics.Raycast(ray, out hit))
        {
            // Check if the hit GameObject or any of its parents have the "InteractiveMarker" tag
            InteractiveMarker marker = HitObjectHasInteractiveMarkerTag(hit.collider.gameObject);
            if (hoveredMarker != null && marker != null && hoveredMarker != marker && selectedMarker == null)
            {
                
                hoveredMarker.Expand(false);
                hoveredMarker = null;
            }
            if (marker != null && hoveredMarker == null && selectedMarker == null)
            {
                // The hit GameObject or one of its parents is an InteractiveMarker
                Debug.Log("InteractiveMarker hit!");
                marker.Expand(true);
                hoveredMarker = marker;
            }
        }
        else
        {
            if (hoveredMarker != null && selectedMarker != null)
            {
                hoveredMarker.Expand(false);
                hoveredMarker = null;
            }
        }
    }

    private InteractiveMarker HitObjectHasInteractiveMarkerTag(GameObject hitObject)
    {
        Transform parent = hitObject.transform;

        while (parent != null)
        {
            if (parent.CompareTag("InteractiveMarker"))
            {
                return parent.GetComponent<InteractiveMarker>();;
            }
            parent = parent.parent;
        }

        return null;
    }
}
