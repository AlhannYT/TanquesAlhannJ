using UnityEngine;

public class EnemyObstacle : MonoBehaviour
{
    [Header("Movimiento")]
    public float m_Speed = 4f;
    public float m_Distance = 12f;
    public bool m_MoveOnZAxis = true;

    private Vector3 m_StartPos;
    private int m_Direction = 1;
    private Rigidbody m_Rb;

    private void Start()
    {
        m_StartPos = transform.position;
        m_Rb = GetComponent<Rigidbody>();
        if (m_Rb != null)
        {
            m_Rb.isKinematic = true; // Para que empuje tanques y absorba proyectiles sin volcarse
        }
    }

    private void FixedUpdate()
    {
        Vector3 movement = m_MoveOnZAxis ? Vector3.forward : Vector3.right;
        transform.position += movement * m_Direction * m_Speed * Time.fixedDeltaTime;

        float currentOffset = m_MoveOnZAxis ? (transform.position.z - m_StartPos.z) : (transform.position.x - m_StartPos.x);

        if (Mathf.Abs(currentOffset) >= m_Distance)
        {
            m_Direction *= -1;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Si choca con un obstáculo del mapa, se devuelve
        if (!collision.gameObject.CompareTag("Player"))
        {
            m_Direction *= -1;
        }
    }
}