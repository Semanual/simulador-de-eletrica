using UnityEngine;
using UnityEngine.EventSystems;
public class Switch : ElectricComponent, IPointerDownHandler {
    [SerializeField] Endpoint positiveTerminal;
    [SerializeField] Endpoint negativeTerminal;
    [SerializeField] Animator animator;
    [SerializeField] bool isOn;

    public override Endpoint[] GetPoweredOutputEndpoints() {
        if (!isOn) {
            return new Endpoint[0];
        }

        return new Endpoint[] { negativeTerminal };
    }

    public void OnPointerDown(PointerEventData eventData) {
        isOn = !isOn;
        animator.SetBool("isOn", isOn);
    }
}