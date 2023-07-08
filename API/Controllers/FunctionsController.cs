using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("functions")]
public class FunctionsController : ControllerBase
{
    readonly HttpClient _httpClient;

    public FunctionsController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("FunctionsAPI");
    }

    [HttpPost("/file")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        Guid uploadId = Guid.NewGuid();

        var data = new MultipartFormDataContent()
        {
            { new StreamContent(file.OpenReadStream()), "file", file.FileName },
            { JsonContent.Create(new { uploadId }), "metadata" },
        };

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        _httpClient.PostAsync("api/Upload", data);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

        // Approximate time required for the HTTP client to invoke.
        // File size has no effect.
        await Task.Delay(TimeSpan.FromSeconds(2.5));

        return AcceptedAtAction(nameof(GetUploadStatus), new { id = uploadId }, null);
    }

    [HttpGet("/file/{id:guid}/status")]
    public async Task<IActionResult> GetUploadStatus([FromRoute] Guid id)
    {
        string response = await _httpClient.GetStringAsync($"api/Status?id={id}");

        return Ok(response);
    }
}
