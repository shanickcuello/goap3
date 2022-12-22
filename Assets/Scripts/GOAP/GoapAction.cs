using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Esta parte transformado en utilizar Funcs, pero por ahora hay una mezcla
/// </summary>
public class GoapAction
{
    public Func<GoapState, bool> Preconditions = delegate { return true; };
    public Func<GoapState, GoapState> Effects;

    public float Cost { get; private set; }
    public ItemType item;
    public string Name { get; private set; }

    public GoapAction(string name)
    {
        this.Name = name;
        Cost = 1f;
    }

    public GoapAction SetCost(float cost)
    {
        if (cost < 1f)
        {
            Debug.Log(string.Format("Warning: Using cost < 1f for '{0}' could yield sub-optimal results", Name));
        }
        this.Cost = cost;
        return this;
    }

    public GoapAction Pre(Func<GoapState, bool> p)
    {
        Preconditions = p;
        return this;
    }

    public GoapAction Effect(Func<GoapState, GoapState> e)
    {
        Effects = e;
        return this;
    }

    public GoapAction SetItem(ItemType type)
    {
        item = type;
        return this;
    }
}
