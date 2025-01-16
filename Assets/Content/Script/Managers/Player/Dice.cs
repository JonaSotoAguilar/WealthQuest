using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Dice : MonoBehaviour
{
    private Rigidbody myRigidbody;
    private int diceRoll;
    private bool isSpinning = false;
    private Vector3 initialPosition;
    private Vector3 rotationDirection = new Vector3(350f, 350f, 350f);

    public int DiceRoll { get => diceRoll; }

    public enum diceTypeList
    {
        D4 = 4,
        D6 = 6,
        D8 = 8,
        D10 = 10,
        D12 = 12,
        D16 = 16,
        D20 = 20,
    }

    void Awake()
    {
        ShowDice(false);
        myRigidbody = GetComponent<Rigidbody>();
        myRigidbody.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
        myRigidbody.isKinematic = true;
        myRigidbody.useGravity = false;
        initialPosition = transform.localPosition;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    public IEnumerator RotateDiceRoutine()
    {
        isSpinning = true;
        StartCoroutine(ChangeRotationDirection());
        while (isSpinning)
        {
            RotateDice();
            yield return null;
        }
        yield break;
    }


    // Rotar el dado
    private void RotateDice()
    {
        transform.localRotation *= Quaternion.Euler(rotationDirection * Time.deltaTime);
        transform.localPosition = initialPosition;
    }

    // Corrutina para cambiar la dirección de la rotación
    private IEnumerator ChangeRotationDirection()
    {
        while (isSpinning)
        {
            // Esperar un tiempo aleatorio antes de cambiar la dirección
            yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

            // Cambiar la dirección de la rotación aleatoriamente (invirtiendo los ejes)
            rotationDirection = new Vector3(
                rotationDirection.x * (Random.value > 0.5f ? 1 : -1),
                rotationDirection.y * (Random.value > 0.5f ? 1 : -1),
                rotationDirection.z * (Random.value > 0.5f ? 1 : -1)
            );
        }
    }

    // Detener el dado y calcular el resultado (ahora como IEnumerator)
    public IEnumerator StopDice()
    {
        yield return new WaitForSeconds(1.1f);
        isSpinning = false;

        // Asegurarse de redondear la rotación para que el dado se estabilice en una cara específica
        myRigidbody.angularVelocity = Vector3.zero;
        transform.rotation = Quaternion.Euler(
            Mathf.Round(transform.rotation.eulerAngles.x / 90) * 90,
            Mathf.Round(transform.rotation.eulerAngles.y / 90) * 90,
            Mathf.Round(transform.rotation.eulerAngles.z / 90) * 90
        );

        // Verificar el resultado según la orientación de la cámara y la posición del dado.
        CheckResult();

        yield return new WaitForSeconds(1.3f);
    }

    // Verificar el resultado del lanzamiento del dado
    void CheckResult()
    {
        Vector3 characterRightDirection = transform.parent.right;

        float maxDot = -1f;
        diceRoll = 0;

        // Iterar a través de cada cara del dado
        for (int index = 0; index < transform.childCount; index++)
        {
            Transform child = transform.GetChild(index);
            Vector3 faceDirection = (child.position - transform.position).normalized;

            float dotProduct = Vector3.Dot(characterRightDirection, faceDirection);

            if (dotProduct > maxDot)
            {
                maxDot = dotProduct;
                diceRoll = index + 1;
            }
        }
    }

    public void ShowDice(bool show)
    {
        gameObject.SetActive(show);
    }
}