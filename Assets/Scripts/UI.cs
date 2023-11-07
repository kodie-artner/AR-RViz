using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// The UI class handles the main UI logic of the application.
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

    // Color Pallet
    // Mint: 4fb286 : (79, 178, 134)
    // Gunmetal: 143642 : (20, 54, 66)
    // Anit-flash White : ebebeb : (235, 235, 235)
    // Cool Gray: 7e7f9a : (126, 127, 154)
    // Bittersweet: f87666 : (248, 118, 102)
    public Color highlightedColor = new Color(79, 178, 134);
    public Color normalColor = new Color(235, 235, 235);
    public Button qrCodeButton;
    public Button localizeButton;
    public Button poseButton;
    public Button manipulateButton;
    public GameObject crossHair;
    public ButtonPressed actionButton;
    public Button menuButton;
    public GameObject menuPanel;
    public CanvasGroup canvasGroup;

    private State state = State.View;

    private Dictionary<State, Button> stateButtons;
    private Button currentlyHighlightedButton;
    private ROSInterface rosInterface;
    private MenuUI menuUI;
    private Localizer localizer;
    private PoseSetter2D poseSetter2D;
    private InteractiveMarkerManipulator markerManipulator;
    private bool menuActive;
    private bool canvasActive;

    void Start()
    {
        stateButtons = new Dictionary<State, Button>
        {
            { State.QRCode, qrCodeButton },
            { State.Localize, localizeButton },
            { State.Pose, poseButton },
            { State.Manipulate, manipulateButton }
        };

        rosInterface = ROSInterface.GetOrCreateInstance();
        localizer = new Localizer();
        menuUI = FindObjectOfType<MenuUI>();
        poseSetter2D = FindObjectOfType<PoseSetter2D>();
        markerManipulator = new InteractiveMarkerManipulator(Camera.main.transform);

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
            throw new NullReferenceException("markerManipulator object is null.");
        }
        if (poseSetter2D == null)
        {
            throw new NullReferenceException("poseSetter2D object is null.");
        }

        qrCodeButton.onClick.AddListener(() => OnChangeStateClick(State.QRCode));
        localizeButton.onClick.AddListener(() => OnChangeStateClick(State.Localize));
        poseButton.onClick.AddListener(() => OnChangeStateClick(State.Pose));
        manipulateButton.onClick.AddListener(() => OnChangeStateClick(State.Manipulate));
        menuButton.onClick.AddListener(OnMenuButtonClick);

        actionButton.normalColor = normalColor;
        actionButton.highlightedColor = highlightedColor;

        menuEnabled(false);
        // Start in QR Code Mode
        ChangeState(State.QRCode);
    }

    void Update()
    {
        if (!canvasActive) return;

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

        // Code for shutting down app
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void SwitchedFromState(State state)
    {
        switch (state)
        {
            case State.View:
                break;
            case State.QRCode:
                menuUI.trackedImageManager.enabled = false;
                actionButton.gameObject.SetActive(false);
                break;
            case State.Localize:
                poseSetter2D.SetState(PoseSetter2D.State.Off);
                actionButton.gameObject.SetActive(false);
                break;
            case State.Pose:
                poseSetter2D.SetState(PoseSetter2D.State.Off);
                actionButton.gameObject.SetActive(false);
                break;
            case State.Manipulate:
                crossHair.SetActive(false);
                actionButton.gameObject.SetActive(false);
                markerManipulator.SetState(InteractiveMarkerManipulator.State.Off);
                break;
        }
    }

    private void SwitchedToState(State state)
    {
        switch (state)
        {
            case State.View:
                break;
            case State.QRCode:
                menuUI.trackedImageManager.enabled = true;
                actionButton.gameObject.SetActive(true);
                break;
            case State.Localize:
                poseSetter2D.SetState(PoseSetter2D.State.SelectingPosition);
                actionButton.gameObject.SetActive(true);
                break;
            case State.Pose:
                poseSetter2D.SetState(PoseSetter2D.State.SelectingPosition);
                actionButton.gameObject.SetActive(true);
                break;
            case State.Manipulate:
                crossHair.SetActive(true);
                markerManipulator.SetState(InteractiveMarkerManipulator.State.Searching);
                break;
        }
        this.state = state;
    }

    private void UpdateQRCode()
    {
        ButtonPressed.State actionButtonState = actionButton.GetState();
        // Add this to On QR code state enter and add activation/deactivation of image finder
        GameObject tracker = GameObject.FindGameObjectWithTag("Tracker");
        if (tracker == null)
        {
            return;
        }
        float trackerSize = float.Parse(menuUI.qrCodeLength.text) / 100;
        tracker.transform.localScale = new Vector3(trackerSize, 0.01f, trackerSize);

        if (actionButtonState == ButtonPressed.State.OnDown)
        {
            // Set tracker active
            tracker.GetComponent<MeshRenderer>().enabled = true;
        }
        else if (actionButtonState == ButtonPressed.State.OnUp)
        {
            // Set tracker disabled
            tracker.GetComponent<MeshRenderer>().enabled = false;
            // Set Localizer to current tracker position
            localizer.SetMapTransformWithTracker(tracker, menuUI.qrCodeLink);
        }
    }

    private void UpdateLocalize()
    {
        ButtonPressed.State actionButtonState = actionButton.GetState();
        if (actionButtonState == ButtonPressed.State.OnDown)
        {
            poseSetter2D.SetState(PoseSetter2D.State.SelectingRotation);
        }
        else if (actionButtonState == ButtonPressed.State.OnUp)
        {
            (Vector3 position, float heading) = poseSetter2D.GetTargetTransform();
            poseSetter2D.SetState(PoseSetter2D.State.SelectingPosition);
            if (menuUI.baseLink != "")
            {
                localizer.SetMapTransform(position, heading, menuUI.baseLink);
            }
        }
    }

    private void UpdatePose()
    {
        ButtonPressed.State actionButtonState = actionButton.GetState();
        if (actionButtonState == ButtonPressed.State.OnDown)
        {
            poseSetter2D.SetState(PoseSetter2D.State.SelectingRotation);
        }
        else if (actionButtonState == ButtonPressed.State.OnUp)
        {
            (Vector3 position, float heading) = poseSetter2D.GetTargetTransform();
            poseSetter2D.SetState(PoseSetter2D.State.SelectingPosition);
            // Need to convert position and heading to proper transform
            if (menuUI.poseTopic.text != "" && menuUI.poseLink != "")
            {
                rosInterface.PublishPoseStampedMsg(position, heading, menuUI.poseLink, menuUI.poseTopic.text);
            }
        }
    }

    private void UpdateManipulate()
    {
        ButtonPressed.State actionButtonState = actionButton.GetState();
        markerManipulator.Update();
        if (markerManipulator.MarkerHovered())
        {
            actionButton.gameObject.SetActive(true);
        }
        else if (!markerManipulator.MarkerSelected())
        {
            actionButton.gameObject.SetActive(false);
        }
        if (actionButtonState == ButtonPressed.State.OnDown)
        {
            markerManipulator.SetState(InteractiveMarkerManipulator.State.MarkerSelected);
            markerManipulator.SetState(InteractiveMarkerManipulator.State.ManipulateBoth);
        }
        else if (actionButtonState == ButtonPressed.State.OnUp)
        {
            markerManipulator.SetState(InteractiveMarkerManipulator.State.Searching);
        }
    }

    private void OnChangeStateClick(State state)
    {
        ChangeState(state);
        stateButtons[state].OnDeselect(null);
    }

    private void ChangeState(State newState)
    {
        SwitchedFromState(this.state);
        // Set the current state button to the normal color
        foreach (Button button in stateButtons.Values)
        {
            SetButtonsNormalColor(button, normalColor);
        }

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
        if (menuActive)
        {
            rosInterface.RegisterPoseTopicIfNotRegistered(menuUI.poseTopic.text);
            menuEnabled(false);
            menuActive = false;
        }
        else
        {
            ChangeState(State.View);
            menuUI.UpdateUI();
            menuEnabled(true);
            menuActive = true;
        }
    }

    private void menuEnabled(bool enabled)
    {
        canvasGroup.alpha = enabled ? 1 : 0;
        canvasGroup.interactable = enabled;
        canvasGroup.blocksRaycasts = enabled;
        canvasActive = enabled;
    }

    private void SetButtonsNormalColor(Button button, Color color)
    {
        ColorBlock selectedColors = button.colors;

        selectedColors.normalColor = color;
        selectedColors.highlightedColor = color;

        button.colors = selectedColors;
    }
}
