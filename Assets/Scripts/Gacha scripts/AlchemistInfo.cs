using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New card", menuName = "Alchemist")]
public class AlchemistInfo : ScriptableObject
{
    public Sprite image;
    public string charName;
    private void Awake()
    {
            
    }
}
