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
    
    private State state = State.View;
    private bool settingDirection = false;
    private PlaneTarget planeTarget;
    
    private List<Button> stateButtons;
    private Button currentlyHighlightedButton;

    private void Start()
    {
        stateButtons = new List<Button>() {qrCodeButton, localizeButton, navGoalButton};
        qrCodeButton.onClick.AddListener(OnQRCodeButtonClick);
        localizeButton.onClick.AddListener(OnLocalizeButtonClick);
        navGoalButton.onClick.AddListener(OnNavGoalButtonClick);
        menuButton.onClick.AddListener(OnMenuButtonClick);

        actionButton.ButtonPressedEvent += OnActionButtonClick;
        actionButton.normalColor = normalColor;
        actionButton.highlightColor = highlightColor;

        planeTarget =  (PlaneTarget)FindObjectOfType(typeof(PlaneTarget));
        planeTarget.ShowTarget();
        // Set the QRCode button as the default highlighted button
        SelectButton(qrCodeButton);
    }

    public void OnQRCodeButtonClick()
    {
        state = State.QRCode;
        planeTarget.HideTarget();
        SelectButton(qrCodeButton);
        // Add your QRCode button callback logic here
    }

    public void OnLocalizeButtonClick()
    {
        state = State.Localize;
        planeTarget.ShowTarget();
        SelectButton(localizeButton);
        // Add your Localize button callback logic here
    }

    public void OnNavGoalButtonClick()
    {
        planeTarget.ShowTarget();
        SelectButton(navGoalButton);
        // Add your NavGoal button callback logic here
    }

    public void OnActionButtonClick(bool pressed)
    {
        switch (state)
        {
            case State.View:
                break;
            case State.QRCode:
                break;
            case State.Localize:
                if (pressed)
                {
                    Debug.Log("start");
                    planeTarget.StartSettingDirection();
                }
                else
                {
                    Debug.Log("end");
                    planeTarget.EndSettingDirection();
                }
                break;
            case State.NavGoal:
                if (pressed)
                {
                    planeTarget.StartSettingDirection();
                }
                else
                {
                    planeTarget.EndSettingDirection();
                }
                break;
        }

    }

    public void OnMenuButtonClick()
    {
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
