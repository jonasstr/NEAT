using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class NeuralNetwork {

    public Genome genome;
    public float[] inputs;
    public bool initialized;
    private bool preActivated;

    public NeuralNetwork(int nInputs, int nOutputs) {

        genome = new Genome();
        var neuronsIn = new Neuron[nInputs];
        for (int i = 0; i < nInputs; i++) {
            neuronsIn[i] = genome.CreateStartNeuron(Neuron.Type.INPUT, i+1, (float) i/nInputs, 0f);
        }
        var neuronBias = genome.CreateStartNeuron(Neuron.Type.BIAS, nInputs+1, 1f, 0f);
        var neuronsOut = new Neuron[nOutputs];
        for (int i = 0; i < nOutputs; i++) {
            neuronsOut[i] = genome.CreateStartNeuron(Neuron.Type.OUTPUT, i+nInputs+2, (float) i/nOutputs, 1f);
        }

        // create input connections
        for (int i = 0; i < nInputs; i++) {
            for (int j = 0; j < nOutputs; j++) {
                genome.CreateStartWeight(neuronsIn[i].ID, neuronsOut[j].ID, Random.Range(-2f, 2f));
            }
        }

        // create bias connections
        for (int i = 0; i < nOutputs; i++) {
            genome.CreateStartWeight(neuronBias.ID, neuronsOut[i].ID, Random.Range(-2f, 2f));
        }        
        // set value of bias neuron
        genome.neurons.ElementAt(nInputs).Value.SetValue(1f);
        initialized = true;
    }

    public NeuralNetwork(Genome genome) {
        this.genome = genome;
        initialized = true;
    }
    
    private float ActivationFn(float x) {
        return 1f / (1f + Mathf.Exp(-x));
    }

    public float[] ComputeOutputs(float[] inputs) {

        for (int i = 0; i < inputs.Length; i++) {
            // set input neuron values
            //Debug.LogError("i = " + i + ", inp length: " + inputs.Length + ", neurons count = " + genome.neurons.Count);
            genome.neurons.ElementAt(i).Value.SetValue(inputs[i]);;
        }

        var outputNeurons = genome.neurons.Where(x => x.Value.type == Neuron.Type.OUTPUT).ToArray();
        float[] outputs = new float[outputNeurons.Length];

        if (!preActivated) {
            for (int i = 0; i < outputNeurons.Length; i++) {
                PreActivateNeuron(outputNeurons.ElementAt(i).Key);
            }
            preActivated = true;
        }

        for (int i = 0; i < outputNeurons.Length; i++) {
            outputs[i] = ActivateNeuron(outputNeurons.ElementAt(i).Key);
        }
        return outputs;
    }

    private float PreActivateNeuron(int id) {

        var neuron = genome.neurons[id];
        float inputsSum = 0f;

        if (neuron.type != Neuron.Type.INPUT) {
            var weightsToNeuron = genome.weights.Where(x => x.Value.neuronOut == id && x.Value.enabled && !x.Value.recurrent).ToArray();
            for (int i = 0; i < weightsToNeuron.Length; i++) {
                var weight = weightsToNeuron.ElementAt(i).Value;
                int neuronIn = weight.neuronIn;
                float value = weight.value;
                inputsSum += PreActivateNeuron(neuronIn) * value;
            }
        }    
        else return neuron.GetValue();
        neuron.SetValue(ActivationFn(inputsSum));
        return neuron.GetValue();
    }

    private float ActivateNeuron(int id) {

        var neuron = genome.neurons[id];
        float inputsSum = 0f;

        if (neuron.type != Neuron.Type.INPUT) {
            var weightsToNeuron = genome.weights.Where(x => x.Value.neuronOut == id && x.Value.enabled).ToArray();
            for (int i = 0; i < weightsToNeuron.Length; i++) {
                var weight = weightsToNeuron.ElementAt(i).Value;
                if (weight.recurrent) {
                    int neuronIn = weight.neuronIn;
                    float neuronInValue = genome.neurons[neuronIn].GetValue();
                    float value = weight.value;
                    inputsSum += neuronInValue * value;
                }
                else {
                    int neuronIn = weight.neuronIn;
                    float value = weight.value;
                    inputsSum += ActivateNeuron(neuronIn) * value;
                }
            }
        }
        else return neuron.GetValue();
        neuron.SetValue(ActivationFn(inputsSum));
        return neuron.GetValue();
    }
}
