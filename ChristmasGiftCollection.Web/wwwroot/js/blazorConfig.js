    Blazor.start({
        reconnectionOptions: {maxRetries: 8, retryIntervalMilliseconds: 500 },
    reconnectionHandler: {
        onConnectionDown: () => {window.__wasDisconnected = true; },
      onConnectionUp: () => { if (window.__wasDisconnected) location.reload(); window.__wasDisconnected = false; }
    },
    configureSignalR: function (builder) {
        builder.withServerTimeout(30000).withKeepAliveInterval(15000);
    }
  });
