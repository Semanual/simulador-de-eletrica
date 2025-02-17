using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum AssociationType {
    PARALLEL,
    SERIES
};
public class Circuit : IElectric, IEnumerable<IElectric> {
    public Circuit(AssociationType associationType, List<IElectric> electrics) {
        this.associationType = associationType;
        this.electrics = electrics;
    }

    public float receivingTension { get; set; }
    public float receivingCurrent { get; set; }
    public float electromotiveForce {
        get => electrics.Aggregate(0f, (currentEF, electric) => {
            if (electric is Circuit circuit) {
                return currentEF + circuit.electromotiveForce;
            } else if (electric is BaseGenerator generator) {
                return currentEF + generator.electromotiveForce;
            }
            return currentEF;
        });
    }
    public float resistance { 
        get {
            // Se não houver componentes, a resistência é 0, evitamos assim divisão por 0
            if (electrics.Count == 0) {
                return 0f;
            }

            if (associationType == AssociationType.SERIES) {
                // Associação em série: x1 + x2 + x3 + ... + xn
                return electrics.Aggregate(0f, (currentResistance, electric) => currentResistance + electric.resistance);
            } else {
                // Associação em paralelo: 1 / (1/x1 + 1/x2 + 1/x3 + ... + 1/xn)
                float inverseResistance = 0f;
                foreach (IElectric electric in electrics) {
                    // Se houver um caminho livre de resistência, a corrente vai passar apenas por lá e a resistência é 0
                    if (electric.resistance == 0) {
                        return 0;
                    }
                    inverseResistance += 1 / electric.resistance;
                }

                return 1 / inverseResistance;
            }
        }
        // Não há como definir a resistência de um circuito, pois ela é calculada a partir dos componentes
        set {}
    }
    public float consumesTension { get => 0; set {} }
    public float consumesCurrent { get => 0; set {} }

    public AssociationType associationType = AssociationType.SERIES;
    public List<IElectric> electrics = new();

    public void UpdateValues() {
        float fullResistance = resistance;
        float remainingElectromotiveForce = receivingTension + electromotiveForce;
        float remainingCurrent = receivingCurrent;

        foreach (IElectric electric in electrics) {
            if (associationType == AssociationType.PARALLEL) {
                electric.receivingTension = remainingElectromotiveForce;
                // Com resistência elétrica = 0, a corrente vai passar inteira por esta série
                electric.receivingCurrent = electric.resistance == 0 ? receivingCurrent : receivingTension / electric.resistance;
                // Em casos que não há resistência elétrica, a corrente é infinita
                // (Não existe circuito sem resistência elétrica na vida real, mas isso é uma simulação)
                if (!float.IsPositiveInfinity(electric.receivingCurrent)) {
                    remainingCurrent -= electric.receivingCurrent;
                }
            } else {
                electric.receivingTension = electric.resistance * remainingCurrent;
                electric.receivingCurrent = remainingCurrent;
                
                remainingElectromotiveForce -= electric.receivingTension;
            }

            if (electric is ElectricComponent component) {
                component.circuit = this;
            }
            
            electric.UpdateValues();
        }

        if (associationType == AssociationType.SERIES && remainingElectromotiveForce > 1e-5) {
            Debug.LogError(
                $"A tensão não foi completamente consumida pelo circuito em série, ou foi usado mais do que havia. O valor calculado está incorreto." +
                $"\nTensão inicial: {receivingTension}" +
                $"\nTensão remanescente: {remainingElectromotiveForce}" +
                $"\nResistência total do circuito: {resistance}" +
                $"\nComponentes: " + electrics.Select(electric => electric is ElectricComponent component ? $"{component.name}: {electric.receivingTension} V {electric.receivingCurrent} A" : $"Subcircuit: {electric.receivingTension} V {electric.receivingCurrent} A")
            );
        } else if (associationType == AssociationType.PARALLEL && remainingCurrent > 1e-5 && !float.IsPositiveInfinity(remainingCurrent)) {
            Debug.LogError(
                $"A corrente não foi completamente consumida pelo circuito em paralelo, ou foi usado mais do que havia. O valor calculado está incorreto." +
                $"\nCorrente inicial: {receivingCurrent}" +
                $"\nCorrente remanescente: {remainingCurrent}" +
                $"\nResistência total do circuito: {resistance}" +
                $"\nComponentes: " + electrics.Select(electric => electric is ElectricComponent component ? $"{component.name}: {electric.receivingTension} V {electric.receivingCurrent} A" : $"Subcircuit: {electric.receivingTension} V {electric.receivingCurrent} A")
            );
        }
    }

    public void Dispose(bool updateChildrenStates) {
        foreach (IElectric electric in electrics) {
            electric.receivingCurrent = 0;
            electric.receivingTension = 0;
            if (electric is ElectricComponent component) {
                component.circuit = null;
                if (updateChildrenStates) {
                    component.UpdateValues();
                }
            } else if (electric is Circuit subcircuit) {
                subcircuit.Dispose(updateChildrenStates);
            }
        }
    }

    public IEnumerator<IElectric> GetEnumerator() {
        return electrics.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return electrics.GetEnumerator();
    }
}