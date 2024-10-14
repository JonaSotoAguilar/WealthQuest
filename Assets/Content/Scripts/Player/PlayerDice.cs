using System.Collections;
using UnityEngine;

public class PlayerDice : MonoBehaviour
{
    [SerializeField] private diceTypeList diceType;
    private Rigidbody myRigidbody;
    private int diceRoll;
    private bool isSpinning = true;
    private Vector3 rotationDirection = new Vector3(300f, 300f, 300f); // Dirección inicial de la rotación

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

    public int DiceRoll { get => diceRoll; }

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody>();
        StartCoroutine(ChangeRotationDirection());
    }

    void Update()
    {
        if (isSpinning)
        {
            RotateDice();
        }
    }

    // Rotación constante del dado
    // Rotación constante del dado con cambio de dirección
    private void RotateDice()
    {
        // Aplica la rotación usando la dirección actual
        transform.Rotate(rotationDirection * Time.deltaTime);
    }

    // Corrutina para cambiar la dirección de la rotación
    private IEnumerator ChangeRotationDirection()
    {
        while (isSpinning)
        {
            // Esperar un tiempo aleatorio antes de cambiar la dirección
            yield return new WaitForSeconds(Random.Range(0.5f, 2f));

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
        isSpinning = false; // Detener la rotación

        // Asegurarse de redondear la rotación para que el dado se estabilice en una cara específica
        myRigidbody.angularVelocity = Vector3.zero;
        transform.rotation = Quaternion.Euler(
            Mathf.Round(transform.rotation.eulerAngles.x / 90) * 90,
            Mathf.Round(transform.rotation.eulerAngles.y / 90) * 90,
            Mathf.Round(transform.rotation.eulerAngles.z / 90) * 90
        );

        // Verificar el resultado según la orientación de la cámara y la posición del dado.
        CheckResult();

        yield return HideDiceAfterDelay(1.3f);
    }


    // Verificar el resultado del lanzamiento del dado
    void CheckResult()
    {
        float maxX = -50000; // Asumimos que la cara que buscamos está inicialmente fuera de vista
        for (int index = 0; index < (int)diceType; index++)
        {
            var getChild = gameObject.transform.GetChild(index);

            // Determinar la cara visible desde el lado positivo del eje X
            if (getChild.position.x > maxX)
            {
                maxX = getChild.position.x;
                diceRoll = index + 1;
            }
        }
    }

    private IEnumerator HideDiceAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
        isSpinning = true;
    }

    public void ShowDice(bool show)
    {
        gameObject.SetActive(show);
    }
}
