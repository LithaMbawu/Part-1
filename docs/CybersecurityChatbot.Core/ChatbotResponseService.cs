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
        ["multi-factor authentication"] = new[]
        {
            "Turn on multi-factor authentication for important accounts. Prefer an authenticator app or security key when available, and keep recovery codes private.",
            "A second sign-in factor helps protect an account if its password is exposed. Never share one-time verification codes."
        },
        ["social engineering"] = new[]
        {
            "Social engineering relies on pressure or impersonation. Pause, verify through a separate trusted channel, and follow your organisation's reporting process.",
            "A convincing caller can still be an impersonator. Never share sign-in codes; contact the organisation using a known official number."
        },
        ["identity theft"] = new[]
        {
            "Use unique passwords, multi-factor authentication, and account alerts. Share personal information only when necessary and through verified channels.",
            "If your identity may be misused, contact affected institutions using their official details and follow local identity-protection guidance."
        },
        ["backup"] = new[]
        {
            "Keep regular backups of important files, with at least one copy separate from your everyday device or account.",
            "Test that you can restore important files from your backup; a backup you cannot restore may not help when needed."
        },
        ["wifi"] = new[]
        {
            "Use trusted networks for sensitive activity. Verify a hotspot name before joining, and use your mobile connection if a public network seems suspicious.",
            "Keep your home router updated, change its default admin password, and use modern Wi-Fi security settings."
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

        if (ContainsAny(input, "tell me more", "explain more", "another tip", "continue", "more details", "what else", "why"))
        {
            return ContinueCurrentTopic(memory, sentiment);
        }

        if (ContainsAny(input, "hello", "hi", "hey", "good morning", "good evening"))
        {
            var greeting = string.IsNullOrWhiteSpace(memory.UserName) ? "Hello!" : $"Hello, {memory.UserName}!";
            return BuildSentimentResponse($"{greeting} I’m your cybersecurity assistant. Ask about passwords, scams, privacy, phishing, malware, or safe online habits.", sentiment);
        }

        if (ContainsAny(input, "thank you", "thanks", "appreciate it"))
        {
            return BuildSentimentResponse("You’re welcome. Ask me to explain more or give you another security tip any time.", sentiment);
        }

        if (ContainsAny(input, "what can you do", "help menu", "what topics", "what do you know"))
        {
            return "I can discuss passwords, phishing, scams, privacy, malware, multi-factor authentication, social engineering, identity protection, backups, Wi-Fi, tasks, reminders, the quiz, and activity history.";
        }

        var matchedTopic = FindTopic(input);
        if (matchedTopic is not null)
        {
            var response = GetRandomResponse(_keywordResponses[matchedTopic]);
            memory.TrackTopic(matchedTopic);
            return BuildSentimentResponse(ApplyPersonalisation(response, memory), sentiment);
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

        if (!string.IsNullOrWhiteSpace(memory.FavouriteTopic) && FindTopic(input) == memory.FavouriteTopic)
        {
            return BuildSentimentResponse($"Since you’re interested in {memory.FavouriteTopic}, here is a useful reminder: {GetRandomResponse(_keywordResponses[memory.FavouriteTopic])}.", sentiment);
        }

        var topicHint = GetLikelyTopicHint(input);
        return topicHint is null
            ? "I’m not sure I understood. Could you rephrase or name the security topic? I can help with passwords, phishing, scams, privacy, malware, MFA, backups, and Wi-Fi safety."
            : $"Are you asking about {topicHint}? Tell me a little more and I’ll give relevant safety guidance. I can also help with passwords, phishing, scams, privacy, malware, MFA, backups, and Wi-Fi safety.";
    }

    private static readonly IReadOnlyDictionary<string, string> TopicAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["password"] = "password", ["passwords"] = "password", ["passphrase"] = "password", ["passphrases"] = "password",
        ["scam"] = "scam", ["scams"] = "scam", ["fraud"] = "scam", ["fraudulent"] = "scam",
        ["privacy"] = "privacy", ["private"] = "privacy", ["data protection"] = "privacy",
        ["phishing"] = "phishing", ["phish"] = "phishing", ["smishing"] = "phishing", ["email scam"] = "phishing",
        ["malware"] = "malware", ["virus"] = "malware", ["ransomware"] = "malware", ["spyware"] = "malware",
        ["security"] = "safe", ["safety"] = "safe", ["safe"] = "safe", ["online safety"] = "safe",
        ["multi factor authentication"] = "multi-factor authentication", ["multifactor authentication"] = "multi-factor authentication",
        ["two factor authentication"] = "multi-factor authentication", ["two factor"] = "multi-factor authentication",
        ["2fa"] = "multi-factor authentication", ["mfa"] = "multi-factor authentication",
        ["social engineering"] = "social engineering", ["identity theft"] = "identity theft", ["identity fraud"] = "identity theft",
        ["backup"] = "backup", ["backups"] = "backup", ["back up"] = "backup",
        ["wifi"] = "wifi", ["wi fi"] = "wifi", ["wireless network"] = "wifi", ["public network"] = "wifi"
    };

    private static string? FindTopic(string input)
    {
        var normalized = Regex.Replace(input.ToLowerInvariant(), @"[^a-z0-9]+", " ").Trim();
        foreach (var (alias, topic) in TopicAliases.OrderByDescending(item => item.Key.Length))
        {
            if (Regex.IsMatch(normalized, $@"\b{Regex.Escape(alias)}\b", RegexOptions.IgnoreCase))
            {
                return topic;
            }
        }

        return null;
    }

    private static string? GetLikelyTopicHint(string input)
    {
        if (ContainsAny(input, "hacked", "account taken over", "someone logged in")) return "account security and recovery";
        if (ContainsAny(input, "link", "attachment", "email", "text message", "sms")) return "suspicious messages and phishing";
        if (ContainsAny(input, "phone", "laptop", "computer", "device")) return "device security";
        if (ContainsAny(input, "code", "verification", "sign in", "login")) return "sign-in security and verification codes";
        return null;
    }

    private string ContinueCurrentTopic(ChatbotMemory memory, string sentiment)
    {
        if (!string.IsNullOrWhiteSpace(memory.LastTopic) && _keywordResponses.TryGetValue(memory.LastTopic, out var responses))
        {
            var topicMessage = $"Absolutely — let’s keep talking about {memory.LastTopic}. {GetRandomResponse(responses)}";
            return BuildSentimentResponse(topicMessage, sentiment);
        }

        return BuildSentimentResponse("Of course. A good place to start is password safety: use a long, unique password and switch on multi-factor authentication for important accounts.", sentiment);
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
            || input.Contains("i'm called", StringComparison.OrdinalIgnoreCase)
            || input.StartsWith("call me ", StringComparison.OrdinalIgnoreCase);
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

        marker = "call me";
        if (input.StartsWith(marker, StringComparison.OrdinalIgnoreCase))
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
            || input.Contains("favorite topic", StringComparison.OrdinalIgnoreCase)
            || input.Contains("i care about", StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractTopic(string input)
    {
        return FindTopic(input) ?? string.Empty;
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
