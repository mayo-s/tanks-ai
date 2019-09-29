using System;
using UnityEngine;

[Serializable]
public class TankManager
{
  public Color m_PlayerColor;
  public Transform m_SpawnPoint;
  [HideInInspector] public int m_PlayerNumber;
  [HideInInspector] public string m_ColoredPlayerText;
  [HideInInspector] public GameObject m_Instance;
  [HideInInspector] public int m_Wins;

  private TankMovement m_Movement;
  private TankShooting m_Shooting;
  public TankHealth m_Health;
  private GameObject m_CanvasGameObject;

  public void Setup()
  {
    m_Movement = m_Instance.GetComponent<TankMovement>();
    m_Shooting = m_Instance.GetComponent<TankShooting>();
    m_Health = m_Instance.GetComponent<TankHealth>();
    m_CanvasGameObject = m_Instance.GetComponentInChildren<Canvas>().gameObject;

    m_Movement.m_PlayerNumber = m_PlayerNumber;
    m_Shooting.m_PlayerNumber = m_PlayerNumber;

    m_ColoredPlayerText = "<color=#" + ColorUtility.ToHtmlStringRGB(m_PlayerColor) + ">PLAYER " + m_PlayerNumber + "</color>";

    MeshRenderer[] renderers = m_Instance.GetComponentsInChildren<MeshRenderer>();

    for (int i = 0; i < renderers.Length; i++)
    {
      renderers[i].material.color = m_PlayerColor;
    }
  }

  public void DisableControl()
  {
    m_Movement.enabled = false;
    m_Shooting.enabled = false;

    m_CanvasGameObject.SetActive(false);
  }

  public void EnableControl()
  {
    m_Movement.enabled = true;
    m_Shooting.enabled = true;

    m_CanvasGameObject.SetActive(true);
  }

  public void Reset()
  {
    while(!RndmSpwnPnt());
    m_Instance.transform.position = m_SpawnPoint.position;
    m_Instance.transform.rotation = m_SpawnPoint.rotation;

    m_Instance.SetActive(false);
    m_Instance.SetActive(true);
  }

  // TODO make list for multiple opponents
  public void UpdateOpponentPosition(Vector3 pos, int player)
  {
    if (m_PlayerNumber != player)
    {
      m_Movement.m_OpponentPosition = pos;
      m_Shooting.m_OpponentPosition = pos;
    }
  }

  private bool RndmSpwnPnt()
  {
    Vector3 pos = new Vector3(UnityEngine.Random.Range(-40f, 40f), 0f, UnityEngine.Random.Range(-40f, 40f));
    for (float angle = 0f; angle < 360f; angle += 20f)
    {
      Vector3 to = (Quaternion.Euler(0f, angle, 0f) * new Vector3(1f, 0f, 0f)).normalized + pos;
      if(Physics.Linecast(pos, to)) return false;
    }
    m_SpawnPoint.position = pos;
    return true;
  }

  public void setTactic(bool tactic){
    m_Movement.m_Passive = tactic;
  }
}