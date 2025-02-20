using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class CardAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, ISubmitHandler, IPointerClickHandler
{

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetSelected();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //OnDeselect(eventData);
    }

    public void OnSelect(BaseEventData eventData)
    {
        AudioManager.PlaySoundButtonSelect();
        ActiveOutline(true);
        ScaleButton();
        SetSelected();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        ActiveOutline(false);
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

    #region Animation

    private void ActiveOutline(bool active)
    {
        Outline outline = GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = active;
        }
    }

    private void ScaleButton()
    {
        LeanTween.scale(gameObject, Vector3.one * 1.1f, 0.1f).setEase(LeanTweenType.easeOutBack).setIgnoreTimeScale(true); ;
    }

    private void UnscaleButton()
    {
        LeanTween.scale(gameObject, Vector3.one, 0.1f).setEase(LeanTweenType.easeOutBack).setIgnoreTimeScale(true); ;
    }

    #endregion

    #region Selectable

    private void SetSelected()
    {
        EventSystem eventSystem = EventSystem.current;
        Selectable firstSelectable = gameObject.GetComponent<Selectable>();

        if (eventSystem == null || firstSelectable == null) return;

        if (firstSelectable != null)
        {
            // Verifica si el objeto actualmente seleccionado es diferente al que se quiere seleccionar
            if (eventSystem.currentSelectedGameObject != gameObject)
            {
                firstSelectable.Select();
            }
        }
    }

    #endregion


}
