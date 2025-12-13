using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public sealed class Damage
{
    public int Value;
    public Entity Author;

    public Damage(int value, Entity author)
    {
        Value = value;
        Author = author;
    }
}
