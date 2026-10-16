# Cybersecurity Awareness Chatbot – Part 2

This project is a Windows desktop chatbot application designed for cybersecurity awareness training. It includes a polished WPF interface, dynamic responses, keyword detection, memory tracking, sentiment adjustments, and a voice greeting using a bundled WAV file.

## Features

- User-friendly WPF interface with dark styling and spacing
- ASCII cyber-themed branding translated into the GUI
- Keyword recognition for password, scam, privacy, phishing, and malware topics
- Randomized cybersecurity tips to keep interactions varied
- Follow-up conversation flow for responses like “tell me more” and “another tip”
- Memory feature for the user name and favourite cybersecurity topic
- Sentiment detection for worried, curious, and frustrated input
- Default fallback response for unclear or unexpected input
- Clean object-oriented structure using a reusable chatbot core service and memory model

## Project structure

- `CybersecurityChatbot.Core/` – reusable chatbot logic
- `CybersecurityChatbot.Gui/` – WPF interface and user interactions
- `assets/` – ASCII art and WAV voice greeting asset files

## Run the project

1. Open the solution in Visual Studio on Windows.
2. Set `CybersecurityChatbot.Gui` as the startup project.
3. Press F5 to run the application.

## Notes

- The voice greeting is implemented by playing the bundled `voice-greeting.wav` file.
- The ASCII art is stored in `assets/cybersecurity-ascii.svg` and also mirrored in the GUI design.
- The logic is split into a reusable core service so the chatbot can be extended in later parts of the project.
