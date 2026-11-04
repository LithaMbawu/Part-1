namespace CybersecurityChatbot.Core;

public sealed record QuizQuestion(string Prompt, IReadOnlyList<string> Options, int CorrectOptionIndex, string Explanation);

public sealed record QuizAnswerResult(bool IsCorrect, string Explanation, int CorrectAnswers, int QuestionsAnswered);

public sealed class QuizService
{
    private readonly IReadOnlyList<QuizQuestion> _questions = new List<QuizQuestion>
    {
        new("What is the safest response to an unexpected email asking for your password?", new[] { "Reply with the password", "Report it as phishing", "Forward it to friends", "Click its link to verify" }, 1, "Legitimate services should not ask you to disclose your password by email. Report suspicious messages through an approved channel."),
        new("Which password practice is safest?", new[] { "Reuse one strong password everywhere", "Use a unique, long password for each account", "Use your birthday", "Change one character for each account" }, 1, "Unique passwords limit the damage if one service is breached. A password manager can help you create and store them."),
        new("True or false: Multi-factor authentication adds a security layer beyond a password.", new[] { "True", "False" }, 0, "A second verification factor makes account access harder even if a password is stolen."),
        new("What should you do before opening an unexpected attachment?", new[] { "Open it quickly", "Verify the sender and whether you expected it", "Disable antivirus", "Forward it to your contacts" }, 1, "Confirm unexpected attachments with the sender through a separate trusted channel."),
        new("Which is a common sign of a phishing message?", new[] { "A request to act urgently through an unfamiliar link", "A saved contact name", "A message you expected", "A bookmarked website" }, 0, "Urgency, unusual requests, and mismatched links are warning signs. Verify independently rather than clicking."),
        new("True or false: Public Wi-Fi always encrypts every connection for you.", new[] { "True", "False" }, 1, "Public Wi-Fi may be untrusted. Prefer secure sites and avoid sensitive activity on networks you do not trust."),
        new("How should you check where a link leads?", new[] { "Trust the displayed link text", "Use a preview or navigate to the official site yourself", "Click it to see", "Share it with someone" }, 1, "Inspect the actual destination carefully, or use a known official address instead of an unexpected link."),
        new("What is social engineering?", new[] { "A type of software update", "Manipulating people into revealing information or taking unsafe actions", "A secure password format", "A network cable standard" }, 1, "Social engineering uses persuasion or deception to exploit human trust and bypass safeguards."),
        new("True or false: Software updates can fix security weaknesses.", new[] { "True", "False" }, 0, "Updates often include security fixes. Install them from official sources and keep devices supported."),
        new("You receive a verification code you did not request. What should you do?", new[] { "Share it with the caller", "Ignore the request and secure the account if needed", "Post it online", "Reply to the message with your password" }, 1, "Never share verification codes. An unexpected code may mean someone is trying to access your account."),
        new("Which download is the safest choice?", new[] { "A file from an unknown pop-up", "Software from its official source", "An attachment from a stranger", "A cracked installer" }, 1, "Download software from the vendor's official website or a trusted app store, and keep it updated."),
        new("True or false: You should report a suspected work phishing message using your organisation's process.", new[] { "True", "False" }, 0, "Reporting helps the security team investigate and protect others. Do not interact with suspicious content."),
    };

    private int _currentIndex;
    private int _correctAnswers;
    private bool _hasAnswered;

    public bool IsStarted { get; private set; }
    public bool IsComplete { get; private set; }
    public int CorrectAnswers => _correctAnswers;
    public int QuestionsAnswered => IsComplete ? _questions.Count : _currentIndex;
    public int TotalQuestions => _questions.Count;
    public QuizQuestion? CurrentQuestion => IsStarted && !IsComplete ? _questions[_currentIndex] : null;

    public void Start()
    {
        _currentIndex = 0;
        _correctAnswers = 0;
        _hasAnswered = false;
        IsStarted = true;
        IsComplete = false;
    }

    public QuizAnswerResult SubmitAnswer(int selectedOptionIndex)
    {
        if (!IsStarted || IsComplete || _hasAnswered)
        {
            throw new InvalidOperationException("Start the quiz and answer the current question only once.");
        }

        var question = _questions[_currentIndex];
        if (selectedOptionIndex < 0 || selectedOptionIndex >= question.Options.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(selectedOptionIndex));
        }

        _hasAnswered = true;
        var isCorrect = selectedOptionIndex == question.CorrectOptionIndex;
        if (isCorrect)
        {
            _correctAnswers++;
        }

        return new QuizAnswerResult(isCorrect, question.Explanation, _correctAnswers, _currentIndex + 1);
    }

    public bool MoveNext()
    {
        if (!_hasAnswered || IsComplete)
        {
            return false;
        }

        if (_currentIndex == _questions.Count - 1)
        {
            IsComplete = true;
            return false;
        }

        _currentIndex++;
        _hasAnswered = false;
        return true;
    }
}