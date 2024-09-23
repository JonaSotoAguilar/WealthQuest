using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    private Transform[] casillas; // Array de casillas
    private int currentPosition = 0;
    public float velocidadMovimiento = 2f; // Velocidad del movimiento del jugador
    private bool isMoving = false; // Estado del movimiento del jugador

    void Start()
    {
        GameObject contenedorCasillas = GameObject.Find("Square");
        if (contenedorCasillas == null)
        {
            Debug.LogError("El objeto 'Casillas' no se ha encontrado en la escena.");
            return;
        }

        casillas = new Transform[contenedorCasillas.transform.childCount];
        for (int i = 0; i < casillas.Length; i++)
        {
            casillas[i] = contenedorCasillas.transform.GetChild(i);
        }
    }

    public void MoverJugador(int steps)
    {
        if (!isMoving)
        {
            int casillasRestantes = casillas.Length - currentPosition - 1;
            steps = Mathf.Min(steps, casillasRestantes);
            StartCoroutine(AvanzarCasilla(steps));
        }
        else
        {
            Debug.Log("Espera a que termine el movimiento actual.");
        }
    }

    private IEnumerator AvanzarCasilla(int steps)
    {
        isMoving = true; // Comienza el movimiento
        for (int i = 0; i < steps; i++)
        {
            currentPosition++;
            Vector3 posicionInicial = transform.position;
            Vector3 posicionDestino = casillas[currentPosition].position;
            Collider casillaCollider = casillas[currentPosition].GetComponent<Collider>();
            posicionDestino.y += casillaCollider.bounds.size.y;

            float tiempo = 0f;
            while (tiempo < 1f)
            {
                tiempo += Time.deltaTime * velocidadMovimiento;
                transform.position = Vector3.Lerp(posicionInicial, posicionDestino, tiempo);
                yield return null;
            }
        }
        isMoving = false; // Termina el movimiento
    }

    public bool IsMoving()
    {
        return isMoving; // Devuelve el estado actual de movimiento del jugador
    }

}
