using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
public class Goap : MonoBehaviour
{
    public static IEnumerable<GoapAction> Execute(GoapState from, GoapState to, Func<GoapState, bool> satisfies,
        Func<GoapState, float> h, IEnumerable<GoapAction> actions)
    {
        var watchdog = 200;
        var seq = AStarNormal<GoapState>.Run(
            from,
            to,
            (curr, goal) => h(curr),
            satisfies,
            curr =>
            {
                if (watchdog == 0)
                    return Enumerable.Empty<AStarNormal<GoapState>.Arc>();
                else
                    watchdog--;
                return actions.Where(a => a.Preconditions(curr))
                    .Aggregate(new FList<AStarNormal<GoapState>.Arc>(), (possibleList, action) =>
                    {
                        var newState = new GoapState(curr);
                        newState = action.Effects(newState);
                        newState.generatingAction = action;
                        newState.step = curr.step + 1;
                        return possibleList + new AStarNormal<GoapState>.Arc(newState, action.Cost);
                    });
            });
        if (seq == null)
        {
            Debug.Log("Imposible planear");
            return null;
        }
        foreach (var act in seq.Skip(1)) Debug.Log(act);
        Debug.Log("WATCHDOG " + watchdog);
        return seq.Skip(1).Select(x => x.generatingAction);
    }
}