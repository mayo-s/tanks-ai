using System.Collections.Generic;
using UnityEngine;

public class TankAI : MonoBehaviour
{
  public bool m_AIbusy = false;
  public List<Vector3> FindPath(Vector3 fromVec, Vector3 toVec)
  {
    m_AIbusy = true;
    Debug.Log("TankAI is busy");
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
      if (InRange(current.vec, toVec, 2f))
      {
        // path found
        Debug.Log("Path found");
        path = BacktrackNodes(current);
        break;
      }
      List<Node> nextNodes = FindNextPossibleNodes(current);
      foreach (Node n in nextNodes)
      {
        Vector3 startRay = current.vec;
        startRay.y = 0.1f;
        Vector3 endRay = n.vec;
        endRay.y = 0.1f;
        // TODO find optimal raycast max distance
        if (!(Physics.Raycast(startRay, endRay, 4)) && !ListContainsNode(closed, n))
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
        else Debug.DrawRay(startRay, endRay, Color.red);
      }
    }
    printPath(path);
    m_AIbusy = false;
    return path;
  }

  private void printPath(List<Vector3> path)
  {

    string route = "Path found - " + path.Count + " nodes\n";
    Vector3 cubesize = new Vector3(0.5f, 0.5f, 0.5f);
    foreach (Vector3 v in path)
    {
      route += v + "\n";
      GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
      cube.transform.position = v;
      cube.transform.localScale = cubesize;
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
      Vector3 nextVec = (Quaternion.Euler(0f, angle, 0f) * new Vector3(1, 0, 0)).normalized + origin;
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