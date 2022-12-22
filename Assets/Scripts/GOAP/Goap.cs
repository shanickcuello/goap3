﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class Goap : MonoBehaviour
{
    //El satisfies y la heuristica ahora son Funciones externas
	public static IEnumerable<GoapAction> Execute(GoapState from, GoapState to, Func<GoapState, bool> satisfies, Func<GoapState, float> h, IEnumerable<GoapAction> actions)
    {
        int watchdog = 200;

        IEnumerable<GoapState> seq = AStarNormal<GoapState>.Run(
            from,                               //Estado inicial
            to,                                 //Estado final
            (curr,goal)  => h (curr),           //Heuristica. Se pasan nodos actual y final y se calcula la heuristica
            satisfies,                          //Funcion que analiza si satisface o no
            curr =>                             //Funcion para conseguir las acciones posibles
            {
                if (watchdog == 0)
                    return Enumerable.Empty<AStarNormal<GoapState>.Arc>();
                else
                    watchdog--;

                //en este Where se evaluan las precondiciones, al ser un diccionario de <string,bool> solo se chequea que todas las variables concuerdes
                //En caso de ser un Func<...,bool> se utilizaria ese func de cada estado para saber si cumple o no
                return actions.Where(a => a.Preconditions(curr)) // Agregue esto para chequear las precondiuciones puestas  en el Func, Al final deberia quedar solo esta
                              .Aggregate(new FList<AStarNormal<GoapState>.Arc>(), (possibleList, action) =>
                              {
                                  var newState = new GoapState(curr);
                                  newState = action.Effects(newState); // se aplican lso effectos del Func
                                  newState.generatingAction = action;
                                  newState.step = curr.step+1;
                                  return possibleList + new AStarNormal<GoapState>.Arc(newState, action.Cost);
                              });
            });

        if (seq == null)
        {
            Debug.Log("Imposible planear");
            return null;
        }

        foreach (var act in seq.Skip(1))
        {
			Debug.Log(act);
        }

		Debug.Log("WATCHDOG " + watchdog);
		
		return seq.Skip(1).Select(x => x.generatingAction);
	}
}
