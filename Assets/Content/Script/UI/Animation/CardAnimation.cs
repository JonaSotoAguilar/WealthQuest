using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class CardAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, ISubmitHandler, IPointerClickHandler
{

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnSelect(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnDeselect(eventData);
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
        LeanTween.scale(gameObject, Vector3.one * 1.1f, 0.1f).setEase(LeanTweenType.easeOutBack).setIgnoreTimeScale(true); ;
    }

    private void UnscaleButton()
    {
        LeanTween.scale(gameObject, Vector3.one, 0.1f).setEase(LeanTweenType.easeOutBack).setIgnoreTimeScale(true); ;
    }
}
