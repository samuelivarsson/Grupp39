using UnityEngine;
using UnityEngine.UI;

public class Taping : MonoBehaviour
{
    [SerializeField] Image timerBar;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 imagePos = Camera.main.WorldToScreenPoint(this.transform.position);
        timerBar.transform.position = imagePos;
    }
}
