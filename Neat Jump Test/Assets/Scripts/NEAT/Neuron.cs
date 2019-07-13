
[System.Serializable]
public class Neuron {

    public int ID;
    public enum Type {
        INPUT, BIAS, HIDDEN, OUTPUT, NONE
    }
    public Type type;
    private float value;
    public float splitX, splitY;

    public Neuron(int ID, Type type, float splitX, float splitY) {
        this.ID = ID;
        this.type = type;
        this.splitX = splitX;
        this.splitY = splitY;
    }

    public float GetValue() {
        return this.value;
    }

    public void SetValue(float value) {
        this.value = value;
    }

    public Neuron Copy() {
        return new Neuron(ID, type, splitX, splitY);
    }

    public override string ToString() {
        return "ID: " + this.ID + ", type: " + this.type;
    }
}
