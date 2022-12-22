using System.Collections.Generic;
using System.Linq;

public class GoapState
{
    public WorldState worldState;


    public GoapAction generatingAction = null;
    public int step = 0;

    #region CONSTRUCTOR
    public GoapState(GoapAction gen = null)
    {
        generatingAction = gen;
        worldState = new WorldState()
        {
            valuesBool = new Dictionary<string, bool>(), // Muy importane inicializarlo en este caso
            valuesInt = new Dictionary<string, int>(),
            valuesFloat = new Dictionary<string, float>(),
            valuesString = new Dictionary<string, string>()
        };
    }

    public GoapState(GoapState source, GoapAction gen = null)
    {
        worldState = source.worldState.Clone();
        generatingAction = gen;
    }
    #endregion


    public override bool Equals(object obj)
    {
        var result =
            obj is GoapState other
            && other.generatingAction == generatingAction
            && other.worldState.valuesBool.Count == worldState.valuesBool.Count
            && other.worldState.valuesBool.All(kv => kv.In(worldState.valuesBool))
            && other.worldState.valuesInt.Count == worldState.valuesInt.Count
            && other.worldState.valuesInt.All(kv => kv.In(worldState.valuesInt))
            && other.worldState.valuesFloat.Count == worldState.valuesFloat.Count
            && other.worldState.valuesFloat.All(kv => kv.In(worldState.valuesFloat))
            && other.worldState.valuesString.Count == worldState.valuesString.Count
            && other.worldState.valuesString.All(kv => kv.In(worldState.valuesString));
        return result;
    }

    public override int GetHashCode()
    {
        return (worldState.valuesBool.Count +
                //worldState.valuesInt.Count +
                worldState.valuesFloat.Count +
                worldState.valuesString.Count) == 0 ? 0 : 31 * (worldState.valuesBool.Count + 
                                                                //worldState.valuesInt.Count +
                                                                worldState.valuesFloat.Count +
                                                                worldState.valuesString.Count) + 31 * 31 * (worldState.valuesBool.First().GetHashCode() +
                                                                                                           //worldState.valuesInt.First().GetHashCode() +
                                                                                                           worldState.valuesFloat.First().GetHashCode() +
                                                                                                           worldState.valuesString.First().GetHashCode());
    }

    public override string ToString()
    {
        var str = "";
        foreach (var kv in worldState.valuesBool.OrderBy(x => x.Key))
        {
            str += (string.Format("{0:12} : {1}\n", kv.Key, kv.Value));
        }
        foreach (var kv in worldState.valuesInt.OrderBy(x => x.Key))
        {
            str += (string.Format("{0:12} : {1}\n", kv.Key, kv.Value));
        }
        foreach (var kv in worldState.valuesFloat.OrderBy(x => x.Key))
        {
            str += (string.Format("{0:12} : {1}\n", kv.Key, kv.Value));
        }
        foreach (var kv in worldState.valuesString.OrderBy(x => x.Key))
        {
            str += (string.Format("{0:12} : {1}\n", kv.Key, kv.Value));
        }
        return ("--->" + (generatingAction != null ? generatingAction.Name : "NULL") + "\n" + str);
    }
}


//Nuestro estado de mundo
//Aca hay una mezcla de lo  anterior con lo nuevo, no necesariamente tiene que haber un diccionario aca adentro
public struct WorldState
{
    public Dictionary<string, bool> valuesBool;
    public Dictionary<string, int> valuesInt;
    public Dictionary<string, float> valuesFloat;
    public Dictionary<string, string> valuesString;

    //MUY IMPORTANTE TENER UN CLONE PARA NO TENER REFENCIAS A LO VIEJO
    public WorldState Clone()
    {
        return new WorldState()
        {
            valuesBool = this.valuesBool.ToDictionary(kv => kv.Key, kv => kv.Value),
            valuesInt = this.valuesInt.ToDictionary(kv => kv.Key, kv => kv.Value),
            valuesFloat = this.valuesFloat.ToDictionary(kv => kv.Key, kv => kv.Value),
            valuesString = this.valuesString.ToDictionary(kv => kv.Key, kv => kv.Value),
        };
    }
}
