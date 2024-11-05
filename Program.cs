using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class ClaudeChatbot
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string API_URL = "https://api.anthropic.com/v1/messages";

    public ClaudeChatbot(string apiKey)
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
    }

    public async Task<string> GetResponse(string userMessage)
    {
        var message = new
        {
            role = "user",
            content = userMessage
        };

        var requestBody = new
        {
            model = "claude-3-sonnet-20240229",
            messages = new[] { message },
            max_tokens = 1024
        };

        var content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        try
        {
            var response = await _httpClient.PostAsync(API_URL, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return $"Erro na API: {responseContent}";
            }

            using JsonDocument document = JsonDocument.Parse(responseContent);
            return document.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString();
        }
        catch (Exception ex)
        {
            return $"Erro: {ex.Message}";
        }
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Iniciando chatbot com Claude...");
        
        string apiKey = "";
        var chatbot = new ClaudeChatbot(apiKey);

        while (true)
        {
            Console.Write("\nVocê: ");
            string input = Console.ReadLine();

            if (string.IsNullOrEmpty(input) || input.ToLower() == "sair")
                break;

            try
            {
                Console.Write("\nClaude: ");
                string response = await chatbot.GetResponse(input);
                Console.WriteLine(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
        }
    }
}