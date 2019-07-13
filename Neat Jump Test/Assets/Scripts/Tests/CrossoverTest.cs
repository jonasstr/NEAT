using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossoverTest : MonoBehaviour {

	void Start() {

        Debug.Log("Starting crossover test...");
        var visualizer = GameObject.Find("NetworkVisualizer").GetComponent<NetworkVisualizer>();

        var parentA = new Genome();
        var parentB = new Genome();

        parentA.weights.Add(1, new Weight(1, 1, 4, 1f, true, false));
        parentA.weights.Add(2, new Weight(2, 2, 4, 1f, false, false));
        parentA.weights.Add(3, new Weight(3, 3, 4, 1f, true, false));
        parentA.weights.Add(4, new Weight(4, 2, 5, 1f, true, false));
        parentA.weights.Add(5, new Weight(5, 5, 4, 1f, true, false));
        parentA.weights.Add(8, new Weight(8, 1, 5, 1f, true, false));

        parentA.neurons.Add(1, new Neuron(1, Neuron.Type.INPUT, 0f, 0f));
        parentA.neurons.Add(2, new Neuron(2, Neuron.Type.INPUT, 0.5f, 0f));
        parentA.neurons.Add(3, new Neuron(3, Neuron.Type.INPUT, 1f, 0f));
        parentA.neurons.Add(4, new Neuron(4, Neuron.Type.OUTPUT, 0.5f, 1f));
        parentA.neurons.Add(5, new Neuron(5, Neuron.Type.HIDDEN, 0.5f, 0.5f));

        parentB.weights.Add(1, new Weight(1, 1, 4, 1f, true, false));
        parentB.weights.Add(2, new Weight(2, 2, 4, 1f, false, false));
        parentB.weights.Add(3, new Weight(3, 3, 4, 1f, true, false));
        parentB.weights.Add(4, new Weight(4, 2, 5, 1f, true, false));
        parentB.weights.Add(5, new Weight(5, 5, 4, 1f, false, false));
        parentB.weights.Add(6, new Weight(6, 5, 6, 1f, true, false));
        parentB.weights.Add(7, new Weight(7, 6, 4, 1f, true, false));
        parentB.weights.Add(9, new Weight(9, 3, 5, 1f, true, false));
        parentB.weights.Add(10, new Weight(10, 1, 6, 1f, true, false));

        parentB.neurons.Add(1, new Neuron(1, Neuron.Type.INPUT, 0f, 0f));
        parentB.neurons.Add(2, new Neuron(2, Neuron.Type.INPUT, 0.5f, 0f));
        parentB.neurons.Add(3, new Neuron(3, Neuron.Type.INPUT, 1f, 0f));
        parentB.neurons.Add(4, new Neuron(4, Neuron.Type.OUTPUT, 0.5f, 1f));
        parentB.neurons.Add(5, new Neuron(5, Neuron.Type.HIDDEN, 0.75f, 0.25f));
        parentB.neurons.Add(6, new Neuron(6, Neuron.Type.HIDDEN, 0.5f, 0.5f));
        parentB.fitness = 1f;

        parentA.innovationDB.CreateInnovation(InnovationDB.Innovation.Type.NEW_WEIGHT, 1, 4, Neuron.Type.NONE, 0f, 0f);
        parentA.innovationDB.CreateInnovation(InnovationDB.Innovation.Type.NEW_WEIGHT, 2, 4, Neuron.Type.NONE, 0f, 0f);
        parentA.innovationDB.CreateInnovation(InnovationDB.Innovation.Type.NEW_WEIGHT, 3, 4, Neuron.Type.NONE, 0f, 0f);
        parentA.innovationDB.CreateInnovation(InnovationDB.Innovation.Type.NEW_WEIGHT, 2, 5, Neuron.Type.NONE, 0f, 0f);
        parentA.innovationDB.CreateInnovation(InnovationDB.Innovation.Type.NEW_WEIGHT, 5, 4, Neuron.Type.NONE, 0f, 0f);
        parentA.innovationDB.CreateInnovation(InnovationDB.Innovation.Type.NEW_WEIGHT, 1, 5, Neuron.Type.NONE, 0f, 0f);
        parentA.innovationDB.CreateInnovation(InnovationDB.Innovation.Type.NEW_WEIGHT, 5, 6, Neuron.Type.NONE, 0f, 0f);
        parentA.innovationDB.CreateInnovation(InnovationDB.Innovation.Type.NEW_WEIGHT, 6, 4, Neuron.Type.NONE, 0f, 0f);
        parentA.innovationDB.CreateInnovation(InnovationDB.Innovation.Type.NEW_WEIGHT, 3, 5, Neuron.Type.NONE, 0f, 0f);
        parentA.innovationDB.CreateInnovation(InnovationDB.Innovation.Type.NEW_WEIGHT, 1, 6, Neuron.Type.NONE, 0f, 0f);

        parentA.innovationDB.CreateInnovation(InnovationDB.Innovation.Type.NEW_NEURON, -1, -1, Neuron.Type.INPUT, 0f, 0f);
        parentA.innovationDB.CreateInnovation(InnovationDB.Innovation.Type.NEW_NEURON, -1, -1, Neuron.Type.INPUT, 0f, 0f);
        parentA.innovationDB.CreateInnovation(InnovationDB.Innovation.Type.NEW_NEURON, -1, -1, Neuron.Type.INPUT, 0f, 0f);
        parentA.innovationDB.CreateInnovation(InnovationDB.Innovation.Type.NEW_NEURON, -1, -1, Neuron.Type.OUTPUT, 0f, 0f);
        parentA.innovationDB.CreateInnovation(InnovationDB.Innovation.Type.NEW_NEURON, -1, -1, Neuron.Type.HIDDEN, 0f, 0f);
        parentA.innovationDB.CreateInnovation(InnovationDB.Innovation.Type.NEW_NEURON, -1, -1, Neuron.Type.HIDDEN, 0f, 0f);

        Debug.Log("Average weight difference: " + parentA.AvgWeightDifference(parentB));
        //var child = parentA.Crossover(parentB);
        //child.MutateAddNeuron();

        // visualizer.Draw(child.neurons, child.weights);
    }
}
