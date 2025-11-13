using System.Runtime.InteropServices;
using UnityEngine;

public class WebGLBridge : MonoBehaviour
{
    #if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void SendMessageToReact(string eventName, string data);
    #endif

    public static void SendToReact(string eventName, string data = "")
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            SendMessageToReact(eventName, data);
        #else
            LogMessage(eventName, data);
        #endif
    }

    public static void RequestPokemonData(int pokemonId) =>
        SendToReact("RequestPokemonData", pokemonId.ToString());

    public static void NotifyCaptureSuccess(string pokemonData) =>
        SendToReact("OnCaptureSuccess", pokemonData);

    public static void NotifyCaptureFailed() =>
        SendToReact("OnCaptureFailed");

    public static void ReturnToMenu() =>
        SendToReact("ReturnToMenu");

    #if UNITY_EDITOR
        private static void LogMessage(string eventName, string data)
        {
            string formattedData = string.IsNullOrEmpty(data) ? "(sem dados)" : data;
            Debug.Log($"🧩 [WebGLBridge] Enviado evento: {eventName} → {formattedData}");
        }
    #endif
}
