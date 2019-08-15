using System.Collections.Generic;
using UnityEngine;

public class TankAI : MonoBehaviour
{
  public List<Vector3> FindPath(Vector3 fromVec, Vector3 toVec)
  {
    List<Node> open = new List<Node>();
    List<Node> closed = new List<Node>();
    Node from = new Node(fromVec);
    Node to = new Node(toVec);
    open.Add(from);
    List<Vector3> path = new List<Vector3>();
    float openListG = float.MaxValue;
    while (open.Count > 0)
    {
      Node current = GetCheapestNodeFromList(open);
      open.Remove(current);
      closed.Add(current);
      current.UpdateValues(GetDistanceFromTo(fromVec, toVec));
      // TODO find optimal distance to target
      if (InRange(current.vec, toVec, 6f))
      {
        // path found
        Debug.Log("Path found");
        path = BacktrackNodes(current);
        break;
      }
      else
      {
        List<Node> nextNodes = FindNextPossibleNodes(current);
        foreach (Node n in nextNodes)
        {
          // TODO find optimal raycast max distance
          if (!(Physics.Raycast(transform.position, n.vec, 6)) && !ListContainsNode(closed, n))
          {
            if (!ListContainsNode(open, n))
            {
              n.prevNode = current;
              n.UpdateValues(GetDistanceFromTo(n.vec, toVec));
            }
            else
            {
              if (n.g < openListG)
              {
                n.prevNode = current;
                openListG = n.g;
                n.UpdateValues(GetDistanceFromTo(n.vec, toVec));
              }
            }
          }
        }
      }
    }
    Debug.Log("Path length = " + path.Count);
    return path;
  }

  private List<Node> FindNextPossibleNodes(Node node)
  {
    List<Node> newNodes = new List<Node>();
    Vector3 origin = node.vec;
    for (float angle = 0f; angle < 360f; angle += 45f)
    {
      Vector3 nextVec = Quaternion.Euler(0f, angle, 0f) * origin;
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

  private bool ListContainsNode(List<Node> nodes, Node node){
      foreach(Node n in nodes){
          if(n.CompareTo(node)) return true;
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

  public bool CompareTo(Node o){
      if(vec.x == o.vec.x && vec.y == o.vec.y && vec.z == o.vec.z) return true;
      return false;
  }
}