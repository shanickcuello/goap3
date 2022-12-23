using System;
using UnityEngine;
public class GoapAction
{
    public Func<GoapState, GoapState> Effects;
    public ItemType item;
    public Func<GoapState, bool> Preconditions = delegate { return true; };
    public GoapAction(string name)
    {
        Name = name;
        Cost = 1f;
    }
    public float Cost { get; private set; }
    public string Name { get; }
    public GoapAction SetCost(float cost)
    {
        if (cost < 1f)
            Debug.Log(string.Format("Warning: Using cost < 1f for '{0}' could yield sub-optimal results", Name));
        Cost = cost;
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