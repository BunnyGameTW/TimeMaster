using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class ChangeScene : MonoBehaviour
{
    public Image fadeImage;
    public string sceneName;
    public string nowSceneName;//TODO 糟糕

    bool canChangeScene, canChangeNowScene;
    // Start is called before the first frame update
    void Start()
    {
        if(SceneManager.GetActiveScene().name == "Game")
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.GetComponent<Animation>().Play("fadeOut");
            StartCoroutine(WaitAndInactive(fadeImage.GetComponent<Animation>().GetClip("fadeOut").length));
        }

        canChangeScene = false;
        StartCoroutine(LoadSceneAsync(sceneName));
    }



    public void LoadScene(bool isLoadNowScene)
    {
        fadeImage.gameObject.SetActive(true);
        fadeImage.GetComponent<Animation>().Play("fadeIn");
        StartCoroutine(WaitAndLoadScene(fadeImage.GetComponent<Animation>().GetClip("fadeIn").length, isLoadNowScene));
    }

    public void LoadSceneImmediately()
    {
        canChangeScene = true;
    }

    IEnumerator WaitAndLoadScene(float waitTime, bool isLoadNowScene)
    {
        yield return new WaitForSeconds(waitTime);
        if (isLoadNowScene) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        else canChangeScene = true; 
    }

    IEnumerator WaitAndInactive(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        fadeImage.gameObject.SetActive(false);
    }

    IEnumerator LoadSceneAsync(string name)
    {
        yield return null;

        //Begin to load the Scene you specify
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(name);
        asyncOperation.allowSceneActivation = false;
        while (!asyncOperation.isDone)
        {
            if (asyncOperation.progress >= 0.9f)
            {
                if (canChangeScene)
                    asyncOperation.allowSceneActivation = true;
            }

            yield return null;
        }
    }

}
