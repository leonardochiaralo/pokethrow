using UnityEngine;

/// <summary>
/// Sistema de cálculo de captura de Pokémon
/// Baseado em força do arremesso e precisão do acerto
/// </summary>
public static class CaptureSystem
{
    // Configurações de probabilidade
    private const float BASE_CAPTURE_RATE = 0.50f;      // Taxa base: 50%
    private const float MAX_FORCE_BONUS = 0.30f;        // Bônus máximo por força: +30%
    private const float MAX_ACCURACY_BONUS = 0.20f;     // Bônus máximo por precisão: +20%
    
    // Limiares
    private const float MIN_FORCE_THRESHOLD = 10f;      // Força mínima para começar a somar bônus
    private const float MAX_FORCE_THRESHOLD = 50f;      // Força máxima considerada
    private const float MIN_ACCURACY_THRESHOLD = 0.3f;  // Precisão mínima para começar a somar bônus

    /// <summary>
    /// Calcula se a captura foi bem-sucedida
    /// </summary>
    /// <param name="throwForce">Força do arremesso (0-50+)</param>
    /// <param name="accuracy">Precisão do acerto (0.0-1.0)</param>
    /// <returns>True se capturou, False se falhou</returns>
    public static bool CalculateCapture(float throwForce, float accuracy)
    {
        // Calcula os bônus
        float forceBonus = CalculateForceBonus(throwForce);
        float accuracyBonus = CalculateAccuracyBonus(accuracy);
        
        // Taxa final de captura
        float captureRate = BASE_CAPTURE_RATE + forceBonus + accuracyBonus;
        captureRate = Mathf.Clamp01(captureRate); // Garante que fica entre 0 e 1
        
        // Sorteia um número aleatório
        float roll = Random.Range(0f, 1f);
        
        bool success = roll <= captureRate;
        
        // Log detalhado
        Debug.Log($"📊 CÁLCULO DE CAPTURA:");
        Debug.Log($"   Base: {BASE_CAPTURE_RATE:P0}");
        Debug.Log($"   Bônus Força: +{forceBonus:P0} (força: {throwForce:F1})");
        Debug.Log($"   Bônus Precisão: +{accuracyBonus:P0} (precisão: {accuracy:P0})");
        Debug.Log($"   Taxa Final: {captureRate:P0}");
        Debug.Log($"   Sorteio: {roll:F3} {(success ? "≤" : ">")} {captureRate:F3}");
        Debug.Log($"   Resultado: {(success ? "✅ CAPTURADO!" : "❌ FALHOU!")}");
        
        return success;
    }

    /// <summary>
    /// Calcula o bônus baseado na força do arremesso
    /// Arremessos mais fortes = maior chance
    /// </summary>
    private static float CalculateForceBonus(float force)
    {
        if (force < MIN_FORCE_THRESHOLD)
            return 0f;
        
        // Normaliza a força entre os limiares
        float normalizedForce = Mathf.InverseLerp(MIN_FORCE_THRESHOLD, MAX_FORCE_THRESHOLD, force);
        
        // Aplica uma curva (quadrática) para tornar mais desafiador
        normalizedForce = Mathf.Pow(normalizedForce, 1.5f);
        
        return normalizedForce * MAX_FORCE_BONUS;
    }

    /// <summary>
    /// Calcula o bônus baseado na precisão do acerto
    /// Acertos no centro = maior chance
    /// </summary>
    private static float CalculateAccuracyBonus(float accuracy)
    {
        if (accuracy < MIN_ACCURACY_THRESHOLD)
            return 0f;
        
        // Normaliza a precisão
        float normalizedAccuracy = Mathf.InverseLerp(MIN_ACCURACY_THRESHOLD, 1f, accuracy);
        
        // Aplica curva exponencial (acertar o centro é muito melhor)
        normalizedAccuracy = Mathf.Pow(normalizedAccuracy, 2f);
        
        return normalizedAccuracy * MAX_ACCURACY_BONUS;
    }

    /// <summary>
    /// Retorna uma descrição do resultado da captura
    /// </summary>
    public static string GetCaptureDescription(float throwForce, float accuracy, bool success)
    {
        if (!success)
        {
            if (throwForce < MIN_FORCE_THRESHOLD)
                return "Arremesso muito fraco!";
            if (accuracy < MIN_ACCURACY_THRESHOLD)
                return "Muito longe do alvo!";
            return "Quase! Tente novamente!";
        }
        
        // Mensagens de sucesso baseadas na performance
        float totalBonus = CalculateForceBonus(throwForce) + CalculateAccuracyBonus(accuracy);
        
        if (totalBonus >= 0.45f)
            return "CAPTURA PERFEITA! 🌟";
        else if (totalBonus >= 0.30f)
            return "Excelente captura! ⭐";
        else if (totalBonus >= 0.15f)
            return "Boa captura! 👍";
        else
            return "Capturado! 🎉";
    }

    /// <summary>
    /// Simula múltiplas tentativas para testes (Debug)
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void SimulateCaptures(int attempts = 100)
    {
        int successes = 0;
        
        Debug.Log($"🧪 SIMULANDO {attempts} CAPTURAS:");
        
        for (int i = 0; i < attempts; i++)
        {
            float randomForce = Random.Range(10f, 50f);
            float randomAccuracy = Random.Range(0f, 1f);
            
            if (CalculateCapture(randomForce, randomAccuracy))
                successes++;
        }
        
        float successRate = (float)successes / attempts;
        Debug.Log($"📈 Taxa de sucesso: {successRate:P1} ({successes}/{attempts})");
    }
}