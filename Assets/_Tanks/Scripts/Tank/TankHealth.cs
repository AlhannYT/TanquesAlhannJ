using UnityEngine;
using UnityEngine.UI;

public class TankHealth : MonoBehaviour
{
    public float m_StartingHealth = 100f;               // Cantidad de salud con la que empieza el tanque[cite: 31]
    public Slider m_Slider;                             // Slider que representa la salud del tanque[cite: 31]
    public Image m_FillImage;                           // Componente de imagen del slider[cite: 31]
    public Color m_FullHealthColor = Color.green;       // Color del slider con salud completa (verde)[cite: 31]
    public Color m_ZeroHealthColor = Color.red;         // Color del slider con salud vacía (rojo)[cite: 31]
    public GameObject m_ExplosionPrefab;                // Prefab que instanciamos al inicio y usamos cuando el tanque muere[cite: 31]

    private AudioSource m_ExplosionAudio;               // Fuente de audio a reproducir cuando el tanque explota[cite: 31]
    private ParticleSystem m_ExplosionParticles;        // Sistema de partículas que se reproduce al destruir el tanque[cite: 31]
    private float m_CurrentHealth;                      // Almacena la salud actual[cite: 31]
    private bool m_Dead;                                // Comprueba si ya murió[cite: 31]

    // --- Compatibilidad con Unity 6 (PowerUpDetector) ---
    [HideInInspector] public bool m_HasShield = false;
    public void ToggleShield(bool state) { m_HasShield = state; }
    public void ToggleShield(float duration) { }
    public void ToggleInvincibility() { }
    public void ToggleInvincibility(bool state) { }
    public void IncreaseHealth(float amount)
    {
        m_CurrentHealth = Mathf.Min(m_StartingHealth, m_CurrentHealth + amount);
        SetHealthUI();
    }
    // -----------------------------------------------------

    private void Awake()
    {
        // Instanciamos el prefab de la explosión y tomamos sus referencias[cite: 31]
        m_ExplosionParticles = Instantiate(m_ExplosionPrefab).GetComponent<ParticleSystem>(); //[cite: 31]
        m_ExplosionAudio = m_ExplosionParticles.GetComponent<AudioSource>(); //[cite: 32]

        // Deshabilitamos las partículas inicialmente[cite: 32]
        m_ExplosionParticles.gameObject.SetActive(false); //[cite: 32]
    }

    private void OnEnable()
    {
        m_CurrentHealth = m_StartingHealth; //[cite: 32]
        m_Dead = false; //[cite: 32]

        SetHealthUI(); //[cite: 32]
    }

    public void TakeDamage(float amount)
    {
        if (m_HasShield) return;

        // Reducimos la salud según el daño recibido[cite: 32]
        m_CurrentHealth -= amount; //[cite: 32]

        // Actualizamos la UI[cite: 32]
        SetHealthUI(); //[cite: 32]

        // Si la salud llega a 0 o menos y no ha muerto aún, llamamos a OnDeath[cite: 32]
        if (m_CurrentHealth <= 0f && !m_Dead) //[cite: 32]
        {
            OnDeath(); //[cite: 32]
        }
    }

    private void SetHealthUI()
    {
        // Ajustamos el valor del slider[cite: 32]
        if (m_Slider != null) //[cite: 32]
            m_Slider.value = m_CurrentHealth; //[cite: 32]

        // Interpola el color entre verde y rojo según el porcentaje de vida restante[cite: 32]
        if (m_FillImage != null) //[cite: 32]
            m_FillImage.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth / m_StartingHealth); //[cite: 32]
    }

    private void OnDeath()
    {
        m_Dead = true; //[cite: 32]

        // Movemos la explosión a la posición del tanque y la activamos[cite: 32]
        m_ExplosionParticles.transform.position = transform.position; //[cite: 32]
        m_ExplosionParticles.gameObject.SetActive(true); //[cite: 33]

        m_ExplosionParticles.Play(); //[cite: 33]
        m_ExplosionAudio.Play(); //[cite: 33]

        // Desactivamos el tanque[cite: 33]
        gameObject.SetActive(false); //[cite: 33]
    }
}