using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskGenerator : MonoBehaviour
{
    [SerializeField] RectTransform task1;
    [SerializeField] RectTransform task2;
    [SerializeField] RectTransform task3;
    [SerializeField] bool stopGenerating = false;
    [SerializeField] float generateTime;
    [SerializeField] float generateDelay;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("GenerateTask", generateTime, generateDelay);
        InvokeRepeating("GenerateTask", generateTime, generateDelay);
        InvokeRepeating("GenerateTask", generateTime, generateDelay);
    }

    public void GenerateTask()
    {
        Instantiate(task1, this.transform);
        Instantiate(task2, this.transform);
        Instantiate(task3, this.transform);
        if (stopGenerating)
        {
            CancelInvoke("GenerateTask");
        }
    }
}
