using System.Collections.Generic;

[System.Serializable]
public class ElementalProperties
{
    private Dictionary<ElementType, int> elementalAffinities;

    public ElementalProperties()
    {
        elementalAffinities = new Dictionary<ElementType, int>();
        foreach (ElementType type in System.Enum.GetValues(typeof(ElementType)))
        {
            elementalAffinities[type] = 0;
        }
    }

    public void SetAffinity(ElementType type, int value)
    {
        if (elementalAffinities.ContainsKey(type))
        {
            elementalAffinities[type] = value;
        }
    }

    public int GetAffinity(ElementType type)
    {
        if (elementalAffinities.ContainsKey(type))
        {
            return elementalAffinities[type];
        }

        return 0;
    }
}
