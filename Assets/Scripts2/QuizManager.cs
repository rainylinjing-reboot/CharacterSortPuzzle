using TMPro;
using UnityEngine;

public class QuizManager : MonoBehaviour
{
    [Header("Quiz UI")]
    public TextMeshProUGUI quizText;

    [Header("Quiz Setting")]
    public bool useLuckQuiz = true;

    private QuizData currentQuizData;

    void Start()
    {
        CreateNewQuiz();
    }

    public QuizData CreateNewQuiz()
    {
        int quizTypeIndex;

        if (useLuckQuiz == true)
        {
            quizTypeIndex = Random.Range(0, 3);
        }
        else
        {
            quizTypeIndex = Random.Range(0, 2);
        }

        if (quizTypeIndex == 0)
        {
            currentQuizData = CreateAddQuiz();
        }
        else if (quizTypeIndex == 1)
        {
            currentQuizData = CreateMultiplyQuiz();
        }
        else
        {
            currentQuizData = CreateLuckQuiz();
        }

        ShowQuiz(currentQuizData);

        return currentQuizData;
    }

    QuizData CreateAddQuiz()
    {
        int leftNumber = Random.Range(1, 10);
        int rightNumber = Random.Range(1, 10);

        int answer = leftNumber + rightNumber;
        int wrongAnswer = CreateWrongAnswer(answer);

        string question = leftNumber + " + " + rightNumber + " = ?";

        return new QuizData(QuizType.Add, question, answer, wrongAnswer);
    }

    QuizData CreateMultiplyQuiz()
    {
        int leftNumber = Random.Range(1, 10);
        int rightNumber = Random.Range(1, 10);

        int answer = leftNumber * rightNumber;
        int wrongAnswer = CreateWrongAnswer(answer);

        string question = leftNumber + " x " + rightNumber + " = ?";

        return new QuizData(QuizType.Multiply, question, answer, wrongAnswer);
    }

    QuizData CreateLuckQuiz()
    {
        string question = "← or →, ?";

        return new QuizData(QuizType.Luck, question, -1, -1);
    }

    int CreateWrongAnswer(int answer)
    {
        int wrongAnswer = answer;

        int safetyCount = 0;

        while (wrongAnswer == answer && safetyCount < 20)
        {
            int offset = Random.Range(-3, 4);

            if (offset == 0)
            {
                offset = 1;
            }

            wrongAnswer = answer + offset;

            if (wrongAnswer < 0)
            {
                wrongAnswer = answer + Mathf.Abs(offset);
            }

            safetyCount++;
        }

        return wrongAnswer;
    }

    void ShowQuiz(QuizData quizData)
    {
        if (quizText == null)
            return;

        quizText.text = quizData.questionText;
    }

    public QuizData GetCurrentQuiz()
    {
        return currentQuizData;
    }
}