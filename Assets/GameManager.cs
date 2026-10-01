using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

public enum  answerType {
    tool,
    thorb,
    number
}
public class GameManager : MonoBehaviour
{
    [SerializeField] private List<Questions> questions;

    //For non number answers we can give them IDs?
    private float currentAnswer = 0;
    private float correctAnswer = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void checkAnswer()
    {

    }
}
