using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// ButtonPressed adds the capability of detecting start of button press and end of button press.
// The default Button class only detects end of press.
public class ButtonPressed : Button, IPointerDownHandler, IPointerUpHandler
{
    public enum State
    {
        OnUpStay,
        OnDown,
        OnDownStay,
        OnUp,
    }

    public Color normalColor;
    public Color highlightedColor;

    private State currentState;
    private State previousState;

    public void OnPointerDown(PointerEventData eventData)
    {
        previousState = currentState;
        currentState = State.OnDown;
        SetButtonNormalColor(highlightedColor);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        previousState = currentState;
        currentState = State.OnUp;
        SetButtonNormalColor(normalColor);
    }

    public State GetState()
    {
        State state = currentState;

        switch (currentState)
        {
            case State.OnUp:
                if (previousState == State.OnUp)
                {
                    state = State.OnUpStay;
                }
                break;

            case State.OnDown:
                if (previousState == State.OnDown)
                {
                    state = State.OnDownStay;
                }
                break;
        }

        previousState = currentState;
        currentState = state;

        return state;
    }

    private void SetButtonNormalColor(Color color)
    {
        ColorBlock selectedColors = colors;

        selectedColors.normalColor = color;
        selectedColors.highlightedColor = color;

        colors = selectedColors;
    }
}
