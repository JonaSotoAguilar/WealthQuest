using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    private Transform[] casillas; // Array de casillas
    private int currentPosition = 0;
    public float velocidadMovimiento = 2f; // Velocidad del movimiento del jugador
    private bool isMoving = false; // Estado del movimiento del jugador

    public int jugadorIndex; // Índice del jugador (0 = esquina superior izquierda, 1 = superior derecha, etc.)

    // Offsets para cada esquina de la casilla
    private Vector3[] esquinas = new Vector3[]
    {
        new Vector3(-0.6f, 0f, 0.6f),  // Esquina superior izquierda
        new Vector3(0.6f, 0f, 0.6f),   // Esquina superior derecha
        new Vector3(-0.6f, 0f, -0.6f), // Esquina inferior izquierda
        new Vector3(0.6f, 0f, -0.6f)   // Esquina inferior derecha
    };

    void Start()
    {
        GameObject contenedorCasillas = GameObject.Find("Squares");
        if (contenedorCasillas == null)
        {
            Debug.LogError("El objeto 'Squares' no se ha encontrado en la escena.");
            return;
        }

        // Rellenar el array de casillas con los hijos del objeto contenedor
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
            StartCoroutine(Mover(steps));
        }
        else
        {
            Debug.Log("Espera a que termine el movimiento actual.");
        }
    }

    private IEnumerator Mover(int steps)
    {
        isMoving = true; // Comienza el movimiento
        for (int i = 0; i < steps; i++)
        {
            currentPosition++;
            Vector3 posicionInicial = transform.position;
            Vector3 posicionCentroCasilla = casillas[currentPosition].position;

            // Raycast para determinar la posición exacta sobre la casilla
            RaycastHit hit;
            Vector3 rayStart = posicionCentroCasilla + Vector3.up * 10; // Comenzar el rayo desde arriba
            if (Physics.Raycast(rayStart, Vector3.down, out hit, Mathf.Infinity))
            {
                Vector3 offsetEsquina = esquinas[jugadorIndex]; // Offset para posicionar en la esquina correcta
                Vector3 posicionDestino = hit.point + new Vector3(offsetEsquina.x, 0, offsetEsquina.z); // Ajusta la posición a la altura de la colisión

                Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, hit.normal); // Rotación para alinear con la inclinación

                float tiempo = 0f;
                while (tiempo < 1f)
                {
                    tiempo += Time.deltaTime * velocidadMovimiento;
                    transform.position = Vector3.Lerp(posicionInicial, posicionDestino, tiempo);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, tiempo);
                    yield return null;
                }
            }
            else
            {
                Debug.LogError("No se encontró la superficie bajo la casilla.");
            }
        }

        // Aquí activamos solo la última casilla (la de destino final)
        Square casilla = casillas[currentPosition].GetComponent<Square>();
        if (casilla != null)
        {
            casilla.ActivarCasilla(GetComponent<PlayerStats>());

            // Actualizamos los stats del jugador después de la activación de la casilla
            PlayerStats jugadorStats = GetComponent<PlayerStats>();

            // Actualiza el HUD del jugador
            HUDManager.instance.ActualizarHUD(jugadorStats); // Asume que tienes un método para esto en tu HUDManager
        }

        isMoving = false; // Termina el movimiento
    }



    public bool IsMoving()
    {
        return isMoving; // Devuelve el estado actual de movimiento del jugador
    }
}