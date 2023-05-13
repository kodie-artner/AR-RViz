
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
 
public class ButtonPressed : Button, IPointerDownHandler, IPointerUpHandler
{
    public event System.Action<bool> ButtonPressedEvent;
    public bool pressed;
    public Color normalColor;
    public Color highlightColor;

    public void OnPointerDown(PointerEventData eventData){
        pressed = true;
        SetButtonNormalColor(highlightColor);
        ButtonPressedEvent?.Invoke(true);
    }
    
    public void OnPointerUp(PointerEventData eventData){
        pressed = false;
        SetButtonNormalColor(normalColor);
        ButtonPressedEvent?.Invoke(false);
    }

    private void SetButtonNormalColor( Color color)
    {
        ColorBlock selectedColors = colors;
            
        selectedColors.normalColor = color;
        selectedColors.highlightedColor = color;

        colors = selectedColors;
    }
}
