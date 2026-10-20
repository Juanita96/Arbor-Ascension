using UnityEngine;

public static class UIUtils
{
    // functions
    public static string KitLabel(Kit AlchemistKit)
    {
        // name shown in the ui for each kit
        switch (AlchemistKit)
        {
            case Kit.Ataque:
                return "ATTACK";
            case Kit.Apoyo:
                return "SUPPORT";
            case Kit.Defensa:
                return "DEFENSE";
            default:
                return "RESERVE";
        }
    }

    public static Color ElementColor(Element AlchemistElement)
    {
        // color used to show each element
        switch (AlchemistElement)
        {
            case Element.Fuego:
                return new Color(0.92f, 0.45f, 0.30f);
            case Element.Agua:
                return new Color(0.38f, 0.62f, 0.95f);
            default:
                return new Color(0.45f, 0.80f, 0.42f);
        }
    }

    public static string ElementLabel(Element AlchemistElement)
    {
        // name shown in the ui for each element
        switch (AlchemistElement)
        {
            case Element.Fuego:
                return "FIRE";
            case Element.Agua:
                return "WATER";
            default:
                return "GRASS";
        }
    }

    public static string ResourceLabel(ResourceType Type)
    {
        // name shown in the ui for each resource
        switch (Type)
        {
            case ResourceType.Sulfur:
                return "SULPHUR";
            case ResourceType.Mercurius:
                return "MERCURIUS";
            default:
                return "SALT";
        }
    }

    public static string CostLabel(ResourceCost Cost)
    {
        return Cost.Sulfur + " SULPHUR - " + Cost.Mercurius + " MERCURIUS - " + Cost.Sal + " SALT";
    }

    public static void ClearChildren(Transform Parent)
    {
        // detaching before destroying so the layout doesn't count them this frame
        for (int i = Parent.childCount - 1; i >= 0; i--)
        {
            Transform Child = Parent.GetChild(i);
            Child.SetParent(null, false);
            Object.Destroy(Child.gameObject);
        }
    }
}
