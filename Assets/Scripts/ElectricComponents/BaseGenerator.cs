using System.Collections.Generic;
using UnityEngine;
public abstract class BaseGenerator : ElectricComponent {
    readonly protected HashSet<ElectricComponent> powering = new();
    public override bool IsGenerator => true;
    public abstract void Refresh();
    public abstract void ShortCircuit(bool isShortCircuited);
    protected virtual void Start() {
        GeneratorManager.Singleton.Register(this);
    }
}