using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

public class SimpleHashTable<TKey, TValue> : IDictionary<TKey, TValue>, IHashTable<TKey, TValue>
{
    private const int DefaulttCapacity = 16;
    private const float LoadFactor = 0.75f;

    private KeyValuePair<TKey, TValue>[] table; //
    private bool[] occupied;//이 필드의 값으로 KeyVaLUpair 형의 값이 있나 없나 확인할꺼임.
    private int size;
    private int count;

    public SimpleHashTable()
    {
        size = DefaulttCapacity;
        table = new KeyValuePair<TKey, TValue>[size];
        occupied = new bool[size];
        count = 0;
    }
    public int GetIndex(TKey key) // 해쉬 함수에서 
    {
        return GetIndex(key, size);
    }
    public int GetIndex(TKey key, int s) // 해쉬 함수에서 
    {
        if (key == null)
            throw new ArgumentException(nameof(key));

        int hash = key.GetHashCode();
        return Mathf.Abs(hash) % size;
    }

    public TValue this[TKey key]
    {
        get
        {
            if (TryGetValue(key, out TValue value))
            {
                return value;
            }
            throw new KeyNotFoundException($"키 없음: {key}");
        }
        set
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if ((float)count / size > LoadFactor)
            {
                Resize();
            }

            int index = GetIndex(key);
            if (!occupied[index])
            {
                table[index] = new KeyValuePair<TKey, TValue>(key, value);
                occupied[index] = true;
                count++;
            }
            else if (table[index].Key.Equals(key)) // 중복확인
            {
                throw new ArgumentNullException("키 중복!");

            }
            else
            {
                throw new InvalidOperationException("해시 충돌!"); // 충돌부분 임시 예외
            }
        }
    }

    public ICollection<TKey> Keys
    {
        get
        {
            var keys = new List<TKey>(count);
            for (int i = 0; i < size; i++)
            {
                if (occupied[i])
                {
                    keys.Add(table[i].Key);
                }
            }
            return keys;
        }
    }

    public ICollection<TValue> Values =>
        Enumerable.Range(0, size)
        .Where(x => occupied[x])
        .Select(x => table[x].Value)
        .ToArray();

    public int Count => count;

    public bool IsReadOnly => false;

    private void Resize()
    {
        int newSize = size * 2;
        var newTable = new KeyValuePair<TKey, TValue>[newSize];
        var newOccupied = new bool[newSize];
        for (int i = 0; i < size; i++)
        {
            if (occupied[i])
            {
                int newIndex = GetIndex(table[i].Key, newSize);
                if (newOccupied[newIndex])
                {
                    throw new InvalidOperationException("해시 충돌!");
                }
                newTable[newIndex] = table[i];
                newOccupied[newIndex] = true;
            }
        }

        size = newSize;
        table = newTable;
        occupied = newOccupied;
    }


    public void Add(TKey key, TValue value)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        if ((float)count / size > LoadFactor)
        {
            Resize();
        }
        int index = GetIndex(key);
        if (!occupied[index])
        {
            table[index] = new KeyValuePair<TKey, TValue>(key, value);
            occupied[index] = true;
            count++;
        }
        else if (table[index].Key.Equals(key)) // 중복확인
        {
            table[index] = new KeyValuePair<TKey, TValue>(key, value);
        }
        else
        {
            throw new InvalidOperationException("해시 충돌!"); // 충돌부분 임시 예외
        }

    }
    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        for (int i = 0; size > i; i++)
        {
            occupied[i] = false;
        }
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return ContainsKey(item.Key);
    }

    public bool ContainsKey(TKey key) // 이키가 들어와 있으면 true 없으면 false
    {
        int index = GetIndex(key);
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        return occupied[index] && table[index].Equals(key);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));

        if (arrayIndex < 0 || arrayIndex >= array.Length - 1)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));


        if (array.Length - arrayIndex < Count)
            throw new ArgumentException("공간 부족");

        int currentindex = arrayIndex;
        foreach (var kvp in this)
        {
            array[currentindex++] = kvp;
        }
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        for (int i = 0; i < size; ++i)
        {
            if (occupied[i])
            {
                yield return table[i];
            }
        }
    }

    public bool Remove(TKey key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        int index = GetIndex(key);
        if (occupied[index] && table[index].Equals(key))
        {
            occupied[index] = false;
            count--;
            return true;
        }
        return false;
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        //if (item.Key == null)
        //    throw new ArgumentNullException(nameof(item.Key));
        //int index = GetIndex(item.Key);
        //table[index].Equals(item.Key);
        //if (occupied[index] && table[index].Equals(item))
        //{
        //    table[index] = default;
        //    occupied[index] = false;
        //    count--;
        //    return true;
        //}
        //return false;
        return Remove(item.Key);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        int index = GetIndex(key);
        if (occupied[index] && table[index].Key.Equals(key))
        {
            value = table[index].Value;
            return true;
        }

        value = default(TValue);
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int GetArrayIndex(TKey key)
    {
        if (key == null)
        {
            throw new ArgumentException(nameof(key));
        }

        int hash = key.GetHashCode();
        return Mathf.Abs(hash) % size;
    }
}
