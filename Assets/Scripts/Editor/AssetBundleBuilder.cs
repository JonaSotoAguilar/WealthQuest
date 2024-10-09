#if UNITY_EDITOR
using UnityEditor;
using UnityEngine; // Para acceder a Application.persistentDataPath
using System.IO;  // Para Directory y Path

public class AssetBundleBuilder
{
    [MenuItem("Assets/Build Asset Bundle")]
    static void BuildAllAssetBundles()
    {
        // Define la ruta persistente del juego
        string assetBundleDirectory = Path.Combine(Application.persistentDataPath, "AssetBundles");

        // Crea el directorio si no existe
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }

        // Construye todos los AssetBundles en la carpeta especificada
        BuildPipeline.BuildAssetBundles(assetBundleDirectory,
            BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);

        Debug.Log($"AssetBundles construidos en: {assetBundleDirectory}");
    }
}
#endif
