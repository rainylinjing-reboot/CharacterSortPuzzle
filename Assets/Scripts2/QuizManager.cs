using TMPro;
using UnityEngine;

public class QuizManager : MonoBehaviour
{
    [Header("Quiz UI")]
    public TextMeshProUGUI quizText;

    [Header("Quiz Setting")]
    public bool useLuckQuiz = true;

    [Header("Number Difficulty")]
    public bool useTwoDigitNumber = true;
    public int oneDigitMin = 1;
    public int oneDigitMax = 9;
    public int twoDigitMin = 10;
    public int twoDigitMax = 19;
    public int twoDigitChancePercent = 45;

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
        int leftNumber;
        int rightNumber;

        CreateQuestionNumbers(out leftNumber, out rightNumber);

        int answer = leftNumber + rightNumber;
        int wrongAnswer = CreateWrongAnswer(answer);

        string question = leftNumber + " + " + rightNumber + " = ?";

        return new QuizData(QuizType.Add, question, answer, wrongAnswer);
    }

    QuizData CreateMultiplyQuiz()
    {
        int leftNumber;
        int rightNumber;

        CreateQuestionNumbers(out leftNumber, out rightNumber);

        int answer = leftNumber * rightNumber;
        int wrongAnswer = CreateWrongAnswer(answer);

        string question = leftNumber + " x " + rightNumber + " = ?";

        return new QuizData(QuizType.Multiply, question, answer, wrongAnswer);
    }

    void CreateQuestionNumbers(out int leftNumber, out int rightNumber)
    {
        leftNumber = CreateOneDigitNumber();
        rightNumber = CreateOneDigitNumber();

        if (useTwoDigitNumber == false)
            return;

        bool useTwoDigit = Random.Range(0, 100) < twoDigitChancePercent;

        if (useTwoDigit == false)
            return;

        bool twoDigitOnLeft = Random.Range(0, 2) == 0;

        if (twoDigitOnLeft == true)
        {
            leftNumber = CreateTwoDigitNumber();
        }
        else
        {
            rightNumber = CreateTwoDigitNumber();
        }
    }

    int CreateOneDigitNumber()
    {
        return Random.Range(oneDigitMin, oneDigitMax + 1);
    }

    int CreateTwoDigitNumber()
    {
        return Random.Range(twoDigitMin, twoDigitMax + 1);
    }

    QuizData CreateLuckQuiz()
    {
        string question = "← or →";

        return new QuizData(QuizType.Luck, question, -1, -1);
    }

    int CreateWrongAnswer(int answer)
    {
        int wrongAnswer = answer;

        int safetyCount = 0;

        while (wrongAnswer == answer && safetyCount < 30)
        {
            int offset;

            if (answer >= 50)
            {
                offset = Random.Range(-10, 11);
            }
            else
            {
                offset = Random.Range(-5, 6);
            }

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