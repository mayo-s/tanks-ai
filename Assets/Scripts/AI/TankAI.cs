using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankAI : MonoBehaviour
{

  public List<Vector3> m_Path;
  // Start is called before the first frame update
  void Awake()
  {
    m_Path = new List<Vector3>();
  }

  // Update is called once per frame
  void Update()
  {

  }

  private List<Vector3> FindPath(List<Vector3> map, Vector3 fromVec, Vector3 toVec)
  {
    List<Node> open = new List<Node>();
    List<Node> closed = new List<Node>();
    Node from = new Node(fromVec);
    Node to = new Node(toVec
);
    open.Add(from);
    List<Vector3> path = new List<Vector3>();
    while (open.Count > 0)
    {
      Node current = GetCheapestNodeFromList(open);
      open.Remove(current);
      closed.Add(current);
      current.UpdateValues(GetDistanceFromTo(fromVec, toVec
));
      if (InRange(current.vec, toVec, 6f))
      {
        path = BacktrackNodes(current);
        break;
      }
    }

    // path found
    return path;
  }

  private List<Node> GetNewNodes(Node node)
  {
    List<Node> newNodes = new List<Node>();

    return newNodes;
  }

  private Vector3 GetVectorFromTo(Vector3 from, Vector3 to)
  {
    return (to - from);
  }

  private float GetDistanceFromTo(Vector3 from, Vector3 to)
  {
    return GetVectorFromTo(from, to).magnitude;
  }

  private Vector3 GetDirectionFromTo(Vector3 from, Vector3 to)
  {
    return (GetVectorFromTo(from, to) / GetDistanceFromTo(from, to));
  }

  private bool InRange(Vector3 from, Vector3 to, float maxRange)
  {
    if (GetVectorFromTo(from, to).sqrMagnitude < maxRange * maxRange) return true;
    return false;
  }

  private Node GetCheapestNodeFromList(List<Node> nodes)
  {
    Node cheapastNode = null;
    float cost = float.MaxValue;
    foreach (Node n in nodes)
    {
      if (n.h < cost)
      {
        cost = n.h;
        cheapastNode = n;
      }
    }
    return cheapastNode;
  }

  private List<Vector3> BacktrackNodes(Node node)
  {
    List<Vector3> path = new List<Vector3>();
    while (node.prevNode != null)
    {
      path.Insert(0, node.vec);
      node = node.prevNode;
    }
    return path;
  }
}


class Node
{

  public Vector3 vec;
  public float g = 0, h = 0, f = 0;
  public Node prevNode = null;
  public Node(Vector3 vecRef)
  {
    vec = vecRef;
  }

  public void UpdateValues(float heuristic)
  {
    Node prev = prevNode;
    g = 0;
    while (prev != null)
    {
      g += prevNode.vec.magnitude;
      prev = prev.prevNode;
    }
    h = heuristic;
    f = g + h;
  }

  int CompareLength(Node o)
  {
    if (vec.magnitude < o.vec.magnitude) return -1;
    else if (vec.magnitude > o.vec.magnitude) return 1;
    return 0;
  }
}