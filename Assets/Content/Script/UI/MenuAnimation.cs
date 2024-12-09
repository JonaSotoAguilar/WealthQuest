using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuAnimation : MonoBehaviour
{
    public static MenuAnimation Instance { get; set; }

    [SerializeField] EventSystem system;
    private GameObject selectedButton;

    public GameObject SelectedButton { get => selectedButton; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    #region Animation Buttons

    public void SelectObject(GameObject button)
    {
        system.SetSelectedGameObject(button);
    }

    public void SubscribeButtonsToEvents(GameObject[] buttons, Color color = default)
    {
        foreach (GameObject button in buttons)
        {
            EventTrigger eventTrigger = button.GetComponent<EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = button.AddComponent<EventTrigger>();
            }

            // Crear un Entry para PointerEnter (que también selecciona)
            EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry();
            pointerEnterEntry.eventID = EventTriggerType.PointerEnter;
            pointerEnterEntry.callback.AddListener((eventData) => { OnPointerEnter(button); });
            eventTrigger.triggers.Add(pointerEnterEntry);

            // Crear un Entry para PointerExit (que deselecciona)
            EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry();
            pointerExitEntry.eventID = EventTriggerType.PointerExit;
            pointerExitEntry.callback.AddListener((eventData) => { OnPointerExit(button, color); });
            eventTrigger.triggers.Add(pointerExitEntry);

            // Crear un Entry para Select
            EventTrigger.Entry selectEntry = new EventTrigger.Entry();
            selectEntry.eventID = EventTriggerType.Select;
            selectEntry.callback.AddListener((eventData) => { OnSelect(button); });
            eventTrigger.triggers.Add(selectEntry);

            // Crear un Entry para Deselect
            EventTrigger.Entry deselectEntry = new EventTrigger.Entry();
            deselectEntry.eventID = EventTriggerType.Deselect;
            deselectEntry.callback.AddListener((eventData) => { OnDeselect(button, color); });
            eventTrigger.triggers.Add(deselectEntry);
        }
    }

    private void OnPointerEnter(GameObject button)
    {
        OnSelect(button);
        system.SetSelectedGameObject(button);
    }

    private void OnPointerExit(GameObject button, Color color = default)
    {
        system.SetSelectedGameObject(selectedButton);
        OnDeselect(selectedButton, color);
    }

    private void OnSelect(GameObject button)
    {
        AudioManager.Instance?.PlaySoundButtonSelect();
        selectedButton = button;

        button.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        Image image = button.GetComponent<Image>();
        if (image == null) return;

        image.color = new Color(1f, 168 / 255f, 65 / 255f, 1f);
    }

    private void OnDeselect(GameObject button, Color color = default)
    {
        button.transform.localScale = Vector3.one;
        Image image = button.GetComponent<Image>();
        if (image == null) return;

        image.color = color;
    }

    #endregion
}
