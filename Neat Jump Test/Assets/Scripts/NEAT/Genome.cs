using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class Genome {

    public InnovationDB innovationDB;
    public IntWeightDictionary weights;
    public IntNeuronDictionary neurons;

    public float fitness, adjustedFitness;
    public float expectedOffspring;
    public int numTriesToAddWeight = 18;

    // structural mutation probabilities
    public float pAddNeuron = 0.05f;
    public float pAddWeight = 0.08f;

    public float pShiftWeight = 0.25f;
    public float pEnableWeight = 0.02f;
    public float pDisableWeight = 0.04f;
    // probability of a newly found weight to be recurrent
    // (see MutateAddWeight)
    public float pRecurrentWeight = 0.00f;
    public float pWeightReplaced = 0.1f;
    // probability of an inherited gene to be enabled when disabled in both parents
    public float pWeightEnabled = 0.25f;

    // factor by which the old weight is multiplied (between min and max)
    // (see MutateShiftWeight)
    public float weightChangeRelativeMin = 0.97f;
    public float weightChangeRelativeMax = 1.04f;
    
    // coefficients for the compatibility distance
    public float c1 = 1.0f, c2 = 2.0f, c3 = 0.6f;
    public int N = 1;

    public Genome() {
        innovationDB = InnovationDB.instance;
        weights = new IntWeightDictionary();
        neurons = new IntNeuronDictionary();
    }

    public Genome(IntNeuronDictionary neurons, IntWeightDictionary weights) {
        innovationDB = InnovationDB.instance;
        this.neurons = neurons;
        this.weights = weights;
    }

    // create neuron at network initialization
    public Neuron CreateStartNeuron(Neuron.Type type, int neuronID, float splitX, float splitY) {

        var oldNeuronInnovID = innovationDB.CheckNeuronStartInnovation(neuronID);
        Neuron newNeuron;

        // new innovation
        if (oldNeuronInnovID == -1) {
            var neuronInnov = innovationDB.CreateInnovation(InnovationDB.Innovation.Type.NEW_NEURON, -1, -1, type, splitX, splitY);
            newNeuron = new Neuron(neuronInnov.neuronID, type, splitX, splitY);
        }
        else {
            var newNeuronID = innovationDB.GetNeuronID(oldNeuronInnovID);
            newNeuron = new Neuron(newNeuronID, type, splitX, splitY);
        }
        neurons.Add(newNeuron.ID, newNeuron);
        return newNeuron;
    }

    // create weight at network initialization
    public void CreateStartWeight(int neuronIn, int neuronOut, float value) {

        var oldWeightInnovID = innovationDB.CheckWeightInnovation(neuronIn, neuronOut);
        Weight newWeight;

        // new innovation
        if (oldWeightInnovID == -1) {
            var weightInnov = innovationDB.CreateInnovation(InnovationDB.Innovation.Type.NEW_WEIGHT, neuronIn, neuronOut, Neuron.Type.NONE, -1, -1);
            newWeight = new Weight(weightInnov.ID, neuronIn, neuronOut, value, true, false);
        }
        else newWeight = new Weight(oldWeightInnovID, neuronIn, neuronOut, value, true, false);
        
        weights.Add(newWeight.innovID, newWeight);
    }

    public float CompatibilityDistance(Genome other) {

        float excessGenes = NumberOfExcessGenes(other);
        float disjointGenes = NumberOfDisjointGenes(other);
        float avgWeight = AvgWeightDifference(other);

        float dist = (c1 * excessGenes) / N + (c2 * disjointGenes) / N + c3 * avgWeight;
        return dist;
    }

    public int NumberOfDisjointGenes(Genome other) {

        var parentA = this;
        var parentB = other;

        int numDisjointGenes = 0;
        int parentAIndex = 0, parentBIndex = 0;
        Weight parentAWeight = null, parentBWeight = null;

        while (parentAIndex < parentA.weights.Count && parentBIndex < parentB.weights.Count) {

            if (parentAIndex < parentA.weights.Count)
                parentAWeight = parentA.weights.ElementAt(parentAIndex).Value.Copy();
            if (parentBIndex < parentB.weights.Count)
                parentBWeight = parentB.weights.ElementAt(parentBIndex).Value.Copy();
            
            if (parentAWeight.innovID < parentBWeight.innovID && parentBIndex != parentB.weights.Count) {
                numDisjointGenes++;
                parentAIndex++;
            }
            else if (parentBWeight.innovID < parentAWeight.innovID && parentAIndex != parentA.weights.Count) {
                numDisjointGenes++;
                parentBIndex++;
            }
            else if (parentAWeight.innovID == parentBWeight.innovID) {
                parentAIndex++;
                parentBIndex++;
            }
        }
        return numDisjointGenes;
    }

    public int NumberOfExcessGenes (Genome other) {

        var parentA = this;
        var parentB = other;

        int numExcessGenes = 0;
        int parentAMaxInnovID = parentA.weights.ElementAt(parentA.weights.Count - 1).Value.innovID;
        int parentBMaxInnovID = parentB.weights.ElementAt(parentB.weights.Count - 1).Value.innovID;
        Genome higherInnovGenome = null;
        int lowerInnovGenomeMaxID = -1;

        if (parentAMaxInnovID > parentBMaxInnovID) {
            higherInnovGenome = parentA;
            lowerInnovGenomeMaxID = parentBMaxInnovID;
        }
        else if (parentBMaxInnovID > parentAMaxInnovID) {
            higherInnovGenome = parentB;
            lowerInnovGenomeMaxID = parentAMaxInnovID;
        }
        // genomes have no excess genes
        else return 0;

        int index = higherInnovGenome.weights.Count - 1;
        Weight selectedWeight = null;

        for (int i = index; i >= 0; i--) {
            selectedWeight = higherInnovGenome.weights.ElementAt(i).Value;
            if (selectedWeight.innovID > lowerInnovGenomeMaxID)
                numExcessGenes++;
            else return numExcessGenes;
        }
        return 0;
    }

    public float AvgWeightDifference(Genome other) {

        var parentA = this;
        var parentB = other;

        int matchingGenes = 0;
        float weightDiff = 0f;        
        int parentAIndex = 0, parentBIndex = 0;
        Weight parentAWeight = null, parentBWeight = null;

        while (parentAIndex < parentA.weights.Count && parentBIndex < parentB.weights.Count) {

            if (parentAIndex < parentA.weights.Count)
                parentAWeight = parentA.weights.ElementAt(parentAIndex).Value.Copy();
            if (parentBIndex < parentB.weights.Count)
                parentBWeight = parentB.weights.ElementAt(parentBIndex).Value.Copy();

            if (parentAWeight.innovID < parentBWeight.innovID)
                parentAIndex++;
            else if (parentBWeight.innovID < parentAWeight.innovID)
                parentBIndex++;
            else {
                matchingGenes++;
                weightDiff += Mathf.Abs(parentAWeight.value - parentBWeight.value);
                parentAIndex++;
                parentBIndex++;
            }
        }
        return matchingGenes == 0 ? 0 : weightDiff / matchingGenes;
    }

    public Genome Crossover(Genome other) {

        var parentA = this;
        var parentB = other;
        Genome best = new Genome();

        if (parentA.fitness == parentB.fitness) {
            // if same fitness pick shortest genome
            if (parentA.NumGenes() < parentB.NumGenes())
                best = parentA;
            else if (parentB.NumGenes() < parentA.NumGenes())
                best = parentB;
            else {
                int randomElement = Random.Range(0, 2);
                best = randomElement == 0 ? parentA : parentB;
            }
        }

        else if (parentA.fitness > parentB.fitness)
            best = parentA;
        else best = parentB;

        var childWeights = new IntWeightDictionary();
        var childNeurons = new IntNeuronDictionary();
        int parentAIndex = 0, parentBIndex = 0;
        Weight parentAWeight = null, parentBWeight = null;

        // while end of both genes is not reached
        while (parentAIndex < parentA.weights.Count || parentBIndex < parentB.weights.Count) {

            if (parentAIndex < parentA.weights.Count)
                parentAWeight = parentA.weights.ElementAt(parentAIndex).Value.Copy();
            if (parentBIndex < parentB.weights.Count)
                parentBWeight = parentB.weights.ElementAt(parentBIndex).Value.Copy();
            Weight selectedWeight = null;

            if (parentAIndex == parentA.weights.Count && parentBIndex != parentB.weights.Count) {
                // add excess genes
                if (parentB == best)
                    selectedWeight = parentBWeight;
                parentBIndex++;
            }

            else if (parentBIndex == parentB.weights.Count && parentAIndex != parentA.weights.Count) {
                // add excess genes
                if (parentA == best)
                    selectedWeight = parentAWeight;
                parentAIndex++;
            }

            else if (parentAWeight.innovID < parentBWeight.innovID) {
                // add parentA disjoint genes
                if (parentA == best)
                    selectedWeight = parentAWeight;
                parentAIndex++;
            }
            else if (parentBWeight.innovID < parentAWeight.innovID) {
                // add parentB disjoint genes
                if (parentB == best)
                    selectedWeight = parentBWeight;
                parentBIndex++;
            }

            else {
                int randomElement = Random.Range(0, 2);
                selectedWeight = randomElement == 0 ? parentAWeight : parentBWeight;

                if (!parentAWeight.enabled && !parentBWeight.enabled) {

                    float random = Random.value;
                    if (random <= pWeightEnabled) {
                        selectedWeight.enabled = true;
                    }
                }                    

                parentAIndex++;
                parentBIndex++;
            }

            if (selectedWeight != null && !childWeights.ContainsKey(selectedWeight.innovID))
                childWeights.Add(selectedWeight.innovID, selectedWeight);

            if (selectedWeight != null) {
                if (!childNeurons.ContainsKey(selectedWeight.neuronIn)) {
                    Neuron neuron = null;
                    if (parentA.weights.Any(x => x.Value.neuronIn == selectedWeight.neuronIn))
                        neuron = parentA.neurons[selectedWeight.neuronIn].Copy();
                    else if (parentB.weights.Any(x => x.Value.neuronIn == selectedWeight.neuronIn))
                        neuron = parentB.neurons[selectedWeight.neuronIn].Copy();

                    if (neuron == null)
                        Debug.LogError("NEURON IS NULL!");

                    childNeurons.Add(neuron.ID, neuron);
                }

                if (!childNeurons.ContainsKey(selectedWeight.neuronOut)) {
                    Neuron neuron = null;
                    if (parentA.weights.Any(x => x.Value.neuronOut == selectedWeight.neuronOut))
                        neuron = parentA.neurons[selectedWeight.neuronOut].Copy();
                    else if (parentB.weights.Any(x => x.Value.neuronOut == selectedWeight.neuronOut))
                        neuron = parentB.neurons[selectedWeight.neuronOut].Copy();

                    if (neuron == null)
                        Debug.LogError("NEURON IS NULL!");

                    childNeurons.Add(neuron.ID, neuron);
                }
            }                       
        }
        return new Genome(childNeurons, childWeights);
    }

    public void Mutate() {

        int mutationType = Extensions.GetRandomElement(new float[] { pAddNeuron, pAddWeight, 1 - pAddNeuron - pAddWeight });
        // structural mutations
        switch (mutationType) {
            case 0:
                MutateAddNeuron();
                break;
            case 1:
                MutateAddWeight();
                break;
            case 2:
                break;
        }

        // weight mutations
        foreach (var weight in weights) {
            if (Random.value < pShiftWeight)
                MutateShiftWeight(weight.Value);
        }

        if (Random.value < pEnableWeight)
            MutateEnableDisable(true);
        if (Random.value < pDisableWeight)
            MutateEnableDisable(false);
    }

    public void MutateAddNeuron() {

        bool done = false;
        while (!done) {
            int index = Random.Range(0, weights.Count);
            var weight = weights.ElementAt(index).Value;

            if (weight.enabled && !weight.recurrent && neurons[weight.neuronIn].type != Neuron.Type.BIAS) {
                done = true;

                float oldValue = weight.value;
                weight.enabled = false;
                int from = weight.neuronIn;
                int to = weight.neuronOut;

                float newSplitX = (neurons[from].splitX + neurons[to].splitX) / 2f;
                float newSplitY = (neurons[from].splitY + neurons[to].splitY) / 2f;

                // check whether new neuron is already in innovation database
                var oldNeuronInnovID = innovationDB.CheckNeuronInnovation(from, to);

                // new innovation
                if (oldNeuronInnovID == -1) {
                    var neuronInnov = innovationDB.CreateInnovation(InnovationDB.Innovation.Type.NEW_NEURON, from, to, Neuron.Type.HIDDEN, newSplitX, newSplitY);
                    var newNeuron = new Neuron(neuronInnov.neuronID, Neuron.Type.HIDDEN, newSplitX, newSplitY);
                    neurons.Add(newNeuron.ID, newNeuron);

                    var weightInInnov = innovationDB.CreateInnovation(InnovationDB.Innovation.Type.NEW_WEIGHT, from, newNeuron.ID, Neuron.Type.NONE, -1, -1);
                    var weightIn = new Weight(weightInInnov.ID, from, newNeuron.ID, 1f, true, false);
                    weights.Add(weightInInnov.ID, weightIn);

                    var weightOutInnov = innovationDB.CreateInnovation(InnovationDB.Innovation.Type.NEW_WEIGHT, newNeuron.ID, to, Neuron.Type.NONE, -1, -1);
                    var weightOut = new Weight(weightOutInnov.ID, newNeuron.ID, to, oldValue, true, false);
                    weights.Add(weightOut.innovID, weightOut);
                }
                // innovation is already in innovation database
                else {
                    var newNeuronID = innovationDB.GetNeuronID(oldNeuronInnovID);
                    var oldWeightInID = innovationDB.CheckWeightInnovation(from, newNeuronID);
                    var oldWeightOutID = innovationDB.CheckWeightInnovation(newNeuronID, to);

                    if (oldWeightInID == -1 || oldWeightOutID == -1) {
                        Debug.LogError("old innov ID " + oldNeuronInnovID);
                        Debug.LogError("Error in MutateAddNeuron: weights not in database, from " + from + ", to " + to + " newNeuronID " + newNeuronID);
                        return;
                    }
                    var newNeuron = new Neuron(newNeuronID, Neuron.Type.HIDDEN, newSplitX, newSplitY);
                    neurons.Add(newNeuron.ID, newNeuron);

                    var weightIn = new Weight(oldWeightInID, from, newNeuron.ID, 1f, true, false);
                    weights.Add(weightIn.innovID, weightIn);
                    var weightOut = new Weight(oldWeightOutID, newNeuron.ID, to, oldValue, true, false);
                    weights.Add(weightOut.innovID, weightOut);
                }
            }            
        }
    }

    public void MutateAddWeight() {

        int numTries = 0;
        while (numTries < numTriesToAddWeight) {

            var neuronA = neurons.ElementAt(Random.Range(0, neurons.Count)).Value;
            var neuronB = neurons.ElementAt(Random.Range(0, neurons.Count)).Value;

            // repeat until two different neurons have been found
            if (neuronA.ID == neuronB.ID)
                continue;

            if ((neuronA.type == Neuron.Type.INPUT || neuronA.type == Neuron.Type.BIAS) &&
                (neuronB.type == Neuron.Type.INPUT || neuronB.type == Neuron.Type.BIAS)) {
                numTries++;
                // find a new weight
                continue;
            }
            else if (neuronA.type == Neuron.Type.OUTPUT && neuronB.type == Neuron.Type.OUTPUT)  {
                numTries++;
                // find a new weight
                continue;
            }

            bool exit = false;
            foreach (var weight in weights.Values) {

                // weight already exists
                if (weight.neuronIn == neuronA.ID && weight.neuronOut == neuronB.ID ||
                    weight.neuronIn == neuronB.ID && weight.neuronOut == neuronA.ID) {

                    numTries++;
                    exit = true;
                }
                if (exit) {
                    //Debug.Log("Weight already exists: " + weight);
                    break;
                }
            }
            if (exit) {
                // find a new weight
                continue;
            }

            bool recurrent = false;  
            float random = Random.value;
            if (random <= pRecurrentWeight) {

                recurrent = true;
               if (neuronB.splitY > neuronA.splitY) {
                    // make the weight recurrent by swapping the neurons
                    var temp = neuronA;
                    neuronA = neuronB;
                    neuronB = temp;
               }
            }
            else {

                if (neuronA.splitY > neuronB.splitY) {
                    // make the weight non-recurrent
                    var temp = neuronA;
                    neuronA = neuronB;
                    neuronB = temp;
                }
            }
            var oldWeightInnovID = innovationDB.CheckWeightInnovation(neuronA.ID, neuronB.ID);
            float value = Random.Range(-1f, 1f);

            // new innovation
            if (oldWeightInnovID == -1) {
                var weightInnov = innovationDB.CreateInnovation(InnovationDB.Innovation.Type.NEW_WEIGHT, neuronA.ID, neuronB.ID, Neuron.Type.NONE, -1, -1);
                var weight = new Weight(weightInnov.ID, neuronA.ID, neuronB.ID, value, true, recurrent);
                weights.Add(weight.innovID, weight);
            }
            // innovation is already in innovation database
            else {
                var weight = new Weight(oldWeightInnovID, neuronA.ID, neuronB.ID, value, true, recurrent);
                weights.Add(weight.innovID, weight);
            }
            return;
        }
    }

    public void MutateShiftWeight(Weight weight) {

        float random = Random.value;
        if (random <= pWeightReplaced)
            weight.value = Random.Range(-2f, 2f);
        else {
            weight.value *= Random.Range(weightChangeRelativeMin, weightChangeRelativeMax);
        }
    }

    public void MutateEnableDisable(bool enable) {

        var candidates = new IntWeightDictionary();
        for (int i = 0; i < weights.Count; i++) {

            var weight = weights.ElementAt(i).Value;
            if (weight.enabled != enable)
                candidates.Add(i, weight);
        }

        if (candidates.Count == 0)
            return;

        var chosen = candidates.ElementAt(Random.Range(0, candidates.Count)).Value;
        chosen.enabled = enable;
    }

    private void AddNewNeuron(Dictionary<int, Neuron> neurons, int neuronID) {

        if (!neurons.ContainsKey(neuronID))
            neurons.Add(neuronID, new Neuron(neuronID, this.neurons[neuronID].type, this.neurons[neuronID].splitX, this.neurons[neuronID].splitY));
    }

    private int NumGenes() {
        return weights.Count;
    }

    private int HighestInnovID() {

        int id = -1;
        for (int i=0; i < weights.Count; i++) {
            if (weights[i].innovID > id)
                id = weights[i].innovID;
        }
        return id;
    }

    private string WeightString(Weight weight) {
        return weight.innovID + ":" + weight.neuronIn + "," + weight.neuronOut + ", " + weight.enabled + " ";
    }    
}