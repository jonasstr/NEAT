using UnityEngine;
using System.Linq;

public static class Extensions {

    public static int GetRandomElement(params float[] elements) {

        if (elements.Sum() != 1f) Debug.LogError("RandomElement: Sum of probabilities must be 100%!");

        float diceRoll = Random.value;
        float cumulative = 0f;

        for (int i = 0; i < elements.Length; i++) {

            cumulative += elements[i];
            if (diceRoll < cumulative) {
                return i;
            }
        }
        Debug.LogError("RandomElement: should not return -1!");
        return -1;
    }
}
