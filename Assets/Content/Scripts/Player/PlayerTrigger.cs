using UnityEngine;

public class PlayerTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) {
            PlayerData player = other.GetComponent<PlayerData>();
        }
    }

    private void OnTriggerExit(Collider other)
    {

    }
}