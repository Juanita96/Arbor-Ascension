using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

// Un asset por alquimista. Lo usan el gacha, el inventario, el armado de equipo y el combate.
// Los valores de cada kit están en la ficha del GDD (4.1.2).
[CreateAssetMenu(fileName = "New card", menuName = "Alchemist")]
public class AlchemistInfo : ScriptableObject
{
    public Sprite image;
    public string charName;

    [Header("Sistemas")]
    public Kit Kit;
    public Element Element;
    [Tooltip("Recurso que aporta al participar en una derrota. El de Reserva no aporta.")]
    public ResourceType Resource;

    [Header("Stats")]
    public float MaxHealth = 100f;
    [Tooltip("Daño del ataque básico. La Reserva no ataca.")]
    [FormerlySerializedAs("Power")] public float BasicDamage = 10f;
    [Tooltip("Intervalo del ataque básico en segundos.")]
    [FormerlySerializedAs("AttackInterval")] public float BasicInterval = 1f;

    [Header("Activa y líder")]
    public float ActiveCooldown = 12f;
    [Tooltip("Ataque: daño de la activa. Apoyo: curación. Defensa y Reserva: no se usa.")]
    public float ActiveValue = 40f;
    [Tooltip("Ataque: daño de la activa como líder. Apoyo: curación como líder. Defensa: HP máximo como líder. Reserva: reducción de cooldown (0.2 = 20%).")]
    public float LeaderValue = 60f;

    [Tooltip("Opcional. Si está vacío se usa el prefab por defecto del GameConfig.")]
    public CharacterUnit Prefab;

    public string DisplayName => string.IsNullOrEmpty(charName) ? name : charName;
    public bool IsReserveOnly => Kit == Kit.Reserva;

    private void Awake()
    {

    }
}
