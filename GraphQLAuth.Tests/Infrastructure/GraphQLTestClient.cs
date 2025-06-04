using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GraphQLAuth.Tests.Infrastructure;

public class GraphQLTestClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public GraphQLTestClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    public void SetAuthorizationToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearAuthorizationToken()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<GraphQLResponse<T>> QueryAsync<T>(string query, object? variables = null)
    {
        var request = new GraphQLRequest
        {
            Query = query,
            Variables = variables
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync("/graphql", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<GraphQLResponse<T>>(responseContent, _jsonOptions);
        return result ?? new GraphQLResponse<T>();
    }
}

public class GraphQLRequest
{
    public string Query { get; set; } = string.Empty;
    public object? Variables { get; set; }
}

public class GraphQLResponse<T>
{
    public T? Data { get; set; }
    public List<GraphQLError>? Errors { get; set; }
}

public class GraphQLError
{
    public string Message { get; set; } = string.Empty;
    public List<Location>? Locations { get; set; }
    public List<object>? Path { get; set; }
    public Dictionary<string, object>? Extensions { get; set; }
}

public class Location
{
    public int Line { get; set; }
    public int Column { get; set; }
}