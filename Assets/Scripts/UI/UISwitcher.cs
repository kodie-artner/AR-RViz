using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;


public class UISwitcher : MonoBehaviour
{
    public Transform hmdCanvasParent;
    public GameObject screenCanvas;
    public GameObject hmdCanvas;

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
        hmdCanvas.SetActive(true);
        hmdCanvas.transform.SetParent(hmdCanvasParent);
        hmdCanvas.transform.localPosition = new Vector3(0.1f, 0, 0);
        hmdCanvas.transform.localRotation = Quaternion.Euler(90, 0, 0);
    }

    void SetupForScreen()
    {
        screenCanvas.SetActive(true);
    }
}

