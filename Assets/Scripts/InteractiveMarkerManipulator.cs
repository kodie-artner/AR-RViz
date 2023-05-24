using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveMarkerManipulator
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

    public InteractiveMarkerManipulator(Transform controllerTransform)
    {
        this.controllerTransform = controllerTransform;
    }

    // Update is called once per frame
    public void Update()
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
        switch (state)
        {
            case State.Off:
                currentState = state;
                selectedMarker = null;
                if (hoveredMarker != null)
                {
                    hoveredMarker.Expand(false);
                    hoveredMarker = null;
                }
                break;
            case State.Searching:
                currentState = state;
                selectedMarker = null;
                break;
            case State.MarkerSelected:
                if (hoveredMarker != null)
                {
                    selectedMarker = hoveredMarker;
                    currentState = state;
                }
                else
                {
                    currentState = State.Searching;
                }
                break;
            case State.ManipulatePosition:
            // continue to ManipulateBoth
            case State.ManipulateRotation:
            // continue to ManipulateBoth
            case State.ManipulateBoth:
                if (currentState != State.Off && currentState != State.Searching)
                {
                    currentState = state;
                    markerPositionOffset =
                        selectedMarker.transform.position - controllerTransform.position;
                    markerRotationOffset =
                        Quaternion.Inverse(controllerTransform.rotation)
                        * selectedMarker.transform.rotation;
                }
                break;
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
        selectedMarker.UpdateTransform(
            controllerTransform.position + markerPositionOffset,
            controllerTransform.rotation * markerRotationOffset
        );
    }

    public void PerformRaycast()
    {
        Ray ray = Camera.main.ViewportPointToRay(raycastLocation);
        RaycastHit hit;
        InteractiveMarker marker;

        // Perform the Raycast
        if (Physics.Raycast(ray, out hit))
        {
            marker = HitObjectHasInteractiveMarkerTag(hit);
        }
        else
        {
            marker = null;
        }

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
        Transform parent = hit.collider.gameObject.transform;

        while (parent != null)
        {
            if (parent.CompareTag("InteractiveMarker"))
            {
                return parent.GetComponent<InteractiveMarker>();
                ;
            }
            parent = parent.parent;
        }

        return null;
    }
}
