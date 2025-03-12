using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChainingHashTable<TKey, TValue> : IDictionary<TKey, TValue>, IHashTable<TKey, TValue>
{
    private const int DefaulttCapacity = 16;
    private const float LoadFactor = 0.75f;
    LinkedList<KeyValuePair<TKey, TValue>>[] table;
    private int size;
    private int count;

    public ChainingHashTable()
    {
        size = DefaulttCapacity;
        table = new LinkedList<KeyValuePair<TKey, TValue>>[size];
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
            throw new KeyNotFoundException($"키 없음: {key} 벨류 {value}");
        }
        set
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            int index = GetIndex(key);
            if ((float)count / size > LoadFactor)
            {
                Resize();
                index = GetIndex(key);
            }


            foreach (var kvr in table[index])
            {
                if (kvr.Key.Equals(index))
                {
                    Remove(kvr);
                    table[index].AddLast(new KeyValuePair<TKey, TValue>(key, value));
                }
            }

            if (table[index].First.Equals(key))
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
                foreach (var kvr in table[i])
                {
                    keys.Add(kvr.Key);
                }
            }
            return keys;
        }
    }

    public ICollection<TValue> Values
    {
        get
        {
            var values = new List<TValue>(count);
            for (int i = 0; i < size; i++)
            {
                foreach (var kvr in table[i])
                {
                    values.Add(kvr.Value);
                }
            }
            return values;
        }
    }

    public int Count => count;

    public bool IsReadOnly => false;

    private void Resize()
    {
        int newSize = size * 2;
        var newTable = new LinkedList<KeyValuePair<TKey, TValue>>[newSize];

        for (int i = 0; i < size; i++)
        {
            if (table[i] != null)
            {
                foreach (var kvr in table[i])
                {
                    int newIndex = GetIndex(kvr.Key, newSize);  // 새로운 크기에 맞게 재계산
                    if (newTable[newIndex] == null)
                    {
                        newTable[newIndex] = new LinkedList<KeyValuePair<TKey, TValue>>();
                    }
                    newTable[newIndex].AddLast(kvr);
                }
            }
        }

        size = newSize;
        table = newTable;
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
        if (table[index] == null)
        {
            table[index] = new LinkedList<KeyValuePair<TKey, TValue>>();
            table[index].AddLast(new KeyValuePair<TKey, TValue>(key, value));
            count++;
            return;
        }
        foreach (var kvr in table[index])
        {
            if (kvr.Key.Equals(key)) // 중복확인
            {
                throw new ArgumentNullException(nameof(key));
            }
        }
        table[index] = new LinkedList<KeyValuePair<TKey, TValue>>();
        table[index].AddLast(new KeyValuePair<TKey, TValue>(key, value));
        count++;
    }
    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        for (int i = 0; size > i; i++)
        {
            table[i] = null;
        }
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return ContainsKey(item.Key);
    }

    public bool ContainsKey(TKey key) // 이 키가 들어와 있으면 true 없으면 false
    {
        int index = GetIndex(key);
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        foreach (var kvr in table[index])
        {
            if (kvr.Equals(key))
            {
                return true;
            }
        }

        return false;
    }

    public void CopyTo(LinkedList<KeyValuePair<TKey, TValue>>[] array, int arrayIndex)
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

    public IEnumerator<LinkedList<KeyValuePair<TKey, TValue>>> GetEnumerator()
    {
        for (int i = 0; i < size; ++i)
        {
            if (table[i] != null)
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
        foreach (var kvp in table[index])
        {
            if (kvp.Equals(key))
            {
                if (table[index] != null)
                {
                    table[index].Remove(kvp);
                    count--;
                    return true;
                }
            }
        }
        return false;
    }

    public bool Remove(LinkedList<KeyValuePair<TKey, TValue>> item)
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
        foreach (var kvp in item)
        {
            return Remove(kvp.Key);
        }
        return false;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        int index = GetIndex(key);
        if (table[index] != null)
        {
            foreach (var kvp in table[index])
            {
                if (kvp.Key.Equals(key))
                {
                    value = kvp.Value;
                    return true;
                }
            }
        }

        value = default(TValue);
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        if (array == null)
        {
            throw new ArgumentNullException(nameof(array));
        }

        if (arrayIndex < 0 || array.Length <= arrayIndex)
        {
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        }

        if (array.Length < arrayIndex + count)
        {
            throw new ArgumentException("공간 부족");
        }

        int currentIndex = arrayIndex;

        foreach (var kvp in this)
        {
            array[currentIndex] = kvp.First();
            ++currentIndex;
        }
    }
    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        return Remove(item.Key);
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

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
        throw new NotImplementedException();
    }
}
