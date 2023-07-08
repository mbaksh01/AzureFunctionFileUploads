using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Function;

public static class Function1
{
    private class Metadata
    {
        [JsonPropertyName("uploadId")]
        public Guid UploadId { get; set; }
    }

    enum UploadStatus
    {
        Complete,
        Failed,
        NotStarted,
        Pending,
    }

    static readonly Dictionary<Guid, UploadStatus> _dictionary = new();

    [FunctionName("Upload")]
    public static async Task<IActionResult> Upload(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log,
        CancellationToken cancellationToken)
    {
        try
        {
            IFormCollection forms = await req.ReadFormAsync(cancellationToken);

            var meta = forms.FirstOrDefault(t => t.Key == "metadata").Value;

            Metadata metadata = JsonConvert.DeserializeObject<Metadata>(meta);

            if (metadata.UploadId == Guid.Empty)
            {
                return new BadRequestObjectResult("Upload id invalid or not present.");
            }

            _dictionary.Add(metadata.UploadId, UploadStatus.NotStarted);

            if (forms.Files.Count > 0)
            {
                log.LogInformation("File present.");

                _dictionary[metadata.UploadId] = UploadStatus.Pending;

                for (int i = 1; i <= 30; i++)
                {
                    log.LogInformation("{} / 30 - Waiting 1000ms", i);
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }

                _dictionary[metadata.UploadId] = UploadStatus.Complete;
            }
            else
            {
                log.LogInformation("No files present.");

                _dictionary[metadata.UploadId] = UploadStatus.Failed;
            }

            return new OkObjectResult($"Finished Upload Job. Upload Status: {_dictionary[metadata.UploadId]}");
        }
        catch (Exception ex)
        {
            return new ResponseMessageResult(new()
            {
                StatusCode = HttpStatusCode.InternalServerError,
                ReasonPhrase = ex.Message,
            });
        }
    }

    [FunctionName("Status")]
    public static Task<IActionResult> Status(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req
    )
    {
        return Task.Run<IActionResult>(() =>
        {
            if (!req.Query.TryGetValue("id", out var value))
            {
                return new BadRequestObjectResult("Expected Id in query params.");
            }

            if (!Guid.TryParse(value, out Guid uploadId))
            {
                return new BadRequestObjectResult($"Expected upload id to be a GUID.");
            }

            if (_dictionary.TryGetValue(uploadId, out UploadStatus status))
            {
                string statusAsString = status switch
                { 
                    UploadStatus.Pending => "Pending",
                    UploadStatus.NotStarted => "Not Started",
                    UploadStatus.Failed => "Failed",
                    UploadStatus.Complete => "Complete",
                    _ => "Unknown",
                };

                return new OkObjectResult($"Status: {statusAsString}");
            }

            return new BadRequestObjectResult($"Finished Status Job. Status: Unknown");
        });
    }
}
