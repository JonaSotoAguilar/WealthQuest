using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class ButtonAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, ISubmitHandler, IPointerClickHandler
{
    // Event System
    private EventSystem eventSystem;

    // Enum for selecting color types
    public enum ColorType
    {
        None,
        Orange
    }

    private void Awake()
    {
        eventSystem = EventSystem.current;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        eventSystem.SetSelectedGameObject(gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {

    }

    public void OnSelect(BaseEventData eventData)
    {
        AudioManager.PlaySoundButtonSelect();
        ScaleButton();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        UnscaleButton();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnSubmit(eventData);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        AudioManager.PlaySoundButtonPress();
    }

    private void ScaleButton()
    {
        LeanTween.scale(gameObject, Vector3.one * 1.2f, 0.1f).setEase(LeanTweenType.easeOutBack).setIgnoreTimeScale(true); ;
    }

    private void UnscaleButton()
    {
        LeanTween.scale(gameObject, Vector3.one, 0.1f).setEase(LeanTweenType.easeOutBack).setIgnoreTimeScale(true); ;
    }
}
