using CloudConvert.API;
using CloudConvert.API.Models.JobModels;
using CloudConvert.API.Models.ImportOperations;
using CloudConvert.API.Models.ExportOperations;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

namespace IntelliPM.Services.MeetingTranscriptServices
{
    public class CloudConvertService
    {
        private readonly CloudConvertAPI _api;

        public CloudConvertService(IConfiguration config)
        {
            var apiKey = config["CloudConvert:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("CloudConvert API key is missing in configuration.");

            _api = new CloudConvertAPI(apiKey);
        }

        public async Task ConvertMp4ToMp3Async(string videoUrl, string outputPath)
        {
            var job = await _api.CreateJobAsync(new JobCreateRequest
            {
                Tasks = new Dictionary<string, object>
                {
                    ["import"] = new ImportUrlCreateRequest
                    {
                        Url = videoUrl
                    },
                    ["convert"] = new Dictionary<string, object>
                    {
                        { "operation", "convert" },
                        { "input", "import" },
                        { "output_format", "mp3" },
                        { "engine", "ffmpeg" }
                    },
                    ["export"] = new ExportUrlCreateRequest
                    {
                        Input = "convert"
                    }
                }
            });

            var completedJob = await _api.WaitJobAsync(job.Data.Id);

            var exportTask = completedJob.Data.Tasks
                .FirstOrDefault(t => t.Name == "export" && t.Result?.Files != null);

            if (exportTask?.Result?.Files == null || !exportTask.Result.Files.Any())
                throw new Exception("Export failed or no file returned.");

            var fileUrl = exportTask.Result.Files.First().Url;

            using var httpClient = new HttpClient();
            var fileBytes = await httpClient.GetByteArrayAsync(fileUrl);
            await File.WriteAllBytesAsync(outputPath, fileBytes);
        }
    }
}
