using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Damage
{
    public Entity Author;
    public int Value;

    public Damage(Entity author, int value)
    {
        Author = author;
        Value = value;
    }
}
