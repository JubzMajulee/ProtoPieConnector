using UnityEngine;
using Vuplex.WebView;

public class TransparentPieInVuplex : MonoBehaviour
{
    // A public slot to drag our CanvasWebViewPrefab into.
    public CanvasWebViewPrefab webview;

    // async/await allows us to wait for the web view to be ready.
    async void Start()
    {
        // 1. Wait until the web view has finished initializing.
        await webview.WaitUntilInitialized();

        // 2. Tell the Vuplex browser to not render its default white background.
        // This allows transparency to show through.
        webview.WebView.SetDefaultBackgroundEnabled(false);

        // 3. Load the URL from your ProtoPie Connect.
        // This is the key to making the Pie's background transparent!
        webview.WebView.LoadUrl("http://<your-ip>:9981/pie?pieid=2&bg=transparent");
    }
}