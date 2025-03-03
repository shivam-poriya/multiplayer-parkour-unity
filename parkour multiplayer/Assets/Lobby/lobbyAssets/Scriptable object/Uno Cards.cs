using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnoCards", menuName = "CardsDict")]
public class UnoCards : ScriptableObject
{
    [Header("Store Indices and Corresponding Card Prefabs")]
    public int[] cardIndices;  // Array for indices
    public GameObject[] cardPrefabs;  // Array for card prefabs

    public Dictionary<int, GameObject> unoCards;

    public void InitializeDictionary()
    {
        unoCards = new Dictionary<int, GameObject>();

        for (int i = 0; i < cardIndices.Length; i++)
        {
            if (i < cardPrefabs.Length)
            {
                unoCards.Add(cardIndices[i], cardPrefabs[i]);
            }
        }
    }

    public GameObject GetCard(int index)
    {
        if (unoCards == null)
        {
            InitializeDictionary();
        }

        if (unoCards.ContainsKey(index))
        {
            return unoCards[index];
        }

        return null;
    }
}
