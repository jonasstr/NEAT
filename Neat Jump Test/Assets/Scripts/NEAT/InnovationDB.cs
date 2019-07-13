using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InnovationDB : MonoBehaviour {

    public List<Innovation> innovations;
    private int currentInnovID;
    private int currentNeuronID;

    public static InnovationDB instance;
    void Awake() {

        if (instance != null && instance != this) {
            Destroy(this.gameObject);
        }
        else instance = this;
        DontDestroyOnLoad(this);

        innovations = new List<Innovation>();
    }

    public Innovation CreateInnovation(Innovation.Type innovationType, int neuronIn, int neuronOut, Neuron.Type neuronType, float splitX, float splitY) {
        int neuronID = innovationType == Innovation.Type.NEW_NEURON ? NextNeuronID() : -1;
        var innovation = new Innovation(NextInnovID(), innovationType, neuronIn, neuronOut, neuronID, neuronType, splitX, splitY);
        innovations.Add(innovation);
        return innovation;
    }

    public int CheckNeuronStartInnovation(int neuronID) {
        foreach (var inn in innovations) {
            if (inn.neuronID == neuronID)
                return inn.ID;
        }
        return -1;
    }

    public int CheckNeuronInnovation(int fromNeuron, int toNeuron) {
        foreach (var inn in innovations) {
            if (inn.innovationType == Innovation.Type.NEW_NEURON && inn.neuronIn == fromNeuron && inn.neuronOut == toNeuron)
                return inn.ID;
        }
        return -1;
    }

    public int CheckWeightInnovation(int fromNeuron, int toNeuron) {
        foreach (var inn in innovations) {
            if (inn.innovationType == Innovation.Type.NEW_WEIGHT && inn.neuronIn == fromNeuron && inn.neuronOut == toNeuron)
                return inn.ID;
        }
        return -1;
    }

    public int GetNeuronID(int innovID) {
        foreach (var inn in innovations) {
            if (inn.ID == innovID)
                return inn.neuronID;
        }
        return -1;
    }

    public int NextNeuronID() {
        return ++currentNeuronID;
    }

    private int NextInnovID() {
        return ++currentInnovID;
    }

    [System.Serializable]
    public class Innovation {

        public int ID;
        public enum Type {
            NEW_NEURON, NEW_WEIGHT
        }
        public Type innovationType;
        public int neuronIn, neuronOut;
        public int neuronID;
        public Neuron.Type neuronType;
        public float splitX, splitY;

        public Innovation(int ID, Type innovationType, int neuronIn, int neuronOut, int neuronID, Neuron.Type neuronType, float splitX, float splitY) {
            this.ID = ID;
            this.innovationType = innovationType;
            this.neuronIn = neuronIn;
            this.neuronOut = neuronOut;
            this.neuronID = neuronID;
            this.neuronType = neuronType;
            this.splitX = splitX;
            this.splitY = splitY;
        }

        public override string ToString() {
            return "Innov ID: " + ID + ", innov type" + innovationType + ", from Neuron: " + neuronIn + ", to Neuron: "
                + neuronOut + " neuron ID " + neuronID;
        }

        public override bool Equals(object obj) {
            var innovation = obj as Innovation;
            return innovation != null &&
                   ID == innovation.ID &&
                   innovationType == innovation.innovationType &&
                   neuronIn == innovation.neuronIn &&
                   neuronOut == innovation.neuronOut &&
                   neuronID == innovation.neuronID &&
                   neuronType == innovation.neuronType &&
                   splitX == innovation.splitX &&
                   splitY == innovation.splitY;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}
