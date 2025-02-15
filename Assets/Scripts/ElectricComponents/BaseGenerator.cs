using UnityEngine;
public abstract class BaseGenerator : ElectricComponent {
    public override bool IsGenerator => true;
    public abstract void Refresh();
    protected virtual void Awake() {
        GeneratorManager.Singleton.Register(this);
    }
}