using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class EndpointAttribute : Attribute {
    public Polarity polarity;
    public EndpointAttribute(Polarity polarity) {
        this.polarity = polarity;
    }
}

public abstract class ElectricComponent : MonoBehaviour, IElectric {
    [field: SerializeField] public float resistance { get; set; }
    [field: SerializeField] public float consumesTension { get; set; }
    [field: SerializeField] public float consumesCurrent { get; set; }
    public float receivingTension { get; set; }
    public float receivingCurrent { get; set; }
    [SerializeField] float maxPower;
    bool isShortCircuited = false;
    bool isPowered;
    public virtual void ShortCircuit(bool isShortCircuited) {}
    public abstract Endpoint[] GetPoweredOutputEndpoints();
    public virtual void SetPowered(bool isPowered) {}
    public Circuit circuit = null;

    protected virtual void Awake() {
        IEnumerable<FieldInfo> fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (FieldInfo field in fields) {
            Attribute attribute = field.GetCustomAttribute(typeof(EndpointAttribute));
            if (attribute == null || attribute is not EndpointAttribute endpointAttribute) {
                continue;
            }

            object valueObj = field.GetValue(this);
            if (valueObj is Endpoint endpoint) {
                endpoint.component = this;
                endpoint.polarity = endpointAttribute.polarity;
                continue;
            }

            if (valueObj is IEnumerable<Endpoint> endpoints) {
                foreach (Endpoint subEndpoint in endpoints) {
                    subEndpoint.component = this;
                    subEndpoint.polarity = endpointAttribute.polarity;
                }
                continue;
            }

            Debug.LogError($"Uso incorreto do atributo EndpointAttribute na variável {field.Name} do componente {GetType().Name}; Só pode ser usado em Endpoints ou coleções de Endpoints");
        }
    }

    public virtual void UpdateValues() {
        bool currentIsPowered = receivingTension >= consumesTension && receivingCurrent >= consumesCurrent && circuit != null;
        bool isCurrentAboveLimit = circuit != null && receivingCurrent * receivingTension > maxPower;

        if (isCurrentAboveLimit != isShortCircuited) {
            isShortCircuited = isCurrentAboveLimit;
            ShortCircuit(isCurrentAboveLimit);
        }

        if (currentIsPowered != isPowered) {
            isPowered = currentIsPowered;
            SetPowered(currentIsPowered);
        }
    }
}