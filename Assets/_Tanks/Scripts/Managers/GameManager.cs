using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Configuración de Partida")]
    public int m_NumRoundsToWin = 5;            // Termina a las 5 victorias (Condición 4)
    public float m_StartDelay = 3f;
    public float m_EndDelay = 3f;
    public CameraControl m_CameraControl;
    public TMP_Text m_MessageText;              // Texto central de mensajes (TMP)
    public GameObject m_TankPrefab;             // Prefab del Tanque
    public TankManager[] m_Tanks;               // Array de 2 tanques

    [Header("Condiciones Especiales")]
    public TMP_Text m_TimerText;                // Texto UI del cronómetro
    public float m_MaxGameTime = 240f;          // 4 minutos = 240 segundos (Condición 6)

    private float m_ElapsedTime = 0f;
    private bool m_IsTimeUp = false;
    private int m_RoundNumber;
    private WaitForSeconds m_StartWait;
    private WaitForSeconds m_EndWait;
    private TankManager m_RoundWinner;
    private TankManager m_GameWinner;

    private void Start()
    {
        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_EndWait = new WaitForSeconds(m_EndDelay);

        SpawnAllTanks();
        SetCameraTargets();

        StartCoroutine(GameLoop());
    }

    private void Update()
    {
        // Cuenta regresiva del temporizador de 4 minutos
        if (m_GameWinner == null && !m_IsTimeUp)
        {
            m_ElapsedTime += Time.deltaTime;
            float timeLeft = m_MaxGameTime - m_ElapsedTime;

            if (timeLeft <= 0f)
            {
                timeLeft = 0f;
                m_IsTimeUp = true;
            }

            UpdateTimerUI(timeLeft);
        }
    }

    private void UpdateTimerUI(float timeToDisplay)
    {
        if (m_TimerText != null)
        {
            int minutes = Mathf.FloorToInt(timeToDisplay / 60F);
            int seconds = Mathf.FloorToInt(timeToDisplay % 60F);
            m_TimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    private void SpawnAllTanks()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].m_Instance =
                Instantiate(m_TankPrefab, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
            m_Tanks[i].m_PlayerNumber = i + 1;
            m_Tanks[i].Setup();
        }
    }

    private void SetCameraTargets()
    {
        Transform[] targets = new Transform[m_Tanks.Length];
        for (int i = 0; i < targets.Length; i++)
        {
            targets[i] = m_Tanks[i].m_Instance.transform;
        }
        m_CameraControl.m_Targets = targets;
    }

    private IEnumerator GameLoop()
    {
        yield return StartCoroutine(RoundStarting());
        yield return StartCoroutine(RoundPlaying());
        yield return StartCoroutine(RoundEnding());

        // Si alguien ganó o se agotaron los 4 minutos
        if (m_GameWinner != null || m_IsTimeUp)
        {
            yield return new WaitForSeconds(5f); // Tiempo para visualizar el ganador y estadísticas
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            StartCoroutine(GameLoop());
        }
    }

    private IEnumerator RoundStarting()
    {
        ResetAllTanks();
        DisableTankControl();
        m_CameraControl.SetStartPositionAndSize();

        m_RoundNumber++;
        m_MessageText.text = "ROUND " + m_RoundNumber;

        yield return m_StartWait;
    }

    private IEnumerator RoundPlaying()
    {
        EnableTankControl();
        m_MessageText.text = string.Empty;

        // Se juega mientras queden tanques y quede tiempo
        while (!OneTankLeft() && !m_IsTimeUp)
        {
            yield return null;
        }
    }

    private IEnumerator RoundEnding()
    {
        DisableTankControl();
        m_RoundWinner = null;

        if (!m_IsTimeUp)
        {
            m_RoundWinner = GetRoundWinner();
            if (m_RoundWinner != null)
                m_RoundWinner.m_Wins++;

            m_GameWinner = GetGameWinner();
        }

        string message = EndMessage();
        m_MessageText.text = message;

        yield return m_EndWait;
    }

    private bool OneTankLeft()
    {
        int numTanksLeft = 0;
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Instance != null && m_Tanks[i].m_Instance.activeSelf)
                numTanksLeft++;
        }
        return numTanksLeft <= 1;
    }

    private TankManager GetRoundWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Instance != null && m_Tanks[i].m_Instance.activeSelf)
                return m_Tanks[i];
        }
        return null;
    }

    private TankManager GetGameWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
                return m_Tanks[i];
        }
        return null;
    }

    private string EndMessage()
    {
        // CONDICIÓN 6: Si el tiempo llega a 0, ambos pierden
        if (m_IsTimeUp)
        {
            return "¡TIEMPO AGOTADO!\nAMBOS JUGADORES PIERDEN";
        }

        string message = "EMPATE!";

        if (m_RoundWinner != null)
            message = m_RoundWinner.m_ColoredPlayerText + " GANA LA RONDA!";

        message += "\n\n\n";

        for (int i = 0; i < m_Tanks.Length; i++)
        {
            message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " VICTORIAS\n";
        }

        // CONDICIÓN 5: Mostrar quién ganó, su puntaje y el tiempo transcurrido
        if (m_GameWinner != null)
        {
            int min = Mathf.FloorToInt(m_ElapsedTime / 60F);
            int sec = Mathf.FloorToInt(m_ElapsedTime % 60F);
            string timeFormatted = string.Format("{0:00}:{1:00}", min, sec);

            message = m_GameWinner.m_ColoredPlayerText + " GANA EL JUEGO!\n\n" +
                      "PUNTAJE FINAL: " + m_GameWinner.m_Wins + " PUNTOS\n" +
                      "TIEMPO TOTAL: " + timeFormatted;
        }

        return message;
    }

    private void ResetAllTanks()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].Reset();
        }
    }

    private void EnableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].EnableControl();
        }
    }

    private void DisableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].DisableControl();
        }
    }
}