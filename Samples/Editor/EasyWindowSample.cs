using UnityEditor;

public class EasyWindowSample : EasyWindow
{
    new public const string DefaultWindowName = "Easy Window Sample";

    [MenuItem("Window/Easy Window Sample")]
    public static void NewWindow()
    {
        MakeWindow<EasyWindowSample>();
    }
}
