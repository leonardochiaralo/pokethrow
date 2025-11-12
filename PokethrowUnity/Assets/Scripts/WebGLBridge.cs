using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Ponte de comunicação entre Unity e JavaScript (React)
/// </summary>
public class WebGLBridge : MonoBehaviour
{
    // Importa função JavaScript para enviar mensagens ao React
    [DllImport("__Internal")]
    private static extern void SendMessageToReact(string eventName, string data);

    /// <summary>
    /// Envia um evento para o React com dados opcionais
    /// </summary>
    public static void SendToReact(string eventName, string data = "")
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SendMessageToReact(eventName, data);
#else
        Debug.Log($"[WebGL Bridge] {eventName}: {data}");
#endif
    }

    /// <summary>
    /// Solicita dados de um Pokémon ao React
    /// </summary>
    public static void RequestPokemonData(int pokemonId)
    {
        SendToReact("RequestPokemonData", pokemonId.ToString());
    }

    /// <summary>
    /// Notifica o React sobre uma captura bem-sucedida
    /// </summary>
    public static void NotifyCaptureSuccess(string pokemonData)
    {
        SendToReact("OnCaptureSuccess", pokemonData);
    }

    /// <summary>
    /// Notifica o React sobre uma captura falhada
    /// </summary>
    public static void NotifyCaptureFailed()
    {
        SendToReact("OnCaptureFailed", "");
    }

    /// <summary>
    /// Notifica o React para voltar ao menu
    /// </summary>
    public static void ReturnToMenu()
    {
        SendToReact("ReturnToMenu", "");
    }
}