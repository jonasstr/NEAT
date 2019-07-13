using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkVisualizer : MonoBehaviour {

    public GameObject neuronPrefab, weightPrefab;
    public Text textField;

    private List<GameObject> neuronObjects, weightObjects;
    private float[] neuronPositions;
    private new LineRenderer renderer;
    private GA ga;
    public float maxBiasX, maxX, maxY;
    public float lineWidth;

    public int index;

    public void Awake() {
        ga = GameObject.FindObjectOfType<GA>();
        weightObjects = new List<GameObject>();
        neuronPositions = new float[500];
        textField.text = "1 / " + ga.populationSize;
    }

    public void Draw(IntNeuronDictionary neurons, IntWeightDictionary weights) {

        if (neuronObjects != null) {
            foreach (var neuron in neuronObjects)
                Destroy(neuron);
        }

        if (weightObjects != null) { 
            foreach (var weight in weightObjects)
                Destroy(weight);
        }

        neuronObjects = new List<GameObject>();
        foreach (var neuron in neurons.Values) {
            neuronPositions[neuron.ID] = (neuron.splitX * maxX - 0.5f * maxX) * 2f;//Random.Range(-.35f, 1.4f) * maxX - 0.5f * maxX;
            var neuronObject = Instantiate(neuronPrefab, transform.localPosition + new Vector3(neuronPositions[neuron.ID], neuron.splitY * maxY - 0.5f * maxY),
                Quaternion.identity, transform).gameObject;
            neuronObject.name = "Neuron" + neuron.ID;
            neuronObjects.Add(neuronObject);
        }

        foreach (var weight in weights.Values) {

            var connection = Instantiate(weightPrefab, Vector3.zero, Quaternion.identity, transform);
            renderer = connection.GetComponent<LineRenderer>();
            renderer.startWidth = renderer.endWidth = lineWidth * weight.value;
            var color = weight.value < 0f ? Color.red : Color.green;
            color = weight.enabled ? color : Color.black;
            renderer.startColor = renderer.endColor = color;
            weightObjects.Add(connection);

            var splitXNeuronIn = neuronPositions[weight.neuronIn];//Random.Range(0f, 1f) * maxX - 0.5f * maxX;//neurons[weight.neuronIn].splitX * maxX - 0.5f * maxX;
            var splitYNeuronIn = neurons[weight.neuronIn].splitY * maxY - 0.5f * maxY;
            var splitXNeuronOut = neuronPositions[weight.neuronOut];//Random.Range(0f, 1f) * maxX;// - 0.5f * maxX;//neurons[weight.neuronOut].splitX * maxX - 0.5f * maxX;
            var splitYNeuronOut = neurons[weight.neuronOut].splitY * maxY - 0.5f * maxY;
            DrawLine(new Vector3(splitXNeuronIn, splitYNeuronIn) + transform.localPosition, new Vector3(splitXNeuronOut, splitYNeuronOut) + transform.localPosition);
        }
    }

    public void DrawNext() {

        var gen = ga.populationGenomes[index];
        Draw(gen.neurons, gen.weights);
        if (index == ga.populationSize - 1)
            index = 0;
        else index++;
        textField.text = (index + 1) + " / " + ga.populationSize;
    }

    private void DrawLine(Vector3 neuronInPos, Vector3 neuronOutPos) {

        renderer.positionCount += 2;
        renderer.SetPosition(renderer.positionCount - 2, neuronInPos);
        renderer.SetPosition(renderer.positionCount - 1, neuronOutPos);
    }
}
