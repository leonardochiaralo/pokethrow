using UnityEngine;

/// <summary>
/// Sistema de cálculo de captura de Pokémon.
/// A chance de captura depende da força do arremesso e da precisão do acerto.
/// </summary>
public static class CaptureSystem
{
    private const float BaseCaptureRate = 0.50f;
    private const float MaxForceBonus = 0.30f;
    private const float MaxAccuracyBonus = 0.20f;

    private const float MinForceThreshold = 10f;
    private const float MaxForceThreshold = 50f;
    private const float MinAccuracyThreshold = 0.3f;

    public static bool CalculateCapture(float throwForce, float accuracy)
    {
        float forceBonus = CalculateForceBonus(throwForce);
        float accuracyBonus = CalculateAccuracyBonus(accuracy);

        float captureRate = Mathf.Clamp01(BaseCaptureRate + forceBonus + accuracyBonus);
        float roll = Random.Range(0f, 1f);
        bool success = roll <= captureRate;

        LogCaptureDetails(throwForce, accuracy, forceBonus, accuracyBonus, captureRate, roll, success);

        return success;
    }

    public static string GetCaptureDescription(float throwForce, float accuracy, bool success)
    {
        if (!success)
        {
            if (throwForce < MinForceThreshold) return "Arremesso muito fraco!";
            if (accuracy < MinAccuracyThreshold) return "Muito longe do alvo!";
            return "Quase! Tente novamente!";
        }

        float totalBonus = CalculateForceBonus(throwForce) + CalculateAccuracyBonus(accuracy);

        return totalBonus switch
        {
            >= 0.45f => "CAPTURA PERFEITA! 🌟",
            >= 0.30f => "Excelente captura! ⭐",
            >= 0.15f => "Boa captura! 👍",
            _ => "Capturado! 🎉"
        };
    }

#if UNITY_EDITOR
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void SimulateCaptures(int attempts = 100)
    {
        int successes = 0;
        Debug.Log($"🧪 Simulando {attempts} capturas...");

        for (int i = 0; i < attempts; i++)
        {
            float randomForce = Random.Range(10f, 50f);
            float randomAccuracy = Random.Range(0f, 1f);
            if (CalculateCapture(randomForce, randomAccuracy)) successes++;
        }

        float successRate = (float)successes / attempts;
        Debug.Log($"📈 Taxa de sucesso: {successRate:P1} ({successes}/{attempts})");
    }
#endif

    private static float CalculateForceBonus(float force)
    {
        if (force < MinForceThreshold) return 0f;

        float normalized = Mathf.InverseLerp(MinForceThreshold, MaxForceThreshold, force);
        normalized = Mathf.Pow(normalized, 1.5f);

        return normalized * MaxForceBonus;
    }

    private static float CalculateAccuracyBonus(float accuracy)
    {
        if (accuracy < MinAccuracyThreshold) return 0f;

        float normalized = Mathf.InverseLerp(MinAccuracyThreshold, 1f, accuracy);
        normalized = Mathf.Pow(normalized, 2f);

        return normalized * MaxAccuracyBonus;
    }

#if UNITY_EDITOR
    private static void LogCaptureDetails(float force, float accuracy, float forceBonus, float accuracyBonus, float rate, float roll, bool success)
    {
        Debug.Log("📊 CÁLCULO DE CAPTURA:");
        Debug.Log($"   Base: {BaseCaptureRate:P0}");
        Debug.Log($"   Bônus Força: +{forceBonus:P0} (força: {force:F1})");
        Debug.Log($"   Bônus Precisão: +{accuracyBonus:P0} (precisão: {accuracy:P0})");
        Debug.Log($"   Taxa Final: {rate:P0}");
        Debug.Log($"   Sorteio: {roll:F3} {(success ? "≤" : ">")} {rate:F3}");
        Debug.Log($"   Resultado: {(success ? "✅ CAPTURADO!" : "❌ FALHOU!")}");
    }
#endif
}
