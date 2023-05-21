using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonPressed : Button, IPointerDownHandler, IPointerUpHandler
{
    public enum State
    {
        OnUpStay,
        OnDown,
        OnDownStay,
        OnUp,
    }

    public State state { get => GetState(); }

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

    private State GetState()
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
