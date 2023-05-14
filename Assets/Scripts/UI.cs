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
        NavGoal,
    }
    public Color highlightColor;
    public Color normalColor = Color.white;
    public Button qrCodeButton;
    public Button localizeButton;
    public Button navGoalButton;
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

    private void Start()
    {
        rosInterface = ROSInterface.GetOrCreateInstance();
        menuUI = FindObjectOfType<MenuUI>();
        stateButtons = new List<Button>() {qrCodeButton, localizeButton, navGoalButton};
        qrCodeButton.onClick.AddListener(OnQRCodeButtonClick);
        localizeButton.onClick.AddListener(OnLocalizeButtonClick);
        navGoalButton.onClick.AddListener(OnNavGoalButtonClick);
        menuButton.onClick.AddListener(OnMenuButtonClick);

        actionButton.ButtonPressedEvent += OnActionButtonClick;
        actionButton.normalColor = normalColor;
        actionButton.highlightColor = highlightColor;

        planeTarget =  (PlaneTarget)FindObjectOfType(typeof(PlaneTarget));
        // Set the QRCode button as the default highlighted button
        OnQRCodeButtonClick();
    }

    public void OnQRCodeButtonClick()
    {
        state = State.QRCode;
        planeTarget.HideTarget();
        SelectButton(qrCodeButton);
    }

    public void OnLocalizeButtonClick()
    {
        state = State.Localize;
        planeTarget.ShowTarget();
        SelectButton(localizeButton);
    }

    public void OnNavGoalButtonClick()
    {
        planeTarget.ShowTarget();
        SelectButton(navGoalButton);
    }

    public void OnActionButtonClick(bool pressed)
    {
        switch (state)
        {
            case State.View:
                break;
            case State.QRCode:
                GameObject tracker = GameObject.FindGameObjectWithTag("tracker");
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
                    localizer.SetMapTransformWithTracker(tracker, menuUI.qrCodeLink.text, menuUI.baseLink.text);
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
                    localizer.SetMapTransform(position, heading, menuUI.baseLink.text, menuUI.baseLink.text);
                }
                break;
            case State.NavGoal:
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
