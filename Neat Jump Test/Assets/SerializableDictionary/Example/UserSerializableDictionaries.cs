using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Runtime.Serialization;

[Serializable]
public class IntNeuronDictionary : SerializableDictionary<int, Neuron> {

    public IntNeuronDictionary() : base() { }
    protected IntNeuronDictionary(SerializationInfo info, StreamingContext context) : base(info,context){ }
}

[Serializable]
public class IntWeightDictionary : SerializableDictionary<int, Weight> {

    public IntWeightDictionary() : base() { }
    protected IntWeightDictionary(SerializationInfo info, StreamingContext context) : base(info,context){ }
}

// ---------------------------

[Serializable]
public class StringStringDictionary : SerializableDictionary<string, string> {}

[Serializable]
public class ObjectColorDictionary : SerializableDictionary<UnityEngine.Object, Color> {}

[Serializable]
public class ColorArrayStorage : SerializableDictionary.Storage<Color[]> {}

[Serializable]
public class StringColorArrayDictionary : SerializableDictionary<string, Color[], ColorArrayStorage> {}

[Serializable]
public class MyClass
{
    public int i;
    public string str;
}

[Serializable]
public class QuaternionMyClassDictionary : SerializableDictionary<Quaternion, MyClass> {}