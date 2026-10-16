using System.Text.RegularExpressions;

namespace CybersecurityChatbot.Core;

public class ChatbotResponseService
{
    private readonly Random _random = new();

    private readonly Dictionary<string, string[]> _keywordResponses = new(StringComparer.OrdinalIgnoreCase)
    {
        ["password"] = new[]
        {
            "Make sure to use a strong, unique password for every account. Avoid personal details and reuse.",
            "A strong password should be long, random, and different across your accounts.",
            "Use a passphrase with mixed characters and turn on two-factor authentication where possible."
        },
        ["scam"] = new[]
        {
            "Never share your login details or banking details over email or text messages. Scammers often pose as trusted people.",
            "If a message creates urgency, pause and verify the sender through a trusted channel before acting.",
            "Be cautious of unexpected links, fake prizes, and requests for private information."
        },
        ["privacy"] = new[]
        {
            "Review your privacy settings regularly to control what others can see and what data is collected.",
            "Limit the information you share on social media and check which apps can access your personal data.",
            "Use strong privacy settings and only install apps that you actually trust."
        },
        ["phishing"] = new[]
        {
            "Be cautious of emails or texts asking for personal information, especially if they create urgency.",
            "Check the sender address and hover over links before clicking to confirm the destination is legitimate.",
            "If something feels urgent or suspicious, contact the company through their official website instead of the message."
        },
        ["malware"] = new[]
        {
            "Keep your operating system and antivirus software updated to reduce the risk of malware attacks.",
            "Avoid downloading files from unverified sources and email attachments from unknown senders.",
            "Scan new downloads and use a reputable security tool to protect your device."
        },
        ["safe"] = new[]
        {
            "Good security habits include using unique passwords, enabling verification steps, and staying alert to suspicious messages.",
            "Staying safe online is about awareness, consistent updates, and verifying requests before you act."
        }
    };

    private readonly List<string> _phishingTips = new()
    {
        "Be cautious of emails asking for personal information. Scammers often disguise themselves as trusted organisations.",
        "Always verify a request before clicking a link or opening an attachment from an unfamiliar sender.",
        "If an email seems rushed or urgent, stop and check the source through a trusted channel before responding.",
        "Phishing messages often look convincing. Look for spelling mistakes, mismatched links, and unusual senders."
    };

    private readonly List<string> _generalTips = new()
    {
        "Keep your software updated to patch known security issues.",
        "Use multi-factor authentication on important accounts for an extra layer of protection.",
        "Be careful with public Wi-Fi and avoid accessing sensitive accounts on unsecured networks.",
        "Think before you click: unexpected messages, links, or attachments are often the first step in an attack."
    };

    public string GetResponse(string userInput, ChatbotMemory memory)
    {
        if (string.IsNullOrWhiteSpace(userInput))
        {
            return "I’m ready to help. Ask me about password safety, scams, privacy, phishing, or general online security.";
        }

        var input = userInput.Trim();
        var sentiment = DetectSentiment(input);
        memory.TrackSentiment(sentiment);

        if (LooksLikeNameCapture(input))
        {
            var name = ExtractName(input);
            if (!string.IsNullOrWhiteSpace(name))
            {
                memory.SaveUserName(name);
                return $"Nice to meet you, {name}. I’ll remember your name and keep our conversation personal and helpful.";
            }
        }

        if (LooksLikeTopicPreference(input))
        {
            var topic = ExtractTopic(input);
            if (!string.IsNullOrWhiteSpace(topic) && _keywordResponses.ContainsKey(topic))
            {
                memory.SaveFavouriteTopic(topic);
                return $"Great! I’ll remember that you’re interested in {topic}. It is a crucial part of staying safe online.";
            }
        }

        if (ContainsAny(input, "tell me more", "explain more", "another tip", "continue", "more details"))
        {
            return ContinueCurrentTopic(memory, sentiment);
        }

        if (ContainsAny(input, "hello", "hi", "hey", "good morning", "good evening"))
        {
            return BuildSentimentResponse("Hello! I’m your cybersecurity assistant. Ask about passwords, scams, privacy, or phishing and I’ll help you stay secure.", sentiment);
        }

        foreach (var keyword in _keywordResponses.Keys)
        {
            if (input.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                var response = GetRandomResponse(_keywordResponses[keyword]);
                memory.TrackTopic(keyword);
                return BuildSentimentResponse(ApplyPersonalisation(response, memory), sentiment);
            }
        }

        if (ContainsAny(input, "phishing tip", "give me a phishing tip", "phishing"))
        {
            memory.TrackTopic("phishing");
            return BuildSentimentResponse(GetRandomResponse(_phishingTips), sentiment);
        }

        if (ContainsAny(input, "tip", "advice", "help"))
        {
            memory.TrackTopic("safety");
            return BuildSentimentResponse(GetRandomResponse(_generalTips), sentiment);
        }

        if (!string.IsNullOrWhiteSpace(memory.FavouriteTopic) && input.Contains(memory.FavouriteTopic, StringComparison.OrdinalIgnoreCase))
        {
            return BuildSentimentResponse($"Since you’re interested in {memory.FavouriteTopic}, here is a useful reminder: {GetRandomResponse(_keywordResponses[memory.FavouriteTopic])}.", sentiment);
        }

        return "I’m not sure I understand. Can you try rephrasing? I can help with password safety, scams, privacy, phishing, and general cybersecurity advice.";
    }

    private string ContinueCurrentTopic(ChatbotMemory memory, string sentiment)
    {
        if (!string.IsNullOrWhiteSpace(memory.LastTopic) && _keywordResponses.ContainsKey(memory.LastTopic))
        {
            var topicMessage = $"Absolutely — let’s keep talking about {memory.LastTopic}. {GetRandomResponse(_keywordResponses[memory.LastTopic])}";
            return BuildSentimentResponse(topicMessage, sentiment);
        }

        return BuildSentimentResponse("Of course. A good place to start is password safety: use a long, unique password and switch on two-factor authentication for important accounts.", sentiment);
    }

    private string BuildSentimentResponse(string response, string sentiment)
    {
        return sentiment switch
        {
            "worried" => $"It’s completely understandable to feel that way. {response}",
            "frustrated" => $"That can be frustrating, but you’re not alone. {response}",
            "curious" => $"That’s a great question. {response}",
            _ => response
        };
    }

    private string ApplyPersonalisation(string response, ChatbotMemory memory)
    {
        if (!string.IsNullOrWhiteSpace(memory.UserName) && !string.IsNullOrWhiteSpace(memory.FavouriteTopic))
        {
            return $"{memory.UserName}, since you’re interested in {memory.FavouriteTopic}, {response.ToLowerInvariant()}";
        }

        if (!string.IsNullOrWhiteSpace(memory.UserName))
        {
            return $"{memory.UserName}, {response}";
        }

        return response;
    }

    private static bool LooksLikeNameCapture(string input)
    {
        return input.Contains("my name is", StringComparison.OrdinalIgnoreCase)
            || input.Contains("i am called", StringComparison.OrdinalIgnoreCase)
            || input.Contains("i'm called", StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractName(string input)
    {
        var marker = "my name is";
        if (input.Contains(marker, StringComparison.OrdinalIgnoreCase))
        {
            return input[(input.IndexOf(marker, StringComparison.OrdinalIgnoreCase) + marker.Length)..].Trim();
        }

        marker = "i am called";
        if (input.Contains(marker, StringComparison.OrdinalIgnoreCase))
        {
            return input[(input.IndexOf(marker, StringComparison.OrdinalIgnoreCase) + marker.Length)..].Trim();
        }

        marker = "i'm called";
        if (input.Contains(marker, StringComparison.OrdinalIgnoreCase))
        {
            return input[(input.IndexOf(marker, StringComparison.OrdinalIgnoreCase) + marker.Length)..].Trim();
        }

        return string.Empty;
    }

    private static bool LooksLikeTopicPreference(string input)
    {
        return input.Contains("interested in", StringComparison.OrdinalIgnoreCase)
            || input.Contains("i like", StringComparison.OrdinalIgnoreCase)
            || input.Contains("favourite topic", StringComparison.OrdinalIgnoreCase)
            || input.Contains("favorite topic", StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractTopic(string input)
    {
        foreach (var keyword in new[] { "password", "scam", "privacy", "phishing", "malware", "safety" })
        {
            if (input.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return keyword;
            }
        }

        return string.Empty;
    }

    private static string DetectSentiment(string input)
    {
        if (ContainsAny(input, "worried", "afraid", "scared", "anxious", "nervous"))
        {
            return "worried";
        }

        if (ContainsAny(input, "frustrated", "annoyed", "confused", "stuck", "lost"))
        {
            return "frustrated";
        }

        if (ContainsAny(input, "curious", "interested", "want to know", "wondering", "learn"))
        {
            return "curious";
        }

        return "neutral";
    }

    private static bool ContainsAny(string input, params string[] phrases)
    {
        foreach (var phrase in phrases)
        {
            if (string.IsNullOrWhiteSpace(phrase))
            {
                continue;
            }

            if (Regex.IsMatch(input, $@"\b{Regex.Escape(phrase)}\b", RegexOptions.IgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private string GetRandomResponse(string[] values)
    {
        return values[_random.Next(values.Length)];
    }

    private string GetRandomResponse(List<string> values)
    {
        return values[_random.Next(values.Count)];
    }
}
