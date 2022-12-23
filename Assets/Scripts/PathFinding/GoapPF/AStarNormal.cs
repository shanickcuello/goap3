﻿using System;
using System.Collections.Generic;
using System.Linq;
using U = Utility;
public class AStarNormal<Node> where Node : class
{
    public class Arc
    {
        public Node endpoint;
        public float cost;
        public Arc(Node ep, float c)
        {
            endpoint = ep;
            cost = c;
        }
    }
    public static IEnumerable<Node> Run
    (
        Node from,
        Node to,
        Func<Node, Node, float> h,
        Func<Node, bool> satisfies,
        Func<Node, IEnumerable<Arc>> expand
    )
    {
        var initialState = new AStarState<Node>();
        initialState.open.Add(from);
        initialState.gs[from] = 0;
        initialState.fs[from] = h(from, to);
        initialState.previous[from] = null;
        initialState.current = from;
        var state = initialState;
        while (state.open.Count > 0 && !state.finished)
        {
            state = state.Clone();
            var candidate = state.open.OrderBy(x => state.fs[x]).First();
            state.current = candidate;
            if (satisfies(candidate))
            {
                U.Log("SATISFIED");
                state.finished = true;
            }
            else
            {
                state.open.Remove(candidate);
                state.closed.Add(candidate);
                var neighbours = expand(candidate);
                if (neighbours == null || !neighbours.Any())
                    continue;
                var gCandidate = state.gs[candidate];
                var debugIterations = 0;
                foreach (var ne in neighbours)
                {
                    debugIterations++;
                    if (ne.endpoint.In(state.closed))
                        continue;
                    var gNeighbour = gCandidate + ne.cost;
                    state.open.Add(ne.endpoint);
                    if (gNeighbour > state.gs.DefaultGet(ne.endpoint, () => gNeighbour))
                        continue;
                    state.previous[ne.endpoint] = candidate;
                    state.gs[ne.endpoint] = gNeighbour;
                    state.fs[ne.endpoint] = gNeighbour + h(ne.endpoint, to);
                }
            }
        }
        if (!state.finished)
            return null;
        var seq =
            U.Generate(state.current, n => state.previous[n])
                .TakeWhile(n => n != null)
                .Reverse();
        return seq;
    }
    private static void DebugGoap(AStarState<Node> state)
    {
        var candidate = state.current;
        U.Log("OPEN SET " + state.open.Aggregate("", (a, x) => a + x.ToString() + "\n\n"));
        U.Log("CLOSED SET " + state.closed.Aggregate("", (a, x) => a + x.ToString() + "\n\n"));
        U.Log("CHOSEN CANDIDATE COST " + state.fs[candidate] + ":" + candidate.ToString());
        if (state is AStarState<GoapState>)
        {
            U.Log("SEQUENCE FOR CANDIDATE" +
                  U.Generate(state.current, n => state.previous[n])
                      .TakeWhile(x => x != null)
                      .Reverse()
                      .Select(x => x as GoapState)
                      .Where(x => x != null && x.generatingAction != null)
                      .Aggregate("", (a, x) => a + "-->" + x.generatingAction.Name)
            );
            var prevs = state.previous as Dictionary<GoapState, GoapState>;
            U.Log("Other candidate chains:\n"
                  + prevs
                      .Select(kv => kv.Key)
                      .Where(y => !prevs.ContainsValue(y))
                      .Aggregate("", (a, y) => a +
                                               U.Generate(y, n => prevs[n])
                                                   .TakeWhile(x => x != null)
                                                   .Reverse()
                                                   .Select(x => x as GoapState)
                                                   .Where(x => x != null && x.generatingAction != null)
                                                   .Aggregate("",
                                                       (a2, x) => a2 + "-->" + x.generatingAction.Name + "(" + x.step +
                                                                  ")")
                                               + " (COST: g" + state.gs[y as Node] + "   f" + state.fs[y as Node] + ")"
                                               + "\n"
                      )
            );
        }
    }
}