using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class ChangeScene : MonoBehaviour
{
    public Image fadeImage;
    // Start is called before the first frame update
    void Start()
    {
        if(SceneManager.GetActiveScene().name == "Game")
        {
            fadeImage.GetComponent<Animation>().Play("fadeOut");
            StartCoroutine(WaitAndInactive(fadeImage.GetComponent<Animation>().GetClip("fadeOut").length));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadScene(string name)
    {
        fadeImage.gameObject.SetActive(true);
        fadeImage.GetComponent<Animation>().Play("fadeIn");
        StartCoroutine(WaitAndLoadScene(fadeImage.GetComponent<Animation>().GetClip("fadeIn").length, name));
    }

    public void LoadSceneImmediately(string name)
    {
        SceneManager.LoadScene(name);
    }
    IEnumerator WaitAndLoadScene(float waitTime, string name)
    {
        yield return new WaitForSeconds(waitTime);
        SceneManager.LoadScene(name);
    }

    IEnumerator WaitAndInactive(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        fadeImage.gameObject.SetActive(false);
    }

}
