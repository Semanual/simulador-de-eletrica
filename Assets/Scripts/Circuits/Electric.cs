using UnityEngine;

public interface IElectric {
    public float resistance { get; set; }
    // Para usar com diodos (que gastam 0,7 V fixo), LEDs (que gastam 2 V fixo) ou fontes (que adicionam valor fixo)
    public float consumesTension { get; set; }
    public float consumesCurrent { get; set; }
    public float receivingTension { get; set; }
    public float receivingCurrent { get; set; }
    public void UpdateValues();
}

public static class IElectricExtensions {
    public static float GetTension(this IElectric electric, float current) {
        if (electric.consumesTension != 0) {
            return electric.consumesTension;
        }

        return -electric.resistance * current;
    }
}