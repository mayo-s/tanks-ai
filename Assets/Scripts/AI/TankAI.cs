using System.Collections.Generic;
using UnityEngine;

public class TankAI : MonoBehaviour
{

  public bool m_AIon = false;
  public bool m_AIbusy = false;

  public bool m_AIaiming = false;
  public List<Vector3> m_Path = new List<Vector3>();
  public LayerMask layermask;
  public void FindPath(Vector3 fromVec, Vector3 toVec)
  {
    m_AIbusy = true;
    // better double check
    m_Path.Clear();

    // a* implementation
    List<Node> open = new List<Node>();
    List<Node> closed = new List<Node>();
    Node from = new Node(fromVec);
    Node to = new Node(toVec);
    open.Add(from);
    List<Vector3> path = new List<Vector3>();
    while (open.Count > 0)
    {
      Node current = GetCheapestNodeFromList(open);
      open.Remove(current);
      closed.Add(current);
      // TODO find optimal distance to target
      if (InRange(current.vec, toVec, 1f) || path.Count >= 20)
      {
        // path found
        Debug.Log("Path found");
        path = BacktrackNodes(current);
        break;
      }
      List<Node> nextNodes = FindNextPossibleNodes(current);
      foreach (Node n in nextNodes)
      {
        Vector3 start = current.vec;
        start.y = 0f;
        Vector3 end = n.vec;
        end.y = 0f;
        float maxDistance = 10f;
        // TODO find optimal raycast max distance
        // if (!(Physics.Raycast(start, end, maxDistance, layermask)) && !ListContainsNode(closed, n))
        if(!Physics.Linecast(start, end) && !ListContainsNode(closed, n))
        {
          if (!ListContainsNode(open, n))
          {
            n.prevNode = current;
            n.UpdateValues(GetDistanceFromTo(n.vec, toVec));
            open.Add(n);
          }
          else
          {
            if (n.g > current.g + 1)
            {
              n.prevNode = current;
              n.UpdateValues(GetDistanceFromTo(n.vec, toVec));
            }
          }
        }
      }
    }
    // printPath(path);
    m_AIbusy = false;
    m_Path = path;
  }

  private void printPath(List<Vector3> path)
  {
    string route = "Path found - " + path.Count + " nodes\n";
    foreach (Vector3 v in path)
    {
      route += v + "\n";
      GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
      Destroy(cube.GetComponent<Collider>());
      cube.transform.position = v;
      cube.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
    }
    route += "____________END____________";
    Debug.Log(route);
  }

  private List<Node> FindNextPossibleNodes(Node node)
  {
    List<Node> newNodes = new List<Node>();
    Vector3 origin = node.vec;
    for (float angle = 0f; angle < 360f; angle += 45f)
    {
      Vector3 nextVec = (Quaternion.Euler(0f, angle, 0f) * new Vector3(1f, 0f, 0f)).normalized + origin;
      // Debug.Log("origin " + origin + " next " + nextVec + "angle " + angle);
      newNodes.Add(new Node(nextVec));
    }
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
    return (GetVectorFromTo(from, to).sqrMagnitude < maxRange * maxRange);
  }

  private Node GetCheapestNodeFromList(List<Node> nodes)
  {
    Node cheapastNode = null;
    float cost = float.MaxValue;
    foreach (Node n in nodes)
    {
      if (n.f < cost)
      {
        cost = n.f;
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
    // remove first, because it's current position
    path.RemoveAt(0);
    return path;
  }

  private bool ListContainsNode(List<Node> nodes, Node node)
  {
    foreach (Node n in nodes)
    {
      if (InRange(n.vec, node.vec, 0.5f))
      {
        return true;
      }
    }
    return false;
  }

  public void activateAI()
  {
    m_AIon = !m_AIon;
    Debug.Log("AI " + m_AIon);
    // Clear path when turning AI off
    if (!m_AIon) m_Path.Clear();
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
    g = prevNode.g + 1;
    h = heuristic;
    f = g + h;
  }

  public bool CompareTo(Node o)
  {
    if (vec.x == o.vec.x && vec.y == o.vec.y && vec.z == o.vec.z) return true;
    return false;
  }
}