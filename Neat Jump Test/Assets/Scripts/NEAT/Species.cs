using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Species {

    // best performing genome (also used for compatibility distance)
    public Genome representative;
    public List<Genome> members;

    public float allTimeMaxFitness;
    public float totalAdjustedFitness;
    public float expectedOffspring;
    public int age, gensNotImproved;

    public Species(Genome representative) {
        this.representative = representative;
        members = new List<Genome>();
        members.Add(representative);
    }

    public void AddGenome(Genome genome) {
        members.Add(genome);
    }

    public void Reset() {

        totalAdjustedFitness = 0f;
        expectedOffspring = 0f;
        representative = BestMember();
        members.Clear();
        age++;
    }

    public bool FitnessImproved(int maxGenerations) {

        float maxFitness = representative.fitness;
        if (maxFitness <= allTimeMaxFitness) {
            gensNotImproved++;
        }
        else gensNotImproved = 0;
        allTimeMaxFitness = Mathf.Max(allTimeMaxFitness, maxFitness);

        return gensNotImproved < maxGenerations;
    }

    public Genome BestMember() {
        float max = members.Max(x => x.adjustedFitness);
        return members.Where(x => x.adjustedFitness == max).FirstOrDefault();
    }

    public float CalcTotalAdjustedFitness() {
        totalAdjustedFitness = members.Sum(x => x.adjustedFitness);
        return totalAdjustedFitness;
    }

    public Genome SelectGenome() {

        float adjustedFitnessSum = 0f;
        foreach (var creature in members)
            adjustedFitnessSum += creature.adjustedFitness;

        float slice = Random.Range(0f, adjustedFitnessSum);
        float total = 0f;

        foreach (var genome in members) {
            total += genome.adjustedFitness;
            if (total > slice)
                return genome;
        }

        return null;
    }
}
