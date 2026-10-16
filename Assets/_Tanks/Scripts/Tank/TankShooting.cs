using UnityEngine;
using UnityEngine.UI;

public class TankShooting : MonoBehaviour
{
    public int m_PlayerNumber = 1;              // Para identificar a los diferentes jugadores
    public GameObject m_Shell;                  // Prefab de la bomba[cite: 41]
    public Transform m_FireTransform;           // Hijo del tanque desde donde se lanzará la bomba[cite: 41]
    public Slider m_AimSlider;                  // Muestra la fuerza de lanzamiento[cite: 41]
    public AudioSource m_ShootingAudio;         // Fuente de audio al disparar[cite: 41]
    public AudioClip m_ChargingClip;            // Clip de carga[cite: 41]
    public AudioClip m_FireClip;                // Clip de disparo[cite: 41]
    public float m_MinLaunchForce = 15f;        // Fuerza mínima[cite: 41]
    public float m_MaxLaunchForce = 30f;        // Fuerza máxima[cite: 41]
    public float m_MaxChargeTime = 0.75f;       // Tiempo máximo de carga[cite: 41]

    private string m_FireButton;
    private float m_CurrentLaunchForce;
    private float m_ChargeSpeed;
    private bool m_Fired;

    private void OnEnable()
    {
        m_CurrentLaunchForce = m_MinLaunchForce; //[cite: 42]
        if (m_AimSlider != null)
            m_AimSlider.value = m_MinLaunchForce; //[cite: 42]
    }

    private void Start()
    {
        m_FireButton = "Fire" + m_PlayerNumber; //[cite: 42]
        m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime; //[cite: 42]
    }

    private void Update()
    {
        if (m_AimSlider != null)
            m_AimSlider.value = m_MinLaunchForce; //[cite: 42]

        // Detectar si se presiona el botón (o la tecla directa: Espacio P1, Enter P2)[cite: 42, 55]
        bool isDown = IsFireButtonDown();
        bool isHeld = IsFireButtonHeld();
        bool isUp = IsFireButtonUp();

        if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)
        {
            m_CurrentLaunchForce = m_MaxLaunchForce; //[cite: 42]
            Fire(); //[cite: 42]
        }
        else if (isDown)
        {
            m_Fired = false; //[cite: 42]
            m_CurrentLaunchForce = m_MinLaunchForce; //[cite: 42]

            if (m_ShootingAudio != null && m_ChargingClip != null)
            {
                m_ShootingAudio.clip = m_ChargingClip; //[cite: 43]
                m_ShootingAudio.Play(); //[cite: 43]
            }
        }
        else if (isHeld && !m_Fired)
        {
            m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime; //[cite: 43]
            if (m_AimSlider != null)
                m_AimSlider.value = m_CurrentLaunchForce; //[cite: 43]
        }
        else if (isUp && !m_Fired)
        {
            Fire(); //[cite: 43]
        }
    }

    private bool IsFireButtonDown()
    {
        if (m_PlayerNumber == 1 && Input.GetKeyDown(KeyCode.Space)) return true; //[cite: 44, 55]
        if (m_PlayerNumber == 2 && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))) return true; //[cite: 55]
        try { return Input.GetButtonDown(m_FireButton); } catch { return false; } //[cite: 42]
    }

    private bool IsFireButtonHeld()
    {
        if (m_PlayerNumber == 1 && Input.GetKey(KeyCode.Space)) return true; //[cite: 44, 55]
        if (m_PlayerNumber == 2 && (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))) return true; //[cite: 55]
        try { return Input.GetButton(m_FireButton); } catch { return false; } //[cite: 43]
    }

    private bool IsFireButtonUp()
    {
        if (m_PlayerNumber == 1 && Input.GetKeyUp(KeyCode.Space)) return true; //[cite: 44, 55]
        if (m_PlayerNumber == 2 && (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))) return true; //[cite: 55]
        try { return Input.GetButtonUp(m_FireButton); } catch { return false; } //[cite: 43]
    }

    private void Fire()
    {
        m_Fired = true; //[cite: 43]

        GameObject shellInstance = Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation);
        Rigidbody shellRb = shellInstance.GetComponent<Rigidbody>();
        if (shellRb != null)
        {
            shellRb.linearVelocity = m_CurrentLaunchForce * m_FireTransform.forward;
        }

        if (m_ShootingAudio != null && m_FireClip != null)
        {
            m_ShootingAudio.clip = m_FireClip; //[cite: 43]
            m_ShootingAudio.Play(); //[cite: 43]
        }

        m_CurrentLaunchForce = m_MinLaunchForce; //[cite: 43]
    }
}