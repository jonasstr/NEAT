using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GA : MonoBehaviour {

    private int nInputs = 2, nOutputs = 2;

    private enum State {
        RandomPopulation,
        BestCreature
    }
    private State state;

    public GameObject creaturePrefab;
    public Vector3 startPos;
    public List<Creature> population;
    public List<Genome> populationGenomes, newPopulationGenomes;
    public List<Species> species;

    public int populationSize = 15;
    public float simulationTime;
    public float compatibilityThreshold = 0.4f;
    public float compatibilityDelta = 0.2f;

    // try to keep the number of species constant at this number
    public int numSpecies = 10;

    // minimum number of genomes in each species after crossover
    public int minSpeciesSize = 2;

    // amount of generations at which species is deleted
    // if it hasn't shown any fitness improvements   
    public int speciesNotImprovedMaxAge = 5;

    // fitness scores of species members are increased or
    // decreased based on the age of the species
    public int speciesAgeBonusThreshold = 10;
    public float speciesAgeBonus = 1.3f;
    public int speciesAgePenaltyThreshold = 30;
    public float speciesAgePenalty = 0.7f;

    public int generation;

    private float time;

    private NetworkVisualizer visualizer;

    void Start() {
        Init();        
    }

    public void Reset() {

        foreach (var p in population)
            Destroy(p.gameObject);
        Init();
    }

    private void Init() {

        time = 0f;
        state = State.RandomPopulation;
        visualizer = GameObject.Find("NetworkVisualizer").GetComponent<NetworkVisualizer>();
        population = new List<Creature>();
        populationGenomes = new List<Genome>();
        species = new List<Species>();

        for (int i = 0; i < populationSize; i++) {
            var creature = Instantiate(creaturePrefab, startPos, Quaternion.identity, transform).GetComponent<Creature>();
            creature.brain = new NeuralNetwork(nInputs, nOutputs);
            population.Add(creature);
            populationGenomes.Add(creature.brain.genome);
            visualizer.Draw(creature.brain.genome.neurons, creature.brain.genome.weights);
        }

        foreach (var creature in population) {
            creature.StartSimulation();
        }
    }

    void Update() {

        if (state == State.BestCreature)
            return;

        time += Time.deltaTime;
        if (time > simulationTime) {            
            time = 0f;
            NewGeneration();
        }
    }

    public void NewGeneration() {

        generation++;

        ResetPopulation();
        //UpdateCompatibilityThreshold();
        UpdateSpecies();
        PutGenomesIntoSpecies();
        CalcAdjustedFitness();
        CalcExpectedOffspring();
        //PerformElitism();
        BreedExpectedOffspring();
        FillUpPopulation();
        CreateNewPopulation();
    }

    public void SaveBest() {

        float maxFitness = population.Max(x => x.brain.genome.fitness);
        var best = population.Where(x => x.brain.genome.fitness == maxFitness).FirstOrDefault();
        SaveManager.instance.Save(best.brain.genome.weights, best.brain.genome.neurons);
    }

    public void LoadBest() {

        if (SaveManager.instance.Load()) {

            time = 0f;
            state = State.BestCreature;
            foreach (var p in population)
                if (p != null)
                    Destroy(p.gameObject);
            population = new List<Creature>();

            var creature = Instantiate(creaturePrefab, startPos, Quaternion.identity, transform).GetComponent<Creature>();
            creature.brain = new NeuralNetwork(new Genome(SaveManager.instance.GetNeurons(), SaveManager.instance.GetWeights()));
            population.Add(creature);
            visualizer.Draw(creature.brain.genome.neurons, creature.brain.genome.weights);
            creature.StartSimulation();
        }
        else Debug.LogError("No network found!");
    }

    private void ResetPopulation() {

        newPopulationGenomes = new List<Genome>();
        foreach (var s in species) {
            s.Reset();
        }
    }

    private void UpdateCompatibilityThreshold() {

        // update compatibility threshold
        if (species.Count > 0) {
            if (species.Count < numSpecies)
                compatibilityThreshold -= compatibilityDelta;
            else if (species.Count > numSpecies)
                compatibilityThreshold += compatibilityDelta;
            compatibilityThreshold = Mathf.Max(compatibilityThreshold, compatibilityDelta);
        }
    }

    private void PutGenomesIntoSpecies() {
        
        foreach (var genome in populationGenomes) {

            bool foundSpecies = false;
            foreach (var s in species) {
                // creature is compatible with representative
                if (genome.CompatibilityDistance(s.representative) < compatibilityThreshold) {
                    foundSpecies = true;
                    s.AddGenome(genome);
                    break;
                }
            }
            if (!foundSpecies) {
                var newSpecies = new Species(genome);
                species.Add(newSpecies);
            }
        }
        
        // remove empty species
        species.RemoveAll(y => y.members.Count == 0);
    }

    private void UpdateSpecies() {

        // delete species if it has made no fitness improvement
        foreach (var s in species) {
            if (!s.FitnessImproved(speciesNotImprovedMaxAge)) {
                species.Remove(s);
                break;
            }
            
            foreach (var genome in s.members) {

                // boost young species
                if (s.age < speciesAgeBonusThreshold) {
                    genome.fitness *= speciesAgeBonus;
                }

                // punish older species
                if (s.age >= speciesAgePenaltyThreshold) {
                    genome.fitness *= speciesAgePenalty;
                }
            }
        }
    }

    private void CalcAdjustedFitness() {

        foreach (var s in species) {
            foreach (var genome in s.members) {
                genome.adjustedFitness = genome.fitness / s.members.Count;
            }
        }        
    }
    
    // calculates adjusted fitness for the entire population
    private float CalculateAvgAdjustedFitness() {

        float sum = 0f;
        foreach (var s in species) {
            sum += s.CalcTotalAdjustedFitness();            
        }
        if (populationGenomes.Count != populationSize)
            Debug.LogError("populationGenomes.Count != populationSize");
        return sum / populationGenomes.Count;
    }

    private void CalcExpectedOffspring() {

        float totalAvgFitness = CalculateAvgAdjustedFitness();
        float totalExpectedOffspring = 0f;

        foreach (var s in species) {
            foreach (var genome in s.members) {
                genome.expectedOffspring = genome.adjustedFitness / totalAvgFitness;
                s.expectedOffspring += genome.expectedOffspring;
            }
        }
    }

    private void BreedExpectedOffspring() {

        int sum = 0;
        int totalOffspringAdded = 0;
        foreach (var s in species) {

            int offspringAdded = 0;
            int speciesOffspring = Mathf.RoundToInt(s.expectedOffspring);
            sum += speciesOffspring;
            bool performedElitism = false;
            Genome child;

            while (offspringAdded < speciesOffspring) {

                if (totalOffspringAdded >= populationSize)
                    break;

                if (!performedElitism) {
                    // per species elitism
                    child = s.BestMember();
                    performedElitism = true;
                }

                else {
                    if (s.members.Count == 1)
                        child = s.SelectGenome();

                    else {

                        var parentA = s.SelectGenome();
                        var parentB = s.SelectGenome();

                        int numTries = 5;
                        while (parentA.Equals(parentB) && numTries > 0) {
                            parentA = s.SelectGenome();
                            parentB = s.SelectGenome();
                            numTries--;
                        }
                        child = parentA.Equals(parentB) ? parentA : parentA.Crossover(parentB);
                    }
                }

                child.Mutate();
                newPopulationGenomes.Add(child);
                visualizer.Draw(child.neurons, child.weights);

                offspringAdded++;
                totalOffspringAdded++;
            }
        }
    }

    private void FillUpPopulation() {

        while (newPopulationGenomes.Count < populationSize) {

            Debug.LogError("while: " + newPopulationGenomes.Count);
            var species = SelectSpecies();
            var parentA = species.SelectGenome();
            var parentB = species.SelectGenome();

            var child = parentA.Crossover(parentB);
            child.Mutate();

            newPopulationGenomes.Add(child);
            visualizer.Draw(child.neurons, child.weights);
        }
    }

    private void CreateNewPopulation() {

        InnovationDB.instance.innovations.Clear();
        populationGenomes = newPopulationGenomes;

        for (int i = 0; i < population.Count; i++) {
            population[i].gameObject.SetActive(true);
            population[i].gameObject.name = "Creature #" + (i+1);
            population[i].transform.position = startPos;
            population[i].body.velocity = new Vector2(0f, 0f);
            // assign new neural network to creature
            population[i].brain = new NeuralNetwork(populationGenomes[i]);            
        }

        visualizer.index = populationSize - 1;
        visualizer.DrawNext();

        if (newPopulationGenomes.Count > populationSize) {
            Debug.LogError("New pop count = " + newPopulationGenomes.Count + "!");
        }        
    }

    private Species SelectSpecies() {

        float totalAdjustedFitnessSum = 0f;
        foreach (var s in species)
            totalAdjustedFitnessSum += s.CalcTotalAdjustedFitness();

        float slice = Random.Range(0f, totalAdjustedFitnessSum);
        float total = 0f;

        foreach (var s in species) {
            total += s.totalAdjustedFitness;
            if (total > slice) {
                return s;
            }
        }
        return null;
    }
}