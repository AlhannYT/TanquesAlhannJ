using UnityEngine;

public class CameraControl : MonoBehaviour
{
    // Tiempo de amortiguación para el movimiento y zoom
    public float m_DampTime = 0.2f;
    // Margen adicional para evitar que los tanques queden pegados al borde de la pantalla
    public float m_ScreenEdgeBuffer = 4f;
    // Tamaño mínimo ortográfico permitido para el zoom
    public float m_MinSize = 6.5f;

    // Arreglo de objetivos (los tanques a enfocar)
    /*[HideInInspector]*/
    public Transform[] m_Targets;

    private Camera m_Camera;
    private float m_ZoomSpeed;
    private Vector3 m_MoveVelocity;
    private Vector3 m_DesiredPosition;

    private void Awake()
    {
        // Se obtiene la referencia a la cámara hija del objeto
        m_Camera = GetComponentInChildren<Camera>();
    }

    private void FixedUpdate()
    {
        Move();
        Zoom();
    }

    private void Move()
    {
        // Calcula el punto central entre todos los tanques activos
        FindAveragePosition();

        // Desplaza la cámara suavemente hacia esa posición
        transform.position = Vector3.SmoothDamp(transform.position, m_DesiredPosition, ref m_MoveVelocity, m_DampTime);
    }

    private void FindAveragePosition()
    {
        Vector3 averagePos = new Vector3();
        int numTargets = 0;

        // Suma las coordenadas de los tanques que estén activos en la escena
        for (int i = 0; i < m_Targets.Length; i++)
        {
            if (m_Targets[i] == null || !m_Targets[i].gameObject.activeSelf)
                continue;

            averagePos += m_Targets[i].position;
            numTargets++;
        }

        if (numTargets > 0)
        {
            averagePos /= numTargets;
        }

        // Conserva la altura original del CameraRig
        averagePos.y = transform.position.y;
        m_DesiredPosition = averagePos;
    }

    private void Zoom()
    {
        // Determina el tamaño ortográfico requerido y lo interpola de forma suave
        float requiredSize = FindRequiredSize();
        m_Camera.orthographicSize = Mathf.SmoothDamp(m_Camera.orthographicSize, requiredSize, ref m_ZoomSpeed, m_DampTime);
    }

    private float FindRequiredSize()
    {
        // Convierte la posición deseada al espacio local del CameraRig
        Vector3 desiredLocalPos = transform.InverseTransformPoint(m_DesiredPosition);
        float size = 0f;

        // Comprueba la distancia de cada tanque activo respecto al centro de la cámara
        for (int i = 0; i < m_Targets.Length; i++)
        {
            if (m_Targets[i] == null || !m_Targets[i].gameObject.activeSelf)
                continue;

            Vector3 targetLocalPos = transform.InverseTransformPoint(m_Targets[i].position);
            Vector3 desiredPosToTarget = targetLocalPos - desiredLocalPos;

            // Determina la distancia máxima en vertical
            size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.y));

            // Determina la distancia máxima en horizontal dividiendo por el ratio de aspecto
            size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.x) / m_Camera.aspect);
        }

        // Aplica el margen de pantalla y asegura el tamaño mínimo de encuadre
        size += m_ScreenEdgeBuffer;
        size = Mathf.Max(size, m_MinSize);

        return size;
    }

    // Utilizado por el GameManager al reiniciar cada ronda para ubicar la cámara de inmediato
    public void SetStartPositionAndSize()
    {
        FindAveragePosition();
        transform.position = m_DesiredPosition;
        m_Camera.orthographicSize = FindRequiredSize();
    }
}