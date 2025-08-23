// Assets/Scripts/Fondation/Save/AppBootstrapper.cs
using UnityEngine;
using UnityEngine.SceneManagement;

using PathOfFaith.Fondation.Core; // <- OK de garder, mais on ne l'utilise plus ici
using PathOfFaith.Save;

public class AppBootstrapper : MonoBehaviour
{
    [SerializeField] private string firstScene = "MainMenu";

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        // ⚠️ On n'enregistre PAS ISaveService ici, pour éviter CS1503
        // ServiceLocator.Register<ISaveService>(new JsonSaveService());  // <- à NE PAS faire

        // On enregistre juste le SaveManager (qui sait utiliser JsonSaveService en interne)
        ServiceLocator.Register<SaveManager>(new SaveManager());
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "Boot")
            SceneManager.LoadScene(firstScene);
    }
}
