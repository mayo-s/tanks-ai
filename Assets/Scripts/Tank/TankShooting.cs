using UnityEngine;
using UnityEngine.UI;

public class TankShooting : MonoBehaviour
{
  public int m_PlayerNumber = 1;
  public Rigidbody m_Shell;
  public Transform m_FireTransform;
  public Slider m_AimSlider;
  public AudioSource m_ShootingAudio;
  public AudioClip m_ChargingClip;
  public AudioClip m_FireClip;
  public float m_MinLaunchForce = 15f;
  public float m_MaxLaunchForce = 30f;
  public float m_MaxChargeTime = 0.75f;
  [HideInInspector] public Vector3 m_OpponentPosition;

  private TankAI m_TankAI;
  private string m_FireButton;
  private float m_CurrentLaunchForce;
  private float m_ChargeSpeed;
  private bool m_Fired;
  private float m_LastShootAction = 0;

  private void Awake()
  {
    m_TankAI = GetComponent<TankAI>();
    m_LastShootAction = Time.realtimeSinceStartup;
  }
  private void OnEnable()
  {
    m_CurrentLaunchForce = m_MinLaunchForce;
    m_AimSlider.value = m_MinLaunchForce;
  }

  private void Start()
  {
    m_FireButton = "Fire" + m_PlayerNumber;

    m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;
  }

  private void Update()
  {
    if (!m_TankAI.m_AIon)
    {
      // Track the current state of the fire button and make decisions based on the current launch force.
      m_AimSlider.value = m_MinLaunchForce;

      if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)
      {
        m_CurrentLaunchForce = m_MaxLaunchForce;
        Fire();
      }
      else if (Input.GetButtonDown(m_FireButton))
      {
        m_Fired = false;
        m_CurrentLaunchForce = m_MinLaunchForce;
        m_ShootingAudio.clip = m_ChargingClip;
        m_ShootingAudio.Play();
      }
      else if (Input.GetButton(m_FireButton) && !m_Fired)
      {
        m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;
        m_AimSlider.value = m_CurrentLaunchForce;
      }
      else if (Input.GetButtonUp(m_FireButton) && !m_Fired)
      {
        Fire();
      }
    }
    else if (!Physics.Linecast(transform.position, m_OpponentPosition) && (m_OpponentPosition - transform.position).magnitude < 30f)
    {
      if (ShootingDelay())
      {
        m_TankAI.m_AIaiming = true;
        m_CurrentLaunchForce = (m_OpponentPosition - transform.position).magnitude;
        if (m_CurrentLaunchForce > m_MaxLaunchForce) m_CurrentLaunchForce = m_MaxLaunchForce;

        // rotate towards target
        transform.forward = Vector3.RotateTowards(transform.forward, m_OpponentPosition - transform.position, 180f * Time.deltaTime, 0.0f);
        Fire();
      }
    }
  }

  private void Fire()
  {
    // Instantiate and launch the shell.
    m_Fired = true;
    Rigidbody shellInstance = Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;
    shellInstance.velocity = m_CurrentLaunchForce * m_FireTransform.forward;
    m_ShootingAudio.clip = m_FireClip;
    m_ShootingAudio.Play();
    m_CurrentLaunchForce = m_MinLaunchForce;
  }

  private bool ShootingDelay()
  {

    if (Time.realtimeSinceStartup - m_LastShootAction > 2f) return true;
    return false;
  }
}