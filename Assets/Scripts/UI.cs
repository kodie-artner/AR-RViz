using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UI : MonoBehaviour
{
    private enum State
    {
        View,
        QRCode,
        Localize,
        Pose,
        Manipulate,
    }
    public Color highlightColor;
    public Color normalColor = Color.white;
    public Button qrCodeButton;
    public Button localizeButton;
    public Button poseButton;
    public Button manipulateButton;
    public GameObject crossHair;
    public ButtonPressed actionButton;
    public Button menuButton;
    public GameObject menuPanel;

    private State state = State.View;
    private bool settingDirection = false;
    private PlaneTarget planeTarget;

    private List<Button> stateButtons;
    private Button currentlyHighlightedButton;
    ROSInterface rosInterface;
    MenuUI menuUI;
    Localizer localizer;
    InteractiveMarkerManipulator markerManipulator;

    void Start()
    {
        rosInterface = ROSInterface.GetOrCreateInstance();
        localizer = FindObjectOfType<Localizer>();
        menuUI = FindObjectOfType<MenuUI>();
        markerManipulator = FindObjectOfType<InteractiveMarkerManipulator>();
        if (menuUI == null)
        {
            throw new NullReferenceException("menuUI object is null.");
        }
        if (localizer == null)
        {
            throw new NullReferenceException("localizer object is null.");
        }
        if (markerManipulator == null)
        {
            throw new NullReferenceException("Marker Manipulator object is null.");
        }
        stateButtons = new List<Button>() { qrCodeButton, localizeButton, poseButton, manipulateButton};
        qrCodeButton.onClick.AddListener(OnQRCodeButtonClick);
        localizeButton.onClick.AddListener(OnLocalizeButtonClick);
        poseButton.onClick.AddListener(OnPoseButtonClick);
        manipulateButton.onClick.AddListener(OnManipulateButtonClick);
        menuButton.onClick.AddListener(OnMenuButtonClick);

        actionButton.ButtonPressedEvent += OnActionButtonClick;
        actionButton.normalColor = normalColor;
        actionButton.highlightColor = highlightColor;

        planeTarget = (PlaneTarget)FindObjectOfType(typeof(PlaneTarget));
        // Set the QRCode button as the default highlighted button
        OnQRCodeButtonClick();
    }

    void Update()
    {
        switch (state)
        {
            case State.Manipulate:
                if (markerManipulator.MarkerSelected())
                {
                    markerManipulator.MoveMarker(Camera.main.transform);
                }
                if (markerManipulator.MarkerHovered())
                {
                    actionButton.gameObject.SetActive(true);
                }
                else if (!markerManipulator.MarkerSelected())
                {
                    actionButton.gameObject.SetActive(false);
                }
                break;
            default:
                break;
        }
    }

    public void OnQRCodeButtonClick()
    {
        state = State.QRCode;
        planeTarget.HideTarget();
        crossHair.SetActive(false);
        SelectButton(qrCodeButton);
    }

    public void OnLocalizeButtonClick()
    {
        state = State.Localize;
        planeTarget.ShowTarget();
        crossHair.SetActive(false);
        SelectButton(localizeButton);
    }

    public void OnPoseButtonClick()
    {
        state = State.Pose;
        planeTarget.ShowTarget();
        crossHair.SetActive(false);
        SelectButton(poseButton);
    }

    public void OnManipulateButtonClick()
    {
        state = State.Manipulate;
        planeTarget.HideTarget();
        crossHair.SetActive(true);
        SelectButton(manipulateButton);
        markerManipulator.SetRaycastActive(state == State.Manipulate);
    }

    public void OnActionButtonClick(bool pressed)
    {
        switch (state)
        {
            case State.View:
                break;
            case State.QRCode:
                GameObject tracker = GameObject.FindGameObjectWithTag("tracker");
                if (tracker == null)
                {
                    return;
                }

                float trackerSize = float.Parse(menuUI.qrCodeLength.text) / 100;
                tracker.transform.localScale = new Vector3(trackerSize, 0.01f, trackerSize);
                if (pressed)
                {
                    // Set tracker active
                    tracker.GetComponent<MeshRenderer>().enabled = true;
                }
                else
                {
                    // Set tracker disabled
                    tracker.GetComponent<MeshRenderer>().enabled = false;
                    // Set Localizer to current tracker position
                    localizer.SetMapTransformWithTracker(tracker, menuUI.qrCodeLink.text);
                }
                break;
            case State.Localize:
                if (pressed)
                {
                    planeTarget.StartSettingDirection();
                }
                else
                {
                    Vector3 position;
                    float heading;
                    (position, heading) = planeTarget.EndSettingDirection();
                    localizer.SetMapTransform(position, heading, menuUI.baseLink.text);
                }
                break;
            case State.Pose:
                if (pressed)
                {
                    planeTarget.StartSettingDirection();
                }
                else
                {
                    Vector3 position;
                    float heading;
                    (position, heading) = planeTarget.EndSettingDirection();
                    rosInterface.PublishPoseStampedMsg(planeTarget.arrow.transform.position, heading, menuUI.baseLink.text, menuUI.poseTopic.text);
                }
                break;
            case State.Manipulate:
                if (pressed)
                {
                    Debug.Log("pressed");
                    if (!markerManipulator.MarkerSelected())
                    {
                        Debug.Log("start");
                        markerManipulator.StartMovingMarker(Camera.main.transform);
                    }
                }
                else
                {
                    Debug.Log("stopped");
                    markerManipulator.StopMovingMarker();
                }
                break;
        }
    }

    public void OnMenuButtonClick()
    {
        if (menuPanel.activeSelf)
        {
            menuPanel.SetActive(false);
        }
        else
        {
            menuPanel.SetActive(true);
        }
    }

    private void SelectButton(Button selectedButton)
    {
        foreach (Button button in stateButtons)
        {
            if (button != selectedButton)
            {
                // Reset the button color to its default state
                SetButtonNormalColor(button, normalColor);
            }
            else
            {
                if (selectedButton == currentlyHighlightedButton)
                {
                    // Reset the button color to its default state
                    SetButtonNormalColor(button, normalColor);
                    currentlyHighlightedButton = null;
                    // Disable action button
                    actionButton.gameObject.SetActive(false);
                    planeTarget.HideTarget();
                    crossHair.SetActive(false);
                    state = State.View;
                }
                else
                {
                    // Highlight the new button
                    SetButtonNormalColor(button, highlightColor);
                    currentlyHighlightedButton = selectedButton;
                    // Ensure action button is enabled
                    actionButton.gameObject.SetActive(true);
                }
            }
        }
        selectedButton.OnDeselect(null);
    }

    private void SetButtonNormalColor(Button button, Color color)
    {
        ColorBlock selectedColors = button.colors;

        selectedColors.normalColor = color;
        selectedColors.highlightedColor = color;

        button.colors = selectedColors;
    }
}
