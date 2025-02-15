using UnityEngine;
public abstract class BaseGenerator : ElectricComponent {
    public override bool IsGenerator => true;
    public abstract void Refresh();
    protected override void Awake() {
        base.Awake();
        GeneratorManager.Singleton.Register(this);
    }
}