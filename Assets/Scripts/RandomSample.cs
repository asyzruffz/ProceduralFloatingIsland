using System.Collections.Generic;
using UnityEngine;

public class RandomSample  {

    List<int> pop = new List<int> ();
    int total;

    public RandomSample (int sample) {
        total = sample;
        Reset ();
    }
	
    public int Next(ref System.Random randomizer) {
        int index = randomizer.Next (0, pop.Count);
        int picked = pop[index];
        pop.RemoveAt (index);
        return picked;
    }

    public void Reset () {
        pop.Clear ();
        for (int i = 0; i < total; i++) {
            pop.Add (i);
        }
    }

    public bool IsEmpty () {
        return pop.Count == 0;
    }
}
