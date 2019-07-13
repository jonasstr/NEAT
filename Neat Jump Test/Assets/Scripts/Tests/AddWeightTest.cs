using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddWeightTest : MonoBehaviour {

	void Start () {

        Debug.Log("Starting add weight test...");
        var visualizer = GameObject.Find("NetworkVisualizer").GetComponent<NetworkVisualizer>();
        var ga = GameObject.FindObjectOfType<GA>();

        var genome = new Genome();

        genome.weights.Add(1, new Weight(1, 1, 4, 1f, true, false));
        genome.weights.Add(2, new Weight(2, 2, 4, 1f, false, false));
        genome.weights.Add(3, new Weight(3, 3, 4, 1f, true, false));
        genome.weights.Add(4, new Weight(4, 2, 5, 1f, true, false));
        genome.weights.Add(5, new Weight(5, 5, 4, 1f, true, false));
        genome.weights.Add(8, new Weight(8, 1, 5, 1f, true, false));

        genome.neurons.Add(1, new Neuron(1, Neuron.Type.INPUT, 0f, 0f));
        genome.neurons.Add(2, new Neuron(2, Neuron.Type.INPUT, 0.5f, 0f));
        genome.neurons.Add(3, new Neuron(3, Neuron.Type.INPUT, 1f, 0f));
        genome.neurons.Add(4, new Neuron(4, Neuron.Type.OUTPUT, 0.5f, 1f));
        genome.neurons.Add(5, new Neuron(5, Neuron.Type.HIDDEN, 0.5f, 0.5f));

        genome.innovationDB.CreateInnovation(InnovationDB.Innovation.Type.NEW_WEIGHT, 1, 4, Neuron.Type.NONE, 0f, 0f);
        genome.innovationDB.CreateInnovation(InnovationDB.Innovation.Type.NEW_WEIGHT, 2, 4, Neuron.Type.NONE, 0f, 0f);
        genome.innovationDB.CreateInnovation(InnovationDB.Innovation.Type.NEW_WEIGHT, 3, 4, Neuron.Type.NONE, 0f, 0f);
        genome.innovationDB.CreateInnovation(InnovationDB.Innovation.Type.NEW_WEIGHT, 2, 5, Neuron.Type.NONE, 0f, 0f);
        genome.innovationDB.CreateInnovation(InnovationDB.Innovation.Type.NEW_WEIGHT, 5, 4, Neuron.Type.NONE, 0f, 0f);
        genome.innovationDB.CreateInnovation(InnovationDB.Innovation.Type.NEW_WEIGHT, 1, 5, Neuron.Type.NONE, 0f, 0f);

        genome.innovationDB.CreateInnovation(InnovationDB.Innovation.Type.NEW_NEURON, -1, -1, Neuron.Type.INPUT, 0f, 0f);
        genome.innovationDB.CreateInnovation(InnovationDB.Innovation.Type.NEW_NEURON, -1, -1, Neuron.Type.INPUT, 0f, 0f);
        genome.innovationDB.CreateInnovation(InnovationDB.Innovation.Type.NEW_NEURON, -1, -1, Neuron.Type.INPUT, 0f, 0f);
        genome.innovationDB.CreateInnovation(InnovationDB.Innovation.Type.NEW_NEURON, -1, -1, Neuron.Type.OUTPUT, 0f, 0f);
        genome.innovationDB.CreateInnovation(InnovationDB.Innovation.Type.NEW_NEURON, -1, -1, Neuron.Type.HIDDEN, 0f, 0f);

        foreach (var w in genome.weights)
            Debug.Log(w);

        genome.MutateAddWeight();
        visualizer.Draw(genome.neurons, genome.weights);
    }
}
