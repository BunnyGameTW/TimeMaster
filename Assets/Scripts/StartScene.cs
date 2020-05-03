using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartScene : MonoBehaviour
{
    public Sprite normalBackground;
    public Sprite nakedBackground;
    public Image backgroundImage;
    public Image effectImage;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnEnterEvent()
    {
        backgroundImage.sprite = nakedBackground;
        effectImage.GetComponent<Animator>().SetFloat("Blend", 0.5f);
    }

    public void OnExitEvent()
    {
        effectImage.GetComponent<Animator>().SetFloat("Blend", 0.0f);

        backgroundImage.sprite = normalBackground;
    }
}
