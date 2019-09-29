using UnityEngine;
using System.Collections.Generic;

public class TankMovement : MonoBehaviour
{
  public int m_PlayerNumber = 1;
  public float m_Speed = 12f;
  public float m_TurnSpeed = 180f;
  public AudioSource m_MovementAudio;
  public AudioClip m_EngineIdling;
  public AudioClip m_EngineDriving;
  public float m_PitchRange = 0.2f;
  public Vector3 m_OpponentPosition;
  public bool m_Passive = false;

  private string m_MovementAxisName;
  private string m_TurnAxisName;
  private Rigidbody m_Rigidbody;
  private float m_MovementInputValue;
  private float m_TurnInputValue;
  private float m_OriginalPitch;
  private TankAI m_TankAI;
  private GameObject m_TargetPointObj;
  private Transform m_TargetPoint;

  private Vector3 m_Hideout;


  private void Awake()
  {
    m_Rigidbody = GetComponent<Rigidbody>();
    m_TankAI = GetComponent<TankAI>();
    m_TargetPointObj = new GameObject();
    m_TargetPoint = m_TargetPointObj.transform;
    m_Hideout = new Vector3(0f, 0f, 0f);
  }

  private void OnEnable()
  {
    m_Rigidbody.isKinematic = false;
    m_MovementInputValue = 0f;
    m_TurnInputValue = 0f;
  }

  private void OnDisable()
  {
    m_Rigidbody.isKinematic = true;
    m_TankAI.m_Path.Clear();
  }

  private void Start()
  {

    m_MovementAxisName = "Vertical" + m_PlayerNumber;
    m_TurnAxisName = "Horizontal" + m_PlayerNumber;

    m_OriginalPitch = m_MovementAudio.pitch;
  }

  private void Update()
  {
    // Store the player's input and make sure the audio for the engine is playing.
    m_MovementInputValue = Input.GetAxis(m_MovementAxisName);
    m_TurnInputValue = Input.GetAxis(m_TurnAxisName);
    if (Input.GetKeyDown(KeyCode.I)) m_TankAI.activateAI();
    if (m_TankAI.m_AIon && !m_TankAI.m_AIbusy && m_TankAI.m_Path.Count <= 0)
    {
      AI();
    }
    EngineAudio();
  }

  private void EngineAudio()
  {
    // Play the correct audio clip based on whether or not the tank is moving and what audio is currently playing.
    if (Mathf.Abs(m_MovementInputValue) < 0.1f && Mathf.Abs(m_TurnInputValue) < 0.1f)
    {
      if (m_MovementAudio.clip == m_EngineDriving)
      {
        m_MovementAudio.clip = m_EngineIdling;
        m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
        m_MovementAudio.Play();
      }
    }
    else
    {
      if (m_MovementAudio.clip == m_EngineIdling)
      {
        m_MovementAudio.clip = m_EngineDriving;
        m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
        m_MovementAudio.Play();
      }
    }

  }

  private void FixedUpdate()
  {
    // Move and turn the tank.
    if (!m_TankAI.m_AIon)
    {
      Move();
      Turn();
    }
    else
    {
      if (m_TankAI.m_Path.Count > 0)
      {
        m_TargetPoint.position = m_TankAI.m_Path[0];
        Follow();
      }
    }
  }

  private void Move()
  {
    // Adjust the position of the tank based on the player's input.
    Vector3 movement = transform.forward * m_MovementInputValue * m_Speed * Time.deltaTime;
    m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
  }

  private void Turn()
  {
    // Adjust the rotation of the tank based on the player's input.
    float turn = m_TurnInputValue * m_TurnSpeed * Time.deltaTime;
    Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
    m_Rigidbody.MoveRotation(m_Rigidbody.rotation * turnRotation);
  }

  void Follow()
  {
    // rotate towards target
    transform.forward = Vector3.RotateTowards(transform.forward, m_TargetPoint.position - transform.position, m_TurnSpeed * Time.deltaTime, 0.0f);

    // move towards target
    transform.position = Vector3.MoveTowards(transform.position, m_TargetPoint.position, m_Speed * Time.deltaTime);

    if (InRange(transform.position, m_TargetPoint.position, 0.5f))
    {
      m_TankAI.m_Path.RemoveAt(0);
      if (m_TankAI.m_Path.Count > 0) m_TargetPoint.position = m_TankAI.m_Path[0];
    }
  }

  private bool InRange(Vector3 from, Vector3 to, float maxRange)
  {
    return ((to - from).sqrMagnitude < maxRange * maxRange);
  }

  private void AI()
  {
    float distance = 20f;
    if (!m_Passive)
    {
      m_TankAI.FindPath(m_Rigidbody.position, m_OpponentPosition);
    }
    else
    {
      if (InRange(transform.position, m_Hideout, 1f))
      {
        m_TankAI.FindPath(m_Rigidbody.position, hideoutPnt(distance));
      }
    }
  }

  private Vector3 hideoutPnt(float distance)
  {

    Vector3 pos = new Vector3(0f, 0f, 0f);
    float magnitude = 0f;
    while (magnitude < distance)
    {
      pos = new Vector3(UnityEngine.Random.Range(-45f, 45f), 0f, UnityEngine.Random.Range(-45f, 45f));
      magnitude = (pos - m_OpponentPosition).magnitude;
    }
    for (float angle = 0f; angle < 360f; angle += 45f)
    {
      Vector3 to = (Quaternion.Euler(0f, angle, 0f) * new Vector3(1f, 0f, 0f)).normalized + pos;
      if (Physics.Linecast(pos, to)) return new Vector3(0f, 0f, 0f);
    }

    return pos;
  }
}