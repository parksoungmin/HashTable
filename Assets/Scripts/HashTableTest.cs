using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HashTableTest : MonoBehaviour
{
    private void Start()
    {
        var hashTable = new ChainingHashTable<int, int>();
        {
            for (int i = 0; i < 100; i++)
            {

                hashTable.Add(i, Random.Range(0, 100));
            }
            for (int i = 0; i < 100; i++)
            {
                Debug.Log(hashTable[i]);
            }
        }
    }
}
