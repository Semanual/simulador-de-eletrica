using System;
using System.Collections.Generic;
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

public abstract class ElectricComponent : MonoBehaviour {
    public virtual bool IsGenerator => false;
    protected virtual void Awake() {
        IEnumerable<FieldInfo> fields = GetType().GetFields();
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
            }

            Debug.LogError($"Uso incorreto do atributo EndpointAttribute na variável {field.Name} do componente {GetType().Name}; Só pode ser usado em Endpoints ou coleções de Endpoints");
        }
    }

    public abstract Endpoint[] GetPoweredOutputEndpoints();
    public virtual void SetPowered(bool powered) {}
}