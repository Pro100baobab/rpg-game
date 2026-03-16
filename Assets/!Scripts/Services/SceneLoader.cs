using UnityEngine.SceneManagement;
using System;

public class SceneLoader : ISceneLoader
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void LoadSceneAsync(string sceneName, Action onLoaded = null)
    {
        var operation = SceneManager.LoadSceneAsync(sceneName);
        if (onLoaded != null)
            operation.completed += _ => onLoaded();
    }
}