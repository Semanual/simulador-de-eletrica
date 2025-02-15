using System.Collections.Generic;
using UnityEngine;
public abstract class ElectricComponent : MonoBehaviour {
    public virtual bool IsGenerator => false;
    public abstract Endpoint[] GetPoweredOutputEndpoints();
    protected void RegisterEndpointsComponent(IEnumerable<Endpoint> endpoints) {
        foreach (Endpoint endpoint in endpoints) {
            endpoint.component = this;
        }
    }
    public virtual void SetPowered(bool powered) {}
}