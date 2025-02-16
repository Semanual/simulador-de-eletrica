using System.Collections.Generic;
using UnityEngine;
public class Resistor : ElectricComponent {
    [SerializeField] float tolerance;
    [Header("Terminais")]
    [Endpoint(Polarity.NONE)]
    [SerializeField] Endpoint[] terminals;
    [Header("Animator para liga/desliga")]
    [SerializeField] Animator animator;
    
    [Header("Linhas do resistor (não definir se não houver)")]
    [SerializeField] MeshRenderer stripeFirstDigit;
    [SerializeField] MeshRenderer stripeSecondDigit;
    [SerializeField] MeshRenderer stripeThirdDigit;
    [SerializeField] MeshRenderer stripeMultiplier;
    [SerializeField] MeshRenderer stripeTolerance;

    [Header("Materiais das linhas (Obrigatório se houver linhas)")]
    [SerializeField] Material black;
    [SerializeField] Material brown;
    [SerializeField] Material red;
    [SerializeField] Material orange;
    [SerializeField] Material yellow;
    [SerializeField] Material green;
    [SerializeField] Material blue;
    [SerializeField] Material violet;
    [SerializeField] Material gray;
    [SerializeField] Material white;

    [SerializeField] Material gold;
    [SerializeField] Material silver;

    Dictionary<int, Material> stripeDigitConverter;
    Dictionary<float, Material> stripeMultiplierConverter;
    Dictionary<float, Material> stripeToleranceConverter;

    protected override void Awake() {
        base.Awake();

        if (stripeFirstDigit == null && stripeSecondDigit == null && stripeThirdDigit == null && stripeMultiplier == null && stripeTolerance == null) {
            return;
        }

        stripeDigitConverter = new Dictionary<int, Material>() {
            { 0, black },
            { 1, brown },
            { 2, red },
            { 3, orange },
            { 4, yellow },
            { 5, green },
            { 6, blue },
            { 7, violet },
            { 8, gray },
            { 9, white }
        };

        stripeMultiplierConverter = new Dictionary<float, Material>() {
            { 0.01f, silver },
            { 0.1f, gold },
            { 1f, black },
            { 10f, brown },
            { 100f, red },
            { 1000f, orange },
            { 10000f, yellow },
            { 100000f, green },
            { 1000000f, blue },
        };

        stripeToleranceConverter = new Dictionary<float, Material>() {
            { 0.01f, brown },
            { 0.02f, red },
            { 0.05f, gold },
            { 0.1f, silver },
        };

        SetResistance(resistance);
    }

    void SetResistance(float resistance) {
        this.resistance = resistance;

        int multiplierMagnitude = Mathf.FloorToInt(Mathf.Log10(resistance));
        int firstDigit = Mathf.FloorToInt(resistance / Mathf.Pow(10, multiplierMagnitude));
        int secondDigit = Mathf.FloorToInt(resistance / Mathf.Pow(10, multiplierMagnitude - 1)) % 10;
        int thirdDigit = Mathf.FloorToInt(resistance / Mathf.Pow(10, multiplierMagnitude - 2)) % 10;

        // Cada fita aumenta uma ordem de grandeza na resistência, pois aumenta a quantidade de dígitos do número multiplicado
        // Portanto, podemos reduzir uma ordem de grandeza do multiplicador para cada fita
        multiplierMagnitude++;

        if (stripeFirstDigit) {
            multiplierMagnitude--;
            stripeFirstDigit.SetMaterials(new() { stripeDigitConverter.GetValueOrDefault(firstDigit) });
        }

        if (stripeSecondDigit) {
            multiplierMagnitude--;
            stripeSecondDigit.SetMaterials(new() { stripeDigitConverter.GetValueOrDefault(secondDigit) });
        }

        if (stripeThirdDigit) {
            multiplierMagnitude--;
            stripeThirdDigit.SetMaterials(new() { stripeDigitConverter.GetValueOrDefault(thirdDigit) });
        }

        // Não há cor definida para valores abaixo de x0.01 ou acima de x1000000
        multiplierMagnitude = Mathf.Clamp(multiplierMagnitude, -2, 6);
        if (stripeMultiplier) {
            stripeMultiplier.SetMaterials(new() { stripeMultiplierConverter.GetValueOrDefault(Mathf.Pow(10, multiplierMagnitude)) });
        }

        float tolerance = this.tolerance;
        Debug.Log(tolerance);
        if (tolerance <= 0.01f) {
            tolerance = 0.01f;
        }
        else if (tolerance <= 0.02f) {
            tolerance = 0.02f;
        }
        else if (tolerance <= 0.05f) {
            tolerance = 0.05f;
        }
        else {
            tolerance = 0.1f;
        }

        if (stripeTolerance) {
            stripeTolerance.SetMaterials(new() { stripeToleranceConverter.GetValueOrDefault(tolerance) });
        }
    }

    public override Endpoint[] GetPoweredOutputEndpoints() {
        return terminals;
    }

    public override void SetPowered(bool isPowered) {
        if (!animator) {
            return;
        }
        animator.SetBool("isPowered", isPowered);
    }
}