using System.Collections.Generic;
using UnityEngine;
public abstract class BaseGenerator : ElectricComponent {
    readonly protected HashSet<ElectricComponent> powering = new();
    [SerializeField] bool isMainGenerator = true;
    public float electromotiveForce = 10;
    public abstract void Refresh();
    protected virtual void Start() {
        if (isMainGenerator) {
            GeneratorManager.Singleton.Register(this);
        }
    }
}