using System.Collections;
using UnityEngine;

public class DiceController : MonoBehaviour
{
    [SerializeField] private diceTypeList diceType;
    [SerializeField] private Rigidbody myRigidbody;
    private bool mode2D;
    private int diceRoll;
    private float topSide;
    private bool diceSleeping;

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
    public bool DiceSleeping { get => diceSleeping; }

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody>();
    }

    // Lanzar el dado
    public void LaunchDice()
    {
        // Aplicar fuerza y torque aleatorio al dado
        diceSleeping = false;
        Vector3 fuerzaAleatoria = new Vector3(Random.Range(-5f, 5f), 10f, Random.Range(-5f, 5f));
        Vector3 torqueAleatorio = new Vector3(Random.Range(-500f, 500f), Random.Range(-500f, 500f), Random.Range(-500f, 500f));
        myRigidbody.AddForce(fuerzaAleatoria, ForceMode.Impulse);
        myRigidbody.AddTorque(torqueAleatorio, ForceMode.Impulse);
        StartCoroutine(WaitAndCheckResult()); 
    }

    // Corutina para esperar hasta que el dado se detenga
    IEnumerator WaitAndCheckResult()
    {
        yield return new WaitForSeconds(1); 
        while (!myRigidbody.IsSleeping()) yield return null;
        CheckResult();
    }

    // Verificar el resultado del lanzamiento del dado
    void CheckResult()
    {
        topSide = mode2D ? 50000 : -50000;
        for (int index = 0; index < (int)diceType; index++)
        {
            var getChild = gameObject.transform.GetChild(index);
            if (!mode2D)
            {
                if (getChild.position.y > topSide)
                {
                    topSide = getChild.position.y;
                    diceRoll = index + 1;
                }
            }
            else
            {
                if (getChild.position.z < topSide)
                {
                    topSide = getChild.position.z;
                    diceRoll = index + 1;
                }
            }
        }
        diceSleeping = true;
    }
}
