using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// Loads the correct Menu scene based on the id of the currently loaded <see cref="XRSessionSubsystem"/>.
    /// </summary>
    [DefaultExecutionOrder(-9999)]
    public class MenuLoader : MonoBehaviour
    {
        const string k_DefaultMenuScene = "Menu";
        const string k_MetaMenuScene = "MetaMenu";

        void Awake()
        {
            LoadMenuScene();
        }

        public static string GetMenuSceneName()
        {
            var loader = UnityEngine.XR.ARFoundation.LoaderUtility.GetActiveLoader();
            var sessionSubsystem = loader != null ? loader.GetLoadedSubsystem<XRSessionSubsystem>() : null;

            if (sessionSubsystem == null)
            {
                // Could be null if user does not have "Initialize XR on Startup" enabled in XR Plug-in Management
                return k_DefaultMenuScene;
            }

            // We switch on Session Descriptor id because we can't guarantee with current preprocessor directives whether
            // a provider package (and its types) will be present. For example, UNITY_ANDROID could signal that either
            // ARCore or OpenXR loader is present. Because we don't know for sure, we are unable to switch on the loader
            // type without introducing a build-time error in case that package was stripped.
            switch (sessionSubsystem.subsystemDescriptor.id)
            {
                case "Meta-Session":
                    return k_MetaMenuScene;
                case "ARKit-Session":
                case "ARCore-Session":
                case "XRSimulation-Session":
                default:
                    // Default case includes HoloLens and other third-party providers
                    return k_DefaultMenuScene;
            }
        }

        public static bool IsHmdDevice()
        {
            return true;
            //return GetMenuSceneName() == k_MetaMenuScene;
        }

        public static void LoadMenuScene()
        {
            var sceneName = GetMenuSceneName();
            if (Application.CanStreamedLevelBeLoaded(sceneName))
                SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }

