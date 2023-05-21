using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveMarkerManipulator : MonoBehaviour
{
    public enum State 
    {
        Off,
        Searching,
        MarkerSelected,
        ManipulatePosition,
        ManipulateRotation,
        ManipulateBoth,
    }
    State currentState = State.Off;
    Vector3 raycastLocation = new Vector3(0.5f, 0.5f, 0f);
    InteractiveMarker hoveredMarker = null;
    InteractiveMarker selectedMarker = null;
    Vector3 markerPositionOffset = Vector3.zero;
    Quaternion markerRotationOffset = Quaternion.identity;
    Transform controllerTransform;


    // Update is called once per frame
    void Update()
    {
        if (currentState == State.Searching)
        {
            PerformRaycast();
        }
        else if (currentState == State.MarkerSelected)
        {
            // Probably do nothing
        }
        else if (currentState == State.ManipulatePosition)
        {
            // Only move position of marker
            UpdateMarkerPosition();
        }
        else if (currentState == State.ManipulateRotation)
        {
            // Only rotate marker
            UpdateMarkerRotation();
        }
        else if (currentState == State.ManipulateBoth)
        {
            // Move position and rotation
            UpdateMarkerTransform();
        }
    }

    public void SetControllerTransform(Transform controllerTransform)
    {
        this.controllerTransform = controllerTransform;
    }

    public void SetState(State state)
    {
        if (state == State.Off)
        {
            currentState = state;
            selectedMarker = null;
            if (hoveredMarker != null)
            {
                hoveredMarker.Expand(false);
                hoveredMarker = null;
            }
        }
         if (state == State.Searching)
        {
            currentState = state;
            selectedMarker = null;
        }
        if (state == State.MarkerSelected)
        {
            if (hoveredMarker != null)
            {
                selectedMarker = hoveredMarker;
            }
            else
            {
                currentState = State.Searching;
            }
        }
        else if (state == State.ManipulatePosition || state == State.ManipulateRotation || state == State.ManipulateBoth)
        {
            if (currentState != State.Off && currentState != State.Searching)
            {
                currentState = state;
                markerPositionOffset = selectedMarker.transform.position - controllerTransform.position;
                markerRotationOffset = Quaternion.Inverse(controllerTransform.rotation) * selectedMarker.transform.rotation;
            }
        }
    }

    public bool MarkerHovered()
    {
        return hoveredMarker != null;
    }

    public bool MarkerSelected()
    {
        return currentState != State.Off && currentState != State.Searching;
    }

    public void UpdateMarkerPosition()
    {
        selectedMarker.UpdatePosition(controllerTransform.position + markerPositionOffset);
    }

    public void UpdateMarkerRotation()
    {
        selectedMarker.UpdateRotation(controllerTransform.rotation * markerRotationOffset);
    }

    public void UpdateMarkerTransform()
    {
        selectedMarker.UpdateTransform(controllerTransform.position + markerPositionOffset, 
            controllerTransform.rotation * markerRotationOffset);
    }

    public void PerformRaycast()
    {
        Ray ray = Camera.main.ViewportPointToRay(raycastLocation);
        RaycastHit hit;

        // Perform the Raycast
        Physics.Raycast(ray, out hit);
        InteractiveMarker marker = HitObjectHasInteractiveMarkerTag(hit);

        if (marker != null)
        {
            if (hoveredMarker != null && hoveredMarker != marker)
            {
                hoveredMarker.Expand(false);
                hoveredMarker = null;
            }
            if (hoveredMarker == null)
            {
                marker.Expand(true);
                hoveredMarker = marker;
            }
        }
        else
        {
            if (hoveredMarker != null)
            {
                hoveredMarker.Expand(false);
                hoveredMarker = null;
            }
        }
    }

    private InteractiveMarker HitObjectHasInteractiveMarkerTag(RaycastHit hit)
    {
        if (hit == null)
        {
            return null;
        }

        Transform parent = hit.collider.gameObject.transform;

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
