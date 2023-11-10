using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class MoveInteractor : MonoBehaviour
{
    public InputAction joystickAction;
    public InputAction TriggerAction;
    private Transform currentObject;
    private float moveSpeed = 0.5f;
    void OnEnable()
    {
        joystickAction.performed += ctx => Debug.Log("Value: " + ctx.ReadValue<Vector2>());
        joystickAction.Enable();

    }

    void Update()
    {
        if (currentObject != null)
        {
            var position = joystickAction.ReadValue<Vector2>();

            if (position.y != 0)
            {
                float moveDistance = -position.y * Time.deltaTime * moveSpeed;
                currentObject.Translate(Vector3.forward * moveDistance, Space.Self);
            }
        }
    }

    public void OnSelectEntering(SelectEnterEventArgs args)
    {
        currentObject = args.interactable.transform;
        currentObject.SetParent(transform);
    }

   public void OnSelectExiting(SelectExitEventArgs args)
    {
        currentObject = args.interactable.transform;
        currentObject.SetParent(null);
    }
}
