using System.Net;

namespace EmailCampaign.Application.Common.Services;

public class AnyService
{
    private readonly HttpClient _http;
    public AnyService(IHttpClientFactory factory) => _http = factory.CreateClient("default");

    public async Task<(bool ok, string? content, int status)> TryCallAsync()
    {
        using var resp = await _http.GetAsync("get");
        var body = await resp.Content.ReadAsStringAsync();
        return (resp.IsSuccessStatusCode, body, (int)resp.StatusCode);
    }
}
