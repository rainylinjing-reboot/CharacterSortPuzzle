using TMPro;
using UnityEngine;

public class QuizManager : MonoBehaviour
{
    [Header("Quiz UI")]
    public TextMeshProUGUI quizText;

    [Header("Quiz Setting")]
    public bool useLuckQuiz = true;

    [Header("Fallback Number Setting")]
    public int oneDigitMin = 1;
    public int oneDigitMax = 9;
    public int twoDigitMin = 10;
    public int twoDigitMax = 19;
    public int fallbackOneDigitChancePercent = 100;

    [Header("Luck Quiz Control")]
    public int consecutiveLuckQuizCount = 0;

    private QuizData currentQuizData;

    void Start()
    {
        CreateNewQuiz();
    }

    public QuizData CreateNewQuiz()
    {
        QuizType selectedQuizType = SelectQuizType();

        if (selectedQuizType == QuizType.Add)
        {
            currentQuizData = CreateAddQuiz();
        }
        else if (selectedQuizType == QuizType.Multiply)
        {
            currentQuizData = CreateMultiplyQuiz();
        }
        else
        {
            currentQuizData = CreateLuckQuiz();
        }

        if (currentQuizData.quizType == QuizType.Luck)
        {
            consecutiveLuckQuizCount++;
        }
        else
        {
            consecutiveLuckQuizCount = 0;
        }

        ShowQuiz(currentQuizData);

        return currentQuizData;
    }

    QuizType SelectQuizType()
    {
        if (useLuckQuiz == false)
        {
            return Random.Range(0, 2) == 0 ? QuizType.Add : QuizType.Multiply;
        }

        int maxConsecutiveLuckQuiz = GetMaxConsecutiveLuckQuiz();

        if (consecutiveLuckQuizCount >= maxConsecutiveLuckQuiz)
        {
            return Random.Range(0, 2) == 0 ? QuizType.Add : QuizType.Multiply;
        }

        int quizTypeIndex = Random.Range(0, 3);

        if (quizTypeIndex == 0)
            return QuizType.Add;

        if (quizTypeIndex == 1)
            return QuizType.Multiply;

        return QuizType.Luck;
    }

    int GetMaxConsecutiveLuckQuiz()
    {
        if (LuckyRunDifficultyManager.instance != null)
        {
            return LuckyRunDifficultyManager.instance.GetCurrentMaxConsecutiveLuckQuiz();
        }

        return 1;
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
        bool useOneDigitQuestion = ShouldUseOneDigitQuestion();

        if (useOneDigitQuestion == true)
        {
            leftNumber = GetOneDigitNumber();
            rightNumber = GetOneDigitNumber();
            return;
        }

        bool twoDigitOnLeft = Random.Range(0, 2) == 0;

        if (twoDigitOnLeft == true)
        {
            leftNumber = GetTwoDigitNumber();
            rightNumber = GetOneDigitNumber();
        }
        else
        {
            leftNumber = GetOneDigitNumber();
            rightNumber = GetTwoDigitNumber();
        }
    }

    bool ShouldUseOneDigitQuestion()
    {
        if (LuckyRunDifficultyManager.instance != null)
        {
            return LuckyRunDifficultyManager.instance.ShouldUseOneDigitQuestion();
        }

        return Random.Range(0, 100) < fallbackOneDigitChancePercent;
    }

    int GetOneDigitNumber()
    {
        if (LuckyRunDifficultyManager.instance != null)
        {
            return LuckyRunDifficultyManager.instance.GetOneDigitNumber();
        }

        return Random.Range(oneDigitMin, oneDigitMax + 1);
    }

    int GetTwoDigitNumber()
    {
        if (LuckyRunDifficultyManager.instance != null)
        {
            return LuckyRunDifficultyManager.instance.GetTwoDigitNumber();
        }

        return Random.Range(twoDigitMin, twoDigitMax + 1);
    }

    QuizData CreateLuckQuiz()
    {
        string question = "← OR →";

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