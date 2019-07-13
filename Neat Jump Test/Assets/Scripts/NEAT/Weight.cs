
[System.Serializable]
public class Weight {

    public int innovID;
    public int neuronIn, neuronOut;
    public float value;
    public bool enabled, recurrent;

    public Weight(int innovID, int neuronIn, int neuronOut, float value, bool enabled, bool recurrent) {
        this.innovID = innovID;
        this.neuronIn = neuronIn;
        this.neuronOut = neuronOut;
        this.value = value;
        this.enabled = enabled;
        this.recurrent = recurrent;
    }

    public Weight Copy() {
        return new Weight(innovID, neuronIn, neuronOut, value, enabled, recurrent);
    }

    public override string ToString() {
        return "From neuron: " + this.neuronIn + " to neuron: " + this.neuronOut +
            ", weight: " + this.value + ", enabled: " + this.enabled + ", recurrent: " + this.recurrent;
    }
}
