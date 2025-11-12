mergeInto(LibraryManager.library, {
  SendMessageToReact: function (eventNamePtr, dataPtr) {
    var eventName = UTF8ToString(eventNamePtr);
    var data = UTF8ToString(dataPtr);

    console.log("[Unity → React]", eventName, data);

    // Envia mensagem para o React através do window
    if (window.unityToReact) {
      window.unityToReact(eventName, data);
    } else {
      console.warn(
        "⚠️ unityToReact não está definido no window! Verifique o React."
      );
    }
  },
});
