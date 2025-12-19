using System.Collections;
using System.Collections.Generic;
using Unity.Android.Types;
using UnityEngine;

[System.Serializable]
public class Damage
{
    public IDamageAuthor Author;
    public float Value;

    public string[] Tags;

    public Damage(IDamageAuthor author, float value, string[] tags)
    {
        Author = author;
        Value = value;

        Tags = tags;
    }
}

public interface IDamageAuthor
{
    public string Label { get; }
    public Damage MakeDamageFor(Entity target);
}