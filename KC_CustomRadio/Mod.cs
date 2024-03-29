using System.Threading;
using MelonLoader;

namespace Quriz.CustomRadio;

public class Mod : MelonMod
{
    public static readonly CancellationTokenSource CancellationTokenSource = new();
    
    public override void OnInitializeMelon()
    {
        Assets.LoadAssets();
        SongsDownloader.Init();
    }

    public override void OnApplicationQuit()
    {
        CancellationTokenSource.Cancel();
        SongsDownloader.Cleanup();
    }
}