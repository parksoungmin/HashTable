using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//1. 선형 탐사법(Linear Probing) : (hash(key) + i) % size
//2. 제곱 탐사법(Quadratic Probing) : (hash(key) + i²) % size
//3. 이중 해싱(Double Hashing) : (hash1(key) + i * hash2(key)) % size

public enum OpenAddressingStrategy
{
    LinearProbing,
    Quadratic,
    DoubleHashing
}

public class OpenAddressingHashTable<TKey, TValue> : IDictionary<TKey, TValue>, IHashTable<TKey, TValue>
{
    private const int DefaultCapacity = 16;
    private const float LoadFactor = 0.6f;

    private KeyValuePair<TKey, TValue>[] table;
    private bool[] occupied;
    private bool[] deleted;
    private int size;
    private int count;
    private OpenAddressingStrategy strategy;

    public OpenAddressingHashTable() : this(DefaultCapacity, OpenAddressingStrategy.LinearProbing)
    {
    }

    public OpenAddressingHashTable(OpenAddressingStrategy strategy) : this(DefaultCapacity, strategy)
    {
    }

    public OpenAddressingHashTable(int capacity, OpenAddressingStrategy strategy) : this(capacity)
    {
        this.strategy = strategy;
    }

    public OpenAddressingHashTable(int capacity)
    {
        size = capacity;
        table = new KeyValuePair<TKey, TValue>[size];
        occupied = new bool[size];
        deleted = new bool[size];
        count = 0;
    }

    private int GetIndex(TKey key)
    {
        return GetIndex(key, size);
    }

    private int GetIndex(TKey key, int s)
    {
        if (key == null)
        {
            throw new ArgumentException(nameof(key));
        }

        int hash = key.GetHashCode();
        return Mathf.Abs(hash) % s;
    }

    public TValue this[TKey key]
    {
        get
        {
            if (TryGetValue(key, out TValue value))
            {
                return value;
            }

            throw new KeyNotFoundException("키 없음");
        }
        set
        {
            if (key == null)
            {
                throw new ArgumentException(nameof(key));
            }

            if (ContainsKey(key))
            {
                throw new ArgumentException("키 중복");
            }

            if ((float)count / size > LoadFactor)
            {
                Resize();
            }

            var indexEnumerator = GetIndexEnumerator(key);
            while (indexEnumerator.MoveNext())
            {
                int currentIndex = indexEnumerator.Current;

                if (!occupied[currentIndex])
                {
                    table[currentIndex] = new KeyValuePair<TKey, TValue>(key, value);
                    occupied[currentIndex] = true;
                    deleted[currentIndex] = false;
                    ++count;
                    return;
                }
            }
        }
    }

    public ICollection<TKey> Keys
    {
        get
        {
            var keys = new List<TKey>(count);
            for (int i = 0; i < size; ++i)
            {
                if (occupied[i])
                {
                    keys[count] = table[i].Key;
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
            for (int i = 0; i < size; ++i)
            {
                if (occupied[i])
                {
                    values[count] = table[i].Value;
                }
            }
            return values;
        }
    }

    public int Count => count;

    public bool IsReadOnly => false;

    public void Add(TKey key, TValue value)
    {
        if (key == null)
        {
            throw new ArgumentException(nameof(key));
        }

        if (ContainsKey(key))
        {
            throw new ArgumentException("키 중복");
        }

        if ((float)count / size > LoadFactor)
        {
            Resize();
        }

        var indexEnumerator = GetIndexEnumerator(key);
        while (indexEnumerator.MoveNext())
        {
            int currentIndex = indexEnumerator.Current;

            if (!occupied[currentIndex])
            {
                table[currentIndex] = new KeyValuePair<TKey, TValue>(key, value);
                occupied[currentIndex] = true;
                deleted[currentIndex] = false;
                ++count;
                return;
            }
        }
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        for (int i = 0; i < size; ++i)
        {
            occupied[i] = false;
            deleted[i] = false;
        }
        count = 0;
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return ContainsKey(item.Key);
    }

    public bool ContainsKey(TKey key)
    {
        if (key == null)
        {
            throw new ArgumentException(nameof(key));
        }

        var indexEnumerator = GetIndexEnumerator(key);

        while (indexEnumerator.MoveNext())
        {
            int index = indexEnumerator.Current;
            if (occupied[index] && table[index].Key.Equals(key))
            {
                return true;
            }
            if (!deleted[index] && !occupied[index])
            {
                return false;
            }
        }
        return false;

        //int index = GetIndex(key);

        //int tryCount = 0;
        //int nextIndex = GetNextIndex(index, tryCount, key);

        //while (occupied[nextIndex] || deleted[nextIndex])
        //{
        //    //todo
        //    if (occupied[nextIndex] && table[nextIndex].Key.Equals(key))
        //    {
        //        return true;
        //    }
        //    ++tryCount;
        //    nextIndex = GetNextIndex(index, tryCount, key);
        //    if (nextIndex == index)
        //    {
        //        return false;
        //    }
        //}
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
            array[currentIndex] = kvp;
            ++currentIndex;
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
        {
            throw new ArgumentException(nameof(key));
        }

        var indexEnumerator = GetIndexEnumerator(key);

        while (indexEnumerator.MoveNext())
        {
            int index = indexEnumerator.Current;
            if (occupied[index])
            {
                if (table[index].Key.Equals(key))
                {
                    occupied[index] = false;
                    deleted[index] = true;
                    --count;
                    return true;
                }
            }
            else if (!deleted[index])
            {
                return false;
            }
        }

        //int index = GetIndex(key);

        //int nextIndex = index;
        //int tryCount = 0;

        //while (occupied[nextIndex] || deleted[nextIndex])
        //{
        //    //todo
        //    if (occupied[nextIndex] && table[nextIndex].Key.Equals(key))
        //    {
        //        occupied[nextIndex] = false;
        //        deleted[nextIndex] = true;
        //        --count;
        //        return true;
        //    }
        //    ++tryCount;
        //    nextIndex = GetNextIndex(index, tryCount, key);
        //    if (nextIndex == index)
        //    {
        //        break;
        //    }
        //}

        return false;
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        return Remove(item.Key);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (key == null)
        {
            throw new ArgumentException(nameof(key));
        }

        var indexEnumerator = GetIndexEnumerator(key);

        while (indexEnumerator.MoveNext())
        {
            int index = indexEnumerator.Current;
            if (occupied[index] && table[index].Key.Equals(key))
            {
                value = table[index].Value;
                return true;
            }
            if (!deleted[index] && !occupied[index])
            {
                value = default(TValue);
                return false;
            }
        }

        value = default(TValue);
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private void Resize()
    {
        int newSize = size * 2;
        var newTable = new KeyValuePair<TKey, TValue>[newSize];
        var newOccupied = new bool[newSize];
        var newDeleted = new bool[newSize];

        for (int i = 0; i < size; ++i)
        {
            if (occupied[i])
            {
                var indexEnumerator = GetIndexEnumerator(table[i].Key, newSize);

                while (indexEnumerator.MoveNext())
                {
                    int index = indexEnumerator.Current;
                    if (!newOccupied[index])
                    {
                        newTable[index] = table[i];
                        newOccupied[index] = true;
                        break;
                    }
                }
            }
        }

        size = newSize;
        table = newTable;
        occupied = newOccupied;
        deleted = newDeleted;
    }

    private int GetSecondaryHash(TKey key, int arraySize)
    {
        int hash = key.GetHashCode();
        return 1 + (Math.Abs(hash) % (arraySize - 1));
    }

    private IEnumerator<int> GetIndexEnumerator(TKey key, int arraySize)
    {
        int index = GetIndex(key, arraySize);
        int tryCount = 0;
        int secondary = (strategy == OpenAddressingStrategy.DoubleHashing)
                        ? GetSecondaryHash(key, arraySize) : 0;

        while (tryCount < arraySize)
        {
            switch (strategy)
            {
                case OpenAddressingStrategy.LinearProbing:
                    yield return (index + tryCount) % arraySize;
                    break;
                case OpenAddressingStrategy.Quadratic:
                    yield return (index + (tryCount * tryCount)) % arraySize;
                    break;
                case OpenAddressingStrategy.DoubleHashing:
                    yield return (index + (tryCount * secondary)) % arraySize;
                    break;
                default:
                    throw new ArgumentException($"{strategy}");
            }
            ++tryCount;
        }
    }

    private IEnumerator<int> GetIndexEnumerator(TKey key)
    {
        return GetIndexEnumerator(key, size);
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
