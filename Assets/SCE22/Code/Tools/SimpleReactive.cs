using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SimpleReactive;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SimpleReactive
{
    /// <summary>
    /// This class represents a simple implementation of the React-style approach to data storage.
    /// </summary>
    /// <typeparam name="T">The type of the stored reactive value.</typeparam>
    [System.Serializable]
    public class ReactiveVar<T> : IDisposable, IReadOnlyReactiveVar<T>
    {
        [SerializeField] private T _value;

        // Remove 'readonly' so we can lazy-initialize it if needed, 
        // though the Property approach below is safer.
        [NonSerialized] private IEqualityComparer<T> _comparer;

        // SAFE ACCESSOR: Handles cases where Unity serialization skipped the constructor
        private IEqualityComparer<T> Comparer => _comparer ?? EqualityComparer<T>.Default;

        public Action<T, T> BeforeChange;
        public Action<T, T> Changed;
        public Action<T, T> AfterChange;
        public Action EmptyInfoChanged;

        public ReactiveVar(T value, IEqualityComparer<T> comparer = null)
        {
            _value = value;
            _comparer = comparer ?? EqualityComparer<T>.Default;
        }

        // Implicit default constructor for Unity serialization compatibility
        public ReactiveVar() { }

        event Action<T, T> IReadOnlyReactiveVar<T>.BeforeChange
        {
            add => BeforeChange += value;
            remove => BeforeChange -= value;
        }

        event Action<T, T> IReadOnlyReactiveVar<T>.Changed
        {
            add => Changed += value;
            remove => Changed -= value;
        }

        event Action<T, T> IReadOnlyReactiveVar<T>.AfterChange
        {
            add => AfterChange += value;
            remove => AfterChange -= value;
        }

        event Action IReadOnlyReactiveVar<T>.EmptyInfoChanged
        {
            add => EmptyInfoChanged += value;
            remove => EmptyInfoChanged -= value;
        }

        public T Value
        {
            get => _value;
            set
            {
                // FIX: Use the 'Comparer' property, not the field directly
                if (Comparer.Equals(value, _value)) return;

                var old = _value;

                BeforeChange?.Invoke(old, value);
                _value = value;
                Changed?.Invoke(old, _value);
                AfterChange?.Invoke(old, value);
                EmptyInfoChanged?.Invoke();
            }
        }

        public T ReadOnlyValue => _value;

        public static implicit operator T(ReactiveVar<T> reactiveVar) => reactiveVar != null ? reactiveVar._value : default;

        // FIX: Use Comparer property in operators too
        public static bool operator ==(ReactiveVar<T> left, T right)
            => left is null ? right is null : left.Comparer.Equals(left._value, right);

        public static bool operator !=(ReactiveVar<T> left, T right) => !(left == right);

        public static bool operator ==(T left, ReactiveVar<T> right)
            => right is null ? left is null : right.Comparer.Equals(left, right._value);

        public static bool operator !=(T left, ReactiveVar<T> right) => !(left == right);

        public static bool operator ==(ReactiveVar<T> left, ReactiveVar<T> right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Comparer.Equals(left._value, right._value);
        }

        public static bool operator !=(ReactiveVar<T> left, ReactiveVar<T> right) => !(left == right);

        public override bool Equals(object obj)
            => obj is ReactiveVar<T> other && Comparer.Equals(_value, other._value);

        public override int GetHashCode() => Comparer.GetHashCode(_value);

        public void Dispose()
        {
            BeforeChange = null;
            Changed = null;
            AfterChange = null;
            EmptyInfoChanged = null;
            GC.SuppressFinalize(this);
        }

        public override string ToString() => _value?.ToString() ?? "null";
    }

    [System.Serializable]
    public class ReactiveList<T> : IReadOnlyReactiveList<T>
    {
        [field: SerializeField] protected List<T> m_list = new();

        public Action<T> OnChanged;
        public Action<T> OnAdd;
        public Action<T> OnRemove;

        public ReactiveList() { }
        public ReactiveList(T single)
        {
            m_list.Add(single);
        }
        public ReactiveList(ICollection<T> range)
        {
            m_list.AddRange(range);
        }

        event Action<T> IReadOnlyReactiveList<T>.OnChanged
        {
            add => OnChanged += value;
            remove => OnChanged -= value;
        }

        // FIX: The original code unsubscribed from OnRemove inside the OnAdd property!
        event Action<T> IReadOnlyReactiveList<T>.OnAdd
        {
            add => OnAdd += value;
            remove => OnAdd -= value; // Fixed
        }

        event Action<T> IReadOnlyReactiveList<T>.OnRemove
        {
            add => OnRemove += value;
            remove => OnRemove -= value;
        }

        public bool Contains(T obj) => m_list.Contains(obj);

        public IReadOnlyList<T> List => m_list;

        public virtual void Add(T item)
        {
            if (item == null) return;

            m_list.Add(item);
            OnAdd?.Invoke(item);
            OnChanged?.Invoke(item);
        }

        public virtual void Remove(T item)
        {
            if (item == null) return;

            if (!m_list.Contains(item)) return;

            m_list.Remove(item);
            OnRemove?.Invoke(item);
            OnChanged?.Invoke(item);
        }

        public int Count => m_list.Count;

        public IReadOnlyCollection<T> ReadonlyList => m_list;

        public void Clear()
        {
            if (m_list.Any())
                foreach (var item in new List<T>(m_list))
                    this.Remove(item);
        }

        public virtual void Remove(int i)
        {
            if (i >= 0 && i < m_list.Count)
            {
                Remove(m_list[i]);
            }
        }
    }
}

public interface IReadOnlyReactiveVar<T>
{
    event System.Action<T, T> BeforeChange;
    event System.Action<T, T> Changed;
    event System.Action<T, T> AfterChange;
    event System.Action EmptyInfoChanged;

    T ReadOnlyValue { get; }
}

public interface IReadOnlyReactiveList<T>
{
    IReadOnlyCollection<T> ReadonlyList { get; }

    event System.Action<T> OnChanged;
    event System.Action<T> OnAdd;
    event System.Action<T> OnRemove;
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReactiveVar<>))]
public class ReactiveVarDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var valueProp = property.FindPropertyRelative("_value");

        if (valueProp != null)
        {
            EditorGUI.PropertyField(position, valueProp, label, true);
        }
        else
        {
            EditorGUI.LabelField(position, label.text, "ReactiveVar: _value not found");
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var valueProp = property.FindPropertyRelative("_value");
        return valueProp != null ? EditorGUI.GetPropertyHeight(valueProp, label, true) : EditorGUIUtility.singleLineHeight;
    }
}
#endif