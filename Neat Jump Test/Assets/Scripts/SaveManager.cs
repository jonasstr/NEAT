using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.IO;
using System;

public class SaveManager : MonoBehaviour {

    private IntWeightDictionary weights;
    private IntNeuronDictionary neurons;

    private string dataPath;
    public static SaveManager instance;

    void Awake() {

        if (instance == null) {
            DontDestroyOnLoad(transform.gameObject);
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
        dataPath = Application.persistentDataPath + "/nn_data.dat";
        print(dataPath);
    }

    public void Save(IntWeightDictionary weights, IntNeuronDictionary neurons) {

        this.weights = weights;
        this.neurons = neurons;

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(dataPath);

        GameData data = new GameData();

        data.weights = weights;
        data.neurons = neurons;

        bf.Serialize(file, data);
        file.Close();
        Debug.Log("Network saved!");
    }

    public bool Load() {

        if (File.Exists(dataPath)) {

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(dataPath, FileMode.Open);

            if (file.Length > 0) {
                GameData data = (GameData)bf.Deserialize(file);
                file.Close();

                weights = data.weights;
                neurons = data.neurons;
            }
            return true;
        }
        return false;
    }  
    
    public IntWeightDictionary GetWeights() {
        return weights;
    }

    public IntNeuronDictionary GetNeurons() {
        return neurons;
    }

    [Serializable]
    class GameData {

        public IntWeightDictionary weights;
        public IntNeuronDictionary neurons;
    }
}