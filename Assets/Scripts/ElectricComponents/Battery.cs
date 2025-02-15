using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class Battery : BaseGenerator {
    [Endpoint(Polarity.POSITIVE)]
    [SerializeField] Endpoint positiveTerminal;
    [Endpoint(Polarity.NEGATIVE)]
    [SerializeField] Endpoint negativeTerminal;

    public override Endpoint[] GetPoweredOutputEndpoints() {
        return new Endpoint[] { positiveTerminal };
    }

    public override void Refresh() {
        bool isPowered = false;
        HashSet<Endpoint> usedEndpoints = new();
        Queue<Endpoint> endpointsToVisit = new();

        if (positiveTerminal != null) {
            endpointsToVisit.Enqueue(positiveTerminal);
        }

        while (endpointsToVisit.Count > 0) {
            Endpoint endpoint = endpointsToVisit.Dequeue();
            Endpoint[] connectedEndpoints = endpoint.component.GetPoweredOutputEndpoints();
            foreach (Endpoint connected in connectedEndpoints) {
                if (usedEndpoints.Contains(connected)) {
                    return;
                }

                endpointsToVisit.Enqueue(connected);
                usedEndpoints.Add(connected);
            }

            if (endpoint == negativeTerminal) {
                isPowered = true;
                break;
            }
        }

        foreach (Endpoint endpoint in usedEndpoints) {
            endpoint.component.SetPowered(isPowered);
        }
    }
}