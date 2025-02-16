using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Endpoint : Conductor, IPointerDownHandler, IPointerUpHandler {
    [HideInInspector] public ElectricComponent component;
    [SerializeField] bool canPullWire = true;
    [SerializeField] Vector3 wirePositionLocalOffset;
    [SerializeField] InputActionReference pointInput;
    Wire wire = null;
    public Wire Wire {
        get => wire;
        set {
            if (wire == value) {
                return;
            }
            
            if (wire != null) {
                Wire lastWire = wire;
                if (lastWire.StartConductor != null && lastWire.StartConductor is Endpoint startConductor) {
                    startConductor.wire = null;
                } else if (lastWire.StartConductor != null && lastWire.StartConductor is Endpoint endConductor) {
                    endConductor.wire = null;
                }
                Destroy(lastWire.gameObject);
            }
            wire = value;
        }
    }
    Wire wirePrefab;
    Plane wirePlane;
    bool isMovingWire = false;
    void Awake() {
        wirePrefab = Resources.Load<Wire>(Wire.PREFAB_PATH);
        wirePlane = new(transform.up, transform.position.y);
    }

    void Update() {
        if (isMovingWire && Wire != null) {
            Vector3 worldMousePosition = GetWorldMousePosition();
            Wire.EndPosition = worldMousePosition;
        }
    }

    public override Conductor[] GetConnectedConductors(Conductor from = null) {
        if (component == null) {
            Debug.LogError("Endpoint não registrado por um componente elétrico: " + name);
            return null;
        }

        Conductor[] positive = component.GetPoweredOutputEndpoints().Where(endpoint => endpoint != this).ToArray();
        Conductor[] negative = Wire != null && Wire != from ? new Conductor[] { Wire } : new Conductor[0];

        return polarity switch {
            Polarity.POSITIVE => positive,
            Polarity.NEGATIVE => negative,
            _ => positive.Concat(negative).ToArray(),
        };
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (!canPullWire) {
            return;
        }

        Vector3 worldMousePosition = GetWorldMousePosition();

        if (Wire != null) {
            Destroy(Wire.gameObject);
        }

        Wire = Instantiate(wirePrefab);
        Wire.StartPosition = GetConnectionPoint();
        Wire.EndPosition = worldMousePosition;
        isMovingWire = true;
        Wire.StartConductor = this;
    }

    Vector3 GetWorldMousePosition() {
        return GetWorldMousePosition(out _);
    }

    Vector3 GetWorldMousePosition(out Ray ray) {
        ray = Camera.main.ScreenPointToRay(pointInput.action.ReadValue<Vector2>());

        if (!wirePlane.Raycast(ray, out float distance)) {
            Debug.LogError("Plano é paralelo ao raio, ou está atrás da câmera" + name);
            return Vector3.zero;
        }

        return ray.GetPoint(distance);
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (!isMovingWire) {
            return;
        }

        GetWorldMousePosition(out Ray ray);

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity)) {
            Wire = null;
            return;
        }

        if (hit.collider == null) {
            Wire = null;
            return;
        }

        if (!hit.collider.gameObject.TryGetComponent(out Endpoint endpoint)) {
            Wire = null;
            return;
        }

        if (endpoint == this) {
            Wire = null;
            return;
        }

        endpoint.Wire = Wire;
        Wire.EndConductor = endpoint;
        Wire.EndPosition = endpoint.GetConnectionPoint();
        isMovingWire = false;
    }
    
    public Vector3 GetConnectionPoint() {
        return transform.TransformPoint(wirePositionLocalOffset);
    }
}