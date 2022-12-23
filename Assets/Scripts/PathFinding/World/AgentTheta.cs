using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AgentTheta : MonoBehaviour
{
    [Header("Pathfinding Settings")] [Tooltip("La máscara correspondiente al obstáculo")] [SerializeField]
    private LayerMask obstacleMask;
    [Tooltip("La máscara correspondiente al waypoint")] [SerializeField]
    private LayerMask nodeMask;
    [Tooltip("La distancia maxima para detectar un nodo cercano a la propia posicion")] [SerializeField]
    private float _distanceDetection;
    [Header("Gizmos settings")] [SerializeField]
    private float radius;
    [SerializeField] private Vector3 offset;
    private Node init;
    private Node finit;
    private Vector3 finPos;
    private List<Node> _list;
    private Theta<Node> _theta = new Theta<Node>();
    public List<Node> GetPathFinding(Node init, Node finit)
    {
        this.init = init;
        this.finit = finit;
        finPos = finit.transform.position;
        return _theta.Run(init, Satisfies, GetNeighbours, GetCost, Heuristic, InSight);
    }
    public List<Node> GetPathFinding(Vector3 init, Vector3 finPos)
    {
        this.init = GetNearestNodeToTarget(init, finPos);
        finit = GetNearestNodeToTarget(finPos, init);
        this.finPos = finPos;
        _list = _theta.Run(this.init, Satisfies, GetNeighbours, GetCost, Heuristic, InSight);
        return FilterStartAndEndPoints(_list, finPos);
    }
    private List<Node> FilterStartAndEndPoints(List<Node> list, Vector3 finPos)
    {
        Vector3 dir;
        while (list.Count > 2)
        {
            dir = list[1].transform.position - transform.position;
            if (!Physics.Raycast(transform.position, dir.normalized, dir.magnitude, obstacleMask))
                list.RemoveAt(0);
            else
                break;
        }
        for (var i = list.Count - 2; i >= 0; i--)
        {
            dir = finPos - list[i].transform.position;
            if (!Physics.Raycast(list[i].transform.position, dir.normalized, dir.magnitude, obstacleMask))
                list.RemoveAt(i + 1);
            else
                break;
        }
        return list;
    }
    private Node GetNearestNodeToTarget(Vector3 start, Vector3 target)
    {
        var distance = 0f;
        Node closestNode = null;
        var nodes = Physics.OverlapSphere(start, _distanceDetection, nodeMask);
        if (nodes == null) return null;
        for (var i = 0; i < nodes.Length; i++)
        {
            var newDistance = Vector3.Distance(nodes[i].transform.position, target);
            if (closestNode == null || newDistance < distance)
            {
                closestNode = nodes[i].GetComponent<Node>();
                distance = newDistance;
            }
        }
        return closestNode;
    }
    private bool InSight(Node gP, Node gC)
    {
        var dir = gC.transform.position - gP.transform.position;
        if (Physics.Raycast(gP.transform.position, dir.normalized, dir.magnitude, obstacleMask)) return false;
        return true;
    }
    private float Heuristic(Node curr)
    {
        return Vector3.Distance(curr.transform.position, finPos);
    }
    private float GetCost(Node from, Node to)
    {
        return Vector3.Distance(from.transform.position, to.transform.position);
    }
    private List<Node> GetNeighbours(Node curr)
    {
        return curr.Neighbours;
    }
    private bool Satisfies(Node curr)
    {
        return curr == finit;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (init != null)
            Gizmos.DrawSphere(init.transform.position + offset, radius);
        Gizmos.DrawSphere(finPos, radius);
        if (_list != null)
        {
            Gizmos.color = Color.blue;
            foreach (var item in _list)
                if (item != init && item != finit)
                    Gizmos.DrawSphere(item.transform.position + offset, radius);
        }
    }
}