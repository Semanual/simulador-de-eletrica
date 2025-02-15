using System.Collections.Generic;
using UnityEngine;

public class GeneratorManager : MonoBehaviour {
    public static GeneratorManager Singleton;
    readonly List<BaseGenerator> generators = new();
    void Awake() {
        if (Singleton != null) {
            Debug.LogError("Há mais de um GeneratorManager nesta cena.");
            Destroy(gameObject);
            return;
        }

        Singleton = this;
    }

    void Start() {
        RefreshConductors();
    }

    public void Register(BaseGenerator generator) {
        if (!generator.IsGenerator) {
            Debug.LogError("Tentou registrar um não gerador como gerador: " + generator.name);
            return;
        }

        generators.Add(generator);
    }

    public void RefreshConductors() {
        foreach (BaseGenerator generator in generators) {
            generator.Refresh();
        }
    }
}