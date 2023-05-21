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
    public Color highlightedColor;
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

    private Dictionary<State, Button> stateButtons = new Dictionary<State, Button> { 
        {State.QRCode, qrCodeButton}, {State.Localize,localizeButton}, {State.Pose,poseButton}, {State.Manipulate, manipulateButton}};
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
        qrCodeButton.onClick.AddListener(() => OnChangeStateClick(State.QRCode));
        localizeButton.onClick.AddListener(() => OnChangeStateClick(State.Localize));
        poseButton.onClick.AddListener(() => OnChangeStateClick(State.Pose));
        manipulateButton.onClick.AddListener(() => OnChangeStateClick(State.Manipulate));
        menuButton.onClick.AddListener(OnMenuButtonClick);
       
        actionButton.normalColor = normalColor;
        actionButton.highlightedColor = highlightedColor;

        planeTarget = (PlaneTarget)FindObjectOfType(typeof(PlaneTarget));
    }
    
    void Update()
    {
        switch (state)
        {
            case State.View:
                break;
            case State.QRCode:
                UpdateQRCode();
                break;
            case State.Localize:
                UpdateLocalize();
                break;
            case State.Pose:
                UpdatePose();
                break;
            case State.Manipulate:
                UpdateManipulate();
                break;
        }
    }

    void UpdateQRCode()
    {
        // Add this to On QR code state enter and add activation/deactivation of image finder
        GameObject tracker = GameObject.FindGameObjectWithTag("tracker");
        if (tracker == null)
        {
            return;
        }
        float trackerSize = float.Parse(menuUI.qrCodeLength.text) / 100;
        tracker.transform.localScale = new Vector3(trackerSize, 0.01f, trackerSize);

        if (actionButton.state == ButtonPressed.State.onDown)
        {
            // Set tracker active
            tracker.GetComponent<MeshRenderer>().enabled = true;
        }
        else if (actionButton.state == ButtonPressed.State.onUp)
        {
            // Set tracker disabled
            tracker.GetComponent<MeshRenderer>().enabled = false;
            // Set Localizer to current tracker position
            localizer.SetMapTransformWithTracker(tracker, menuUI.qrCodeLink.text);
        }
    }

    void UpdateLocalize()
    {
        if (actionButton.state == ButtonPressed.State.onDown)
        {
            planeTarget.StartSettingDirection();
        }
        else if (actionButton.state == ButtonPressed.State.onUp)
        {
            Vector3 position;
            float heading;
            (position, heading) = planeTarget.EndSettingDirection();
            localizer.SetMapTransform(position, heading, menuUI.baseLink.text);
        }
    }

    void UpdatePose()
    {
         if (actionButton.state == ButtonPressed.State.onDown)
        {
            planeTarget.StartSettingDirection();
        }
        else if (actionButton.state == ButtonPressed.State.onUp)
        {
            Vector3 position;
            float heading;
            (position, heading) = planeTarget.EndSettingDirection();
            rosInterface.PublishPoseStampedMsg(planeTarget.arrow.transform.position, heading, menuUI.baseLink.text, menuUI.poseTopic.text);
        }
    }

    void UpdateManipulate()
    {
        if (markerManipulator.MarkerHovered())
        {
            actionButton.gameObject.SetActive(true);
        }
        else if (!markerManipulator.MarkerSelected())
        {
            actionButton.gameObject.SetActive(false);
        }
        if (actionButton.state == ButtonPressed.State.onDown)
        {
            markerManipulator.StartMovingMarker(Camera.main.transform);
        }
        else if (actionButton.state == ButtonPressed.State.down)
        {
            
            markerManipulator.MoveMarker(Camera.main.transform);
        }
        else if (actionButton.state == ButtonPressed.State.onUp)
        {
            markerManipulator.StopMovingMarker();
        }
    }

    void SwitchedFromState(State state)
    {
        switch (state)
        {
            case State.View:
                break;
            case State.QRCode:
                // TODO turn off image matcher
                actionButton.gameObject.SetActive(false);
                break;
            case State.Localize:
                planeTarget.HideTarget();
                actionButton.gameObject.SetActive(false);
                break;
            case State.Pose:
                planeTarget.HideTarget();
                actionButton.gameObject.SetActive(false);
                break;
            case State.Manipulate:
                crossHair.SetActive(false);
                actionButton.gameObject.SetActive(false);
                markerManipulator.SetState(InteractiveMarkerManipulator.State.Off);
                break;
        }
    }

    void SwitchedToState(State state)
    {
        switch (state)
        {
            case State.View:
                break;
            case State.QRCode:
                actionButton.gameObject.SetActive(true);
                break;
            case State.Localize:
                planeTarget.ShowTarget();
                actionButton.gameObject.SetActive(true);
                break;
            case State.Pose:
                planeTarget.ShowTarget();
                actionButton.gameObject.SetActive(true);
                break;
            case State.Manipulate:
                crossHair.SetActive(true);
                markerManipulator.SetState(InteractiveMarkerManipulator.State.Searching);
                break;
        }
        this.state = state;
    }

    private void OnChangeStateClick(State state)
    {
        ChangeState(state);
        stateButtons[state].OnDeselect(null);
    }

    private void ChangeState(State newSate)
    {
        SwitchedFromState(this.state);
        // Set the current state button to the normal color
        SetButtonsNormalColor(stateButtons[this.state], normalColor);

        // If the newState equals the current state or is, set the newState to View mode
        if (newState == this.state || newState == State.View)
        {
            newState = State.View;   
        }
        // Else set the new state button to the highlighted color
        else
        {
            SetButtonsNormalColor(stateButtons[newState], highlightedColor);
        }
        SwitchedToState(newState);
    }

    public void OnMenuButtonClick()
    {
        if (menuPanel.activeSelf)
        {
            menuPanel.SetActive(false);
        }
        else
        {
            ChangeState(State.View);
            menuPanel.SetActive(true);
        }
    }

    private void SetButtonsNormalColor(Button button, Color color)
    {
        ColorBlock selectedColors = button.colors;

        selectedColors.normalColor = color;
        selectedColors.highlightedColor = color;

        button.colors = selectedColors;
    }
}
