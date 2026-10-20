using UnityEngine.Serialization;

// variables

// kit of each alchemist, defines its basic attack, active, passive and leader skill
public enum Kit
{
    Ataque,
    Apoyo,
    Defensa,
    Reserva
}

// element of each alchemist, only used for team synergies
public enum Element
{
    Fuego,
    Agua,
    Planta
}

// tria prima resources, given by alchemists when they help defeat an enemy
public enum ResourceType
{
    Sulfur,
    Mercurius,
    Sal
}

// amount of each resource needed to pay for something
[System.Serializable]
public struct ResourceCost
{
    // variables
    [FormerlySerializedAs("Madera")] public int Sulfur;
    [FormerlySerializedAs("Oro")] public int Mercurius;
    [FormerlySerializedAs("Piedra")] public int Sal;

    // functions
    public int Get(ResourceType Type)
    {
        // returning the amount that matches the requested resource
        switch (Type)
        {
            case ResourceType.Sulfur:
                return Sulfur;
            case ResourceType.Mercurius:
                return Mercurius;
            default:
                return Sal;
        }
    }
}
