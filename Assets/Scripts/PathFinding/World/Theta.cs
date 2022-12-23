using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Theta<T>
{
    public delegate bool Satisfies(T curr);
    public delegate bool InSight(T gP, T gC);
    public delegate List<T> GetNeighbours(T curr);
    public delegate float GetCost(T father, T child);
    public delegate float Heuristic(T current);
    public List<T> Run(T start, Satisfies satisfies, GetNeighbours getNeighbours, GetCost getcost, Heuristic heuristic,
        InSight inSight, int watchDong = 500)
    {
        var cost = new Dictionary<T, float>();
        var parents = new Dictionary<T, T>();
        var pending = new PriorityQueue<T>();
        var visited = new HashSet<T>();
        pending.Enqueue(start, 0);
        cost.Add(start, 0);
        while (!pending.IsEmpty)
        {
            var current = pending.Dequeue();
            watchDong--;
            if (watchDong <= 0) return new List<T>();
            if (satisfies(current)) return ConstructPath(current, parents);
            visited.Add(current);
            var neighbours = getNeighbours(current);
            for (var i = 0; i < neighbours.Count; i++)
            {
                var node = neighbours[i];
                if (visited.Contains(node)) continue;
                T currParent;
                if (parents.ContainsKey(current) && inSight(parents[current], node))
                    currParent = parents[current];
                else
                    currParent = current;
                var nodeCost = getcost(currParent, node);
                var totalCost = cost[currParent] + nodeCost;
                if (cost.ContainsKey(node) && cost[node] < totalCost) continue;
                cost[node] = totalCost;
                parents[node] = currParent;
                pending.Enqueue(node, totalCost + heuristic(node));
            }
        }
        return new List<T>();
    }
    private List<T> ConstructPath(T end, Dictionary<T, T> parents)
    {
        var path = new List<T>();
        path.Add(end);
        while (parents.ContainsKey(path[path.Count - 1]))
        {
            var lastNode = path[path.Count - 1];
            path.Add(parents[lastNode]);
        }
        path.Reverse();
        return path;
    }
    private List<T> ConstructPath2(T end, Dictionary<T, T> parents)
    {
        var path = new List<T>();
        path.Add(end);
        var currentGrandFather = end;
        while (parents.ContainsKey(path[path.Count - 1]))
        {
            var lastNode = path[path.Count - 1];
            path.Add(parents[lastNode]);
        }
        path.Reverse();
        return path;
    }
}