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
    public GameObject titleGameObject;
    RectTransform backgroundRectTransform, effectRectTransform, titleRectTransform;
    Vector3 effectOriginPosition, titleOriginPosition;
    const float BACKGROUND_MOVE_RATIO = 0.005f;
    const float MIDDLE_MOVE_RATIO = 0.003f;
    const float FOREGROUND_MOVE_RATIO = 0.02f;

    // Start is called before the first frame update
    void Start()
    {
        backgroundRectTransform = backgroundImage.transform.parent.GetComponent<RectTransform>();
        effectRectTransform = effectImage.transform.parent.GetComponent<RectTransform>();
        effectOriginPosition = effectRectTransform.localPosition;
        titleRectTransform = titleGameObject.GetComponent<RectTransform>();
        titleOriginPosition = titleRectTransform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        Vector3 offsetVector = new Vector3(Input.mousePosition.x - Screen.width / 2, Input.mousePosition.y - Screen.height/ 2, 0);
        backgroundRectTransform.localPosition = -offsetVector * BACKGROUND_MOVE_RATIO;
        titleRectTransform.localPosition = titleOriginPosition + offsetVector * MIDDLE_MOVE_RATIO;
        effectRectTransform.localPosition = effectOriginPosition + offsetVector * FOREGROUND_MOVE_RATIO;
#else
        Vector3 offsetVector = new Vector3(Input.acceleration.x, Input.acceleration.y, 0);
        backgroundRectTransform.localPosition = -offsetVector * 15.0f;
        titleRectTransform.localPosition = titleOriginPosition + offsetVector * 9.0f;
        effectRectTransform.localPosition = effectOriginPosition + offsetVector * 18.0f;
#endif
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
