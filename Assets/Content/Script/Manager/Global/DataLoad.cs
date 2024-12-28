using UnityEngine;

public class DataLoad : MonoBehaviour
{
    [Header("Content")]
    [SerializeField] private Content content;

    private async void Awake()
    {
        await ProfileUser.LoadProfile();
        content.InitializateLocalContent();
    }
}
