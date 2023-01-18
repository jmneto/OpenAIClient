// jmneto OpenAi Client
// Jan 2023 - Version 1.0

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;


namespace OpenAIClient
{
    public partial class MainWindow : Window
    {
        string context = string.Empty;

        public MainWindow()
        {
            InitializeComponent();

            // Center form on Screen
            Window currentWindow = Application.Current.MainWindow;
            Size windowSize = new Size(currentWindow.Width, currentWindow.Height);
            Rect screenSize = SystemParameters.WorkArea;
            currentWindow.Left = (screenSize.Width / 2) - (windowSize.Width / 2);
            currentWindow.Top = (screenSize.Height / 2) - (windowSize.Height / 2);

            // Set focus to promt field
            txtPrompt.Focus();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            // Trim Prompt
            string prompt = txtPrompt.Text.Trim('\n', '\r', ' ');

            // If prompt is empty reset and return
            if (prompt == string.Empty)
            {
                // Reset Completion
                txtCompletion.Text = "";
                lblCompletion.Content = "Completion context cleared";
                context = string.Empty;
                return;
            }

            // Setup parameters for request
            string apiEndpoint = "https://api.openai.com/v1/completions";
            string apikey = txtAPIKey.Text;
            context = String.Format("{0}User:{1}\n", context, prompt);
            var jsonRequest = new
            {
                model = txtModel.Text,
                prompt = context,
                max_tokens = int.Parse((txtMaxTokens.Text).ToString()),
                temperature = float.Parse(txtTemperature.Text.ToString())
            };

            // Call the REST API on a separate thread from UI
            Task.Run(() =>
            {
                // Send the request to the API
                using (var client = new HttpClient())
                {
                    // Post Request
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apikey}");
                    var response = client.PostAsync(apiEndpoint, new StringContent(JsonSerializer.Serialize(jsonRequest), Encoding.UTF8, "application/json")).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        // Get the JSON response
                        var jsonResponse = response.Content.ReadAsStringAsync().Result;

                        // Deserialize
                        var responseObject = JsonSerializer.Deserialize<GPT3Response>(jsonResponse);

                        // Print Completion or Raw Error JSON
                        if (responseObject != null)
                            if (responseObject.choices != null)
                                Dispatcher.Invoke(() =>
                                {
                                    lblCompletion.Content = String.Format("Completion context saved. Tokens used: {0}", responseObject.usage.total_tokens);
                                    txtCompletion.Text = responseObject.choices[0].text.TrimStart('\n').Replace("\n", Environment.NewLine);
                                    context += String.Format("{0}\n", txtCompletion.Text);
                                });
                            else
                                Dispatcher.Invoke(() =>
                                {
                                    txtCompletion.Text = jsonResponse.Replace("\n", Environment.NewLine);
                                });
                    }
                    else
                    {
                        lblCompletion.Content = "The request was unsuccessful. Status code: " + (int)response.StatusCode;
                        txtCompletion.Text = "";
                    }
                }
            });

            // Wait message (this happens first since we are posting the request on another thread)
            lblCompletion.Content = "Request is processing...";
            txtCompletion.Text = "";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Init/Load From Registry
            txtAPIKey.Text = RegistryHelper.ReadAppInfo("APIKEY");
            if (txtAPIKey.Text == string.Empty)
                txtAPIKey.Text = "Your OpenAI API Key here";

            txtModel.Text = RegistryHelper.ReadAppInfo("MODEL");
            if (txtModel.Text == string.Empty)
                txtModel.Text = "text-davinci-003";

            txtTemperature.Text = RegistryHelper.ReadAppInfo("TEMPERATURE");
            if (txtTemperature.Text == string.Empty)
                txtTemperature.Text = "0.5";

            txtMaxTokens.Text = RegistryHelper.ReadAppInfo("MAXTOKENS");
            if (txtMaxTokens.Text == string.Empty)
                txtMaxTokens.Text = "2048";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Save to Registry
            RegistryHelper.WriteAppInfo("APIKEY", txtAPIKey.Text);
            RegistryHelper.WriteAppInfo("MODEL", txtModel.Text);
            RegistryHelper.WriteAppInfo("TEMPERATURE", txtTemperature.Text);
            RegistryHelper.WriteAppInfo("MAXTOKENS", txtMaxTokens.Text);
        }
    }

    /*
    Registry Helper
    You can write information to the registry by calling the WriteAppInfo method and passing in a key and a value. 
    You can read that information back from the registry by calling the ReadAppInfo method and passing in the key. 
    */
    internal static class RegistryHelper
    {
        private const string AppKey = "SOFTWARE\\OpenAIClient";

        public static void WriteAppInfo(string key, string value)
        {
            using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(AppKey))
            {
                registryKey.SetValue(key, value);
            }
        }
        public static string? ReadAppInfo(string key)
        {
            using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(AppKey))
            {
                if (registryKey == null) return null;
                return (string)registryKey.GetValue(key);
            }
        }
    }

    /*
    This class defines the structure of the JSON response from the GPT-3 API, 
    with properties for the choices, id, log_probs, and stop_token fields. 
    The choices field is a list of Choice objects, which have properties for text, 
    log_probs, and stop_token. This class can be used to deserialize the JSON response 
    from the GPT-3 API and access the data in a strongly-typed way.
    */
    internal class GPT3Response
    {
        public string id { get; set; }
        public string @object { get; set; }
        public int created { get; set; }
        public string model { get; set; }
        public List<Choice> choices { get; set; }
        public Usage usage { get; set; }
    }
    internal class Choice
    {
        public string text { get; set; }
        public int index { get; set; }
        public object logprobs { get; set; }
        public string finish_reason { get; set; }
    }
    internal class Usage
    {
        public int prompt_tokens { get; set; }
        public int completion_tokens { get; set; }
        public int total_tokens { get; set; }
    }
}
