using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public interface IHashTable<TKey, TValue>
{
    public int GetArrayIndex(TKey key);
}