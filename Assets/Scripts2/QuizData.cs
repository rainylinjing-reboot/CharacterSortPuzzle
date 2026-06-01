using UnityEngine;

[System.Serializable]
public class QuizData
{
    public QuizType quizType;
    public string questionText;
    public int answer;
    public int wrongAnswer;

    public QuizData(QuizType newQuizType, string newQuestionText, int newAnswer, int newWrongAnswer)
    {
        quizType = newQuizType;
        questionText = newQuestionText;
        answer = newAnswer;
        wrongAnswer = newWrongAnswer;
    }
}

public enum QuizType
{
    Add,
    Multiply,
    Luck
}