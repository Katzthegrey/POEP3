using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SideBar_Function
{
    public partial class MainWindow : Window
    {
        // ChatBot variables
        private string userName = string.Empty;
        private readonly List<string> answers = new List<string>();
        private readonly List<string> ignoreWords = new List<string>();
        private readonly List<string> positiveResponses = new List<string>();
        private readonly List<string> concernResponses = new List<string>();
        private readonly List<string> gratitudeResponses = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            PlayGreeting();
            InitializeChatBot();
        }

        private void InitializeChatBot()
        {
            // Initialize all lists
            InitializeAnswers();
            InitializeIgnoreWords();
            InitializeSentimentResponses();

            // Add welcome message to chat
            chatbot_res.Items.Add("ChatBot: Welcome to the cybersecurity bot!");
            chatbot_res.Items.Add("ChatBot: Please enter your name below and press Submit");
        }

        private void InitializeAnswers()
        {
            answers.Add("Passwords help as a decryption key to sensitive data which has been encrypted to limit who has access to the data.".ToLower());
            answers.Add("If a password is compromised, an attacker could gain access to sensitive data rendering the encryption useless.".ToLower());
            answers.Add("Strong and unique passwords are recommended to further enhance your security".ToLower());
            answers.Add("Phishing is a method attackers use to disguise themselves as legitimate companies to trick users into revealing sensitive information.".ToLower());
            answers.Add("The most common type of phishing is email phishing where attackers send emails appearing to be from legitimate sources.".ToLower());
            answers.Add("To avoid phishing attacks, always verify email addresses and website URLs before entering any information.".ToLower());
        }

        private void InitializeIgnoreWords()
        {
            ignoreWords.AddRange(new[] { "tell", "me", "about", "what", "how", "is", "are", "the", "a", "an" });
        }

        private void InitializeSentimentResponses()
        {
            // Concern responses
            concernResponses.AddRange(new[] {
                "I understand you're worried about {0}. Here's how you can prevent cyber attacks related to this...",
                "Security concerns about {0} are valid. Let me explain how to stay protected...",
                "I hear your concern about {0}. Cybersecurity is important, and here's what you can do..."
            });

            // Gratitude responses
            gratitudeResponses.AddRange(new[] {
                "You're welcome! I'm happy to help with your cybersecurity questions.",
                "Glad I could assist! Let me know if you have other security concerns.",
                "No problem at all! Stay safe online."
            });

            // Positive responses
            positiveResponses.AddRange(new[] {
                "Great question! Here's what you should know...",
                "That's an important topic. Let me explain...",
                "I'm happy to provide information about this security topic..."
            });
        }

        private void PlayGreeting()
        {
            try
            {
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Greeting.wav");

                if (!File.Exists(fullPath))
                {
                    string projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
                    fullPath = Path.Combine(projectRoot, "Greeting.wav");

                    if (!File.Exists(fullPath))
                    {
                        Debug.WriteLine("Greeting.wav not found at: " + fullPath);
                        return;
                    }
                }

                Task.Run(() =>
                {
                    using (SoundPlayer player = new SoundPlayer(fullPath))
                    {
                        player.PlaySync();
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error playing greeting: " + ex.Message);
            }
        }

        private void ProcessQuestion()
        {
            string question = user_questions.Text.Trim();
            if (string.IsNullOrEmpty(question)) return;

            user_questions.Text = "";

            // Check if we need to set the user name
            if (string.IsNullOrEmpty(userName))
            {
                if (Regex.IsMatch(question, @"^[A-Za-z\s'-]+$"))
                {
                    userName = question;
                    chatbot_res.Items.Add($"{userName}: {question}");
                    chatbot_res.Items.Add("ChatBot: Welcome " + userName + "! How may I be of assistance today?");
                }
                else
                {
                    chatbot_res.Items.Add($"You: {question}");
                    chatbot_res.Items.Add("ChatBot: Invalid name. Only letters and spaces allowed. Please try again.");
                }
                return;
            }

            // Add user question to chat
            chatbot_res.Items.Add($"{userName}: {question}");

            // Check for exit command
            if (question.ToLower() == "exit")
            {
                chatbot_res.Items.Add("ChatBot: Thank you for using the cybersecurity awareness bot. Goodbye!");
                chatbot_res.ScrollIntoView(chatbot_res.Items[chatbot_res.Items.Count - 1]);
                return;
            }

            // Check for gratitude expressions
            if (IsGratitudeExpression(question))
            {
                RespondToGratitude();
                return;
            }

            // Check for concerns
            string concernTopic = DetectConcern(question);
            if (!string.IsNullOrEmpty(concernTopic))
            {
                RespondToConcern(concernTopic);
                return;
            }

            // Process technical questions
            ProcessTechnicalQuestion(question);
        }

        private bool IsGratitudeExpression(string input)
        {
            var gratitudeKeywords = new List<string> { "thank", "thanks", "appreciate", "grateful" };
            return gratitudeKeywords.Any(keyword => input.ToLower().Contains(keyword));
        }

        private void RespondToGratitude()
        {
            Random rand = new Random();
            string response = gratitudeResponses[rand.Next(gratitudeResponses.Count)];
            chatbot_res.Items.Add("ChatBot: " + response);
        }

        private string DetectConcern(string input)
        {
            var concernKeywords = new List<string> { "worried", "concerned", "afraid", "scared", "nervous", "anxious" };
            var securityTopics = new List<string> { "password", "phishing", "hack", "attack", "virus", "malware" };

            foreach (string keyword in concernKeywords)
            {
                if (input.ToLower().Contains(keyword))
                {
                    foreach (string topic in securityTopics)
                    {
                        if (input.ToLower().Contains(topic))
                        {
                            return topic;
                        }
                    }
                    return "security"; // Default topic if none specifically mentioned
                }
            }
            return null;
        }

        private void RespondToConcern(string topic)
        {
            Random rand = new Random();
            string response = string.Format(concernResponses[rand.Next(concernResponses.Count)], topic);
            chatbot_res.Items.Add("ChatBot: " + response);

            // Add relevant technical information
            var relevantAnswers = answers.Where(a => a.Contains(topic)).ToList();
            if (relevantAnswers.Any())
            {
                foreach (string answer in relevantAnswers)
                {
                    chatbot_res.Items.Add("ChatBot: " + CapitalizeFirstLetter(answer));
                }
            }
        }

        private void ProcessTechnicalQuestion(string question)
        {
            // Process user input
            string[] words = question.Split(' ');
            List<string> filteredWords = words
                .Select(word => word.ToLower())
                .Where(word => !ignoreWords.Contains(word))
                .ToList();

            // Find matching answers
            List<string> matchedAnswers = answers
                .Where(answer => filteredWords.Any(word => answer.Contains(word)))
                .ToList();

            // Display results
            if (matchedAnswers.Count > 0)
            {
                Random rand = new Random();
                string positiveResponse = positiveResponses[rand.Next(positiveResponses.Count)];
                chatbot_res.Items.Add("ChatBot: " + positiveResponse);

                foreach (string answer in matchedAnswers)
                {
                    chatbot_res.Items.Add("ChatBot: " + CapitalizeFirstLetter(answer));
                }
                chatbot_res.Items.Add("ChatBot: Hope this response was helpful!");
            }
            else
            {
                chatbot_res.Items.Add("ChatBot: Couldn't quite get that. Ask about passwords, phishing, etc.");
            }

            // Scroll to the bottom of the chat
            chatbot_res.ScrollIntoView(chatbot_res.Items[chatbot_res.Items.Count - 1]);
        }

        private string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return char.ToUpper(input[0]) + input.Substring(1);
        }

        private void Reminders(object sender, RoutedEventArgs e)
        {
            Reminders_page.Visibility = Visibility.Hidden;
            Query_page.Visibility = Visibility.Visible;
            Activity_page.Visibility = Visibility.Hidden;
            chats_page.Visibility = Visibility.Hidden;
            Quiz_page.Visibility = Visibility.Hidden;
        }

        private void Quiz(object sender, RoutedEventArgs e)
        {
            Reminders_page.Visibility = Visibility.Hidden;
            Query_page.Visibility = Visibility.Hidden;
            Activity_page.Visibility = Visibility.Hidden;
            chats_page.Visibility = Visibility.Hidden;
            Quiz_page.Visibility = Visibility.Visible;
        }

        private void Activity(object sender, RoutedEventArgs e)
        {
            Reminders_page.Visibility = Visibility.Hidden;
            Query_page.Visibility = Visibility.Hidden;
            Activity_page.Visibility = Visibility.Visible;
            chats_page.Visibility = Visibility.Hidden;
            Quiz_page.Visibility = Visibility.Hidden;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Exit the application
            Application.Current.Shutdown();
        }

        private void submit_question(object sender, RoutedEventArgs e)
        {
            ProcessQuestion();
        }

        private void user_questions_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessQuestion();
                e.Handled = true; // Prevent the beep sound
            }
        }
    }
}