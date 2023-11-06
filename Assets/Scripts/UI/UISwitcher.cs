using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;


public class UISwitcher : MonoBehaviour
{
    public Canvas mainCanvas;
    public Transform hmdCanvasParent;

    void Start()
    {
        if (MenuLoader.IsHmdDevice())
        {
            SetupForHmd();
        }
        else
        {
            SetupForScreen();
        }
    }

    void SetupForHmd()
    {
        mainCanvas.transform.SetParent(hmdCanvasParent);
        // Set to world space

        // Update dimensions. Or just create a ui from

    }

    void SetupForScreen()
    {

    }
}

