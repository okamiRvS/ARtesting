using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class addFoodList : MonoBehaviour {

    int numObj = 0;
    public Text foodsText;

    /// <summary>
    /// The debugText.
    /// </summary>
    [Tooltip("The debugText text.")]
    [SerializeField] private Text debugText = null;

    void Update () {

        int cont = 0;
        foreach (GameObject anchor in GameObject.FindGameObjectsWithTag("anchor"))
        {
            cont++;
            debugText.text = debugText.text + "\n" + cont;
            Debug.Log("cont " + cont);
        }

        if (cont > numObj)
        {
            numObj++;
            var obj = Instantiate(foodsText, gameObject.transform);
            Vector2 pos = new Vector2(obj.rectTransform.anchoredPosition.x, obj.rectTransform.anchoredPosition.y - 50 * numObj);
            obj.rectTransform.anchoredPosition = pos;

            obj.GetComponent<Text>().text = numObj.ToString();

        }
    }
}
