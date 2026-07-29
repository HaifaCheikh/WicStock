// wwwroot/js/turnstile-interop.js
// Interop JS <-> Blazor pour Cloudflare Turnstile

window.turnstileInterop = {
    widgetId: null,
    dotnetRef: null,

    render: function (containerId, siteKey, dotnetRef) {
        window.turnstileInterop.dotnetRef = dotnetRef;

        const tryRender = () => {
            if (window.turnstile) {
                window.turnstileInterop.widgetId = window.turnstile.render("#" + containerId, {
                    sitekey: siteKey,
                    theme: "light",
                    callback: function (token) {
                        dotnetRef.invokeMethodAsync("OnCaptchaSuccess", token);
                    },
                    "error-callback": function () {
                        dotnetRef.invokeMethodAsync("OnCaptchaError");
                    },
                    "expired-callback": function () {
                        dotnetRef.invokeMethodAsync("OnCaptchaExpired");
                    }
                });
            } else {
                setTimeout(tryRender, 100);
            }
        };
        tryRender();
    },

    reset: function () {
        if (window.turnstile && window.turnstileInterop.widgetId !== null) {
            window.turnstile.reset(window.turnstileInterop.widgetId);
        }
    }
};
