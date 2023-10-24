using UnityEngine;
using UnityEngine.SceneManagement;
namespace HeroicArcade.CC.Core
{
    public sealed class _Main : MonoBehaviour
    {
        private static GameObject _mainResources;

        // This function runs before a scene gets loaded.
        // NOTE1 Sometimes we want to drag Resources::_Main into the Hierarchy before pressing Play,
        //       for instance because we want to access the child objects parented under it.
        //       The Awake() function will take care of removing the original Resources::_Main
        //       (Unity may complain that "There can be only one active Event System",
        //       but it is innocuous.)
        // NOTE2 We remove (Clone) from the name of the instantiated _Main(Clone) so it is named no
        //       differently than the _Main we may manually drag into the Hierarchy.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static private void LoadMain()
        {
            _mainResources = Resources.Load("_Main") as GameObject;
            if (_mainResources == null)
            {
                Debug.LogError("Could not find Resources::_Main");
                return;
            }
            if (_mainResources.activeSelf == false)
            {
                Debug.LogWarning("Resources::_Main is inactive");
            }
            _mainResources = Object.Instantiate(_mainResources);
            _mainResources.name = "_Main"; // Remove trailing (Clone) from _Main(Clone)'s name.
            GameObject.DontDestroyOnLoad(_mainResources);
        }

        // You can append child GameObject services to _Main.
        //   Example: EventSystem, Main Camera, Directional Light, Virtual Cameras, etc.
        // You can also add "Service" components to the _Main prefab.
        //   Examples are: CursorManager, TransformsSyncher, InputController, etc.
        // Finally, you can add API helpers directly in the body of this class.
        //   Example: 'static private readonly LevelManager levelManager = new LevelManager()'
        //            would be used to implement 'public static void LoadScene(int buildIndex)'
        //            that can be called by simply typing _Main.LoadScene(1) from anywhere.

        private static int numAwakeExecutions = 0;
        private void Awake()
        {
            numAwakeExecutions++;
            //Debug.Log("Number of Awake() executions " + numAwakeExecutions);
            if (numAwakeExecutions == 2)
            {
                // Keep _Main and destroy the one coming from Resources,
                // otherwise keep the one from Resources (the one whose
                // name has been changed to drop the (Clone) at the end,
                // so that, no matter what, the object in the Hierarchy
                // is always named _Main.)
                Debug.Log("Eliminating instance of cloned Resources::_Main");
                Destroy(_mainResources);
            }
        }

        /*
         * APIS accessible by simply writing _Main.FunctionName(arguments);
         */

        public static void LoadScene(int buildIndex)
        {
            SceneManager.LoadScene(buildIndex);
        }
    }
}

