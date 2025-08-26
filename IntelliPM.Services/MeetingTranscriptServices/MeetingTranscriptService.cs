using AutoMapper;
using IntelliPM.Data.DTOs.MeetingTranscript.Request;
using IntelliPM.Data.DTOs.MeetingTranscript.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.DynamicCategoryRepos;
using IntelliPM.Repositories.MeetingSummaryRepos;
using IntelliPM.Repositories.MeetingTranscriptRepos;
using IntelliPM.Services.GeminiServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Org.BouncyCastle.Math.EC.ECCurve;
using System.Security.Cryptography;
using IntelliPM.Data.DTOs.MeetingTranscript.Response;

namespace IntelliPM.Services.MeetingTranscriptServices
{
    public class MeetingTranscriptService : IMeetingTranscriptService
    {
        private readonly IMeetingTranscriptRepository _repo;
        private readonly IMeetingSummaryRepository _summaryRepo;
        private readonly IGeminiService _geminiService;
        private readonly IMapper _mapper;
        private readonly ILogger<MeetingTranscriptService> _logger;
        private readonly CloudConvertService _cloudConvertService;
        private readonly string _uploadPath;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _openAiApiKey;
        private readonly IConfiguration _config;
        private readonly IDynamicCategoryRepository _dynamicCategoryRepo;
        private readonly string _historyRoot;


        public MeetingTranscriptService(
            IMeetingTranscriptRepository repo,
            IMeetingSummaryRepository summaryRepo,
            IGeminiService geminiService,
            IMapper mapper,
            ILogger<MeetingTranscriptService> logger,
            IConfiguration config,
            IHttpClientFactory httpClientFactory,
            CloudConvertService cloudConvertService,
            IDynamicCategoryRepository dynamicCategoryRepo)

        {
            _repo = repo;
            _summaryRepo = summaryRepo;
            _geminiService = geminiService;
            _mapper = mapper;
            _logger = logger;
            _config = config;
            _uploadPath = Path.Combine(AppContext.BaseDirectory, config["UploadPath"]);
            _httpClientFactory = httpClientFactory;
            _cloudConvertService = cloudConvertService;
            _dynamicCategoryRepo = dynamicCategoryRepo;
            _historyRoot = Path.Combine(_uploadPath, "transcript_history");
            Directory.CreateDirectory(_historyRoot);

            //// Lấy API Key từ biến môi trường
            _openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")

                            ?? config["OpenAI:ApiKey"];

            if (string.IsNullOrWhiteSpace(_openAiApiKey))

            {
                throw new Exception("API Key không được thiết lập.");
            }
        }



        public async Task<MeetingTranscriptResponseDTO> UploadTranscriptFromUrlAsync(MeetingTranscriptFromUrlRequestDTO dto)
        {
            try
            {
                Directory.CreateDirectory(_uploadPath);
                string videoUrl = dto.VideoUrl;

                string mp3Path = Path.Combine(_uploadPath, $"{dto.MeetingId}.mp3");
                string wavPath = Path.ChangeExtension(mp3Path, ".wav");

                // Step 1: Convert Video to MP3 using CloudConvert
                var cloudConvertService = new CloudConvertService(_config); // hoặc tên biến config bạn đã khai báo
                                                                            // Hoặc inject qua constructor nếu đã tạo service
                await cloudConvertService.ConvertMp4ToMp3Async(videoUrl, mp3Path);

                // Step 2: Convert MP3 to WAV
                AudioConverter.ConvertMp3ToWav(mp3Path, wavPath);

                // Step 3: Whisper
                string transcript = await GenerateTranscriptAsync(wavPath);

                // Step 4: Cleanup
                try
                {
                    File.Delete(mp3Path);
                    File.Delete(wavPath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Temp file cleanup failed");
                }

                // Step 5: Save transcript
                var entity = new MeetingTranscript
                {
                    MeetingId = dto.MeetingId,
                    TranscriptText = transcript,
                    CreatedAt = DateTime.UtcNow
                };
                var saved = await _repo.AddAsync(entity);

                string summary;
                try
                {
                    summary = await _geminiService.SummarizeTextAsync(transcript);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to summarize.");
                    summary = "Cannot auto-generate summary.";
                }

                var summaryEntity = new MeetingSummary
                {
                    MeetingTranscriptId = saved.MeetingId,
                    SummaryText = summary,
                    CreatedAt = DateTime.UtcNow
                };
                await _summaryRepo.AddAsync(summaryEntity);

                await LogMeetingActionAsync(dto.MeetingId, "TRANSCRIPT_FROM_URL_CREATED");

                return _mapper.Map<MeetingTranscriptResponseDTO>(saved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UploadTranscriptFromUrlAsync failed: {Error}", ex.ToString());
                throw;
            }
        }

        // ADD helper methods
        private string GetHistoryDir(int meetingId) => Path.Combine(_historyRoot, meetingId.ToString());

        private static string Sha256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input ?? string.Empty);
            return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
        }

        private async Task<string> SaveSnapshotAsync(MeetingTranscript current, string? reason, int? editedBy)
        {
            var dir = GetHistoryDir(current.MeetingId);
            Directory.CreateDirectory(dir);

            var payload = new
            {
                MeetingId = current.MeetingId,
                TakenAtUtc = DateTime.UtcNow,
                TranscriptText = current.TranscriptText,
                Hash = Sha256(current.TranscriptText ?? string.Empty),
                CreatedAtOriginal = current.CreatedAt,
                EditReason = reason,
                EditedByAccountId = editedBy
            };

            var file = $"{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}_{payload.Hash}.json";
            var path = Path.Combine(dir, file);
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(path, json);
            return path;
        }

        private async Task<string?> ReadSnapshotTextAsync(string path)
        {
            if (!File.Exists(path)) return null;
            using var fs = File.OpenRead(path);
            using var doc = await JsonDocument.ParseAsync(fs);
            return doc.RootElement.GetProperty("TranscriptText").GetString();
        }

        public async Task<MeetingTranscriptResponseDTO> UploadTranscriptAsync(MeetingTranscriptRequestDTO dto)
        {
            try
            {
                Directory.CreateDirectory(_uploadPath);

                string mp3Path = Path.Combine(_uploadPath, $"{dto.MeetingId}.mp3");
                _logger.LogInformation("MP3 path: {Mp3Path}", mp3Path);

                await using (var fs = new FileStream(mp3Path, FileMode.Create))
                {
                    await dto.AudioFile.CopyToAsync(fs);
                }

                string wavPath = Path.ChangeExtension(mp3Path, ".wav");
                _logger.LogInformation("WAV path: {WavPath}", wavPath);

                AudioConverter.ConvertMp3ToWav(mp3Path, wavPath);

                // Call Whisper API
                string transcript = await GenerateTranscriptAsync(wavPath);

                _logger.LogInformation("Generated transcript: {Transcript}", transcript);

                if (string.IsNullOrWhiteSpace(transcript))
                {
                    _logger.LogWarning("Transcript is empty or null for meeting ID: {MeetingId}", dto.MeetingId);
                    transcript = "[Transcript could not be generated.]";
                }

                try
                {
                    if (File.Exists(mp3Path)) File.Delete(mp3Path);
                    if (File.Exists(wavPath)) File.Delete(wavPath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Cannot delete temp files.");
                }

                var entity = new MeetingTranscript
                {
                    MeetingId = dto.MeetingId,
                    TranscriptText = transcript,
                    CreatedAt = DateTime.UtcNow
                };
                var saved = await _repo.AddAsync(entity);

                string summary;
                try
                {
                    if (string.IsNullOrWhiteSpace(transcript) || transcript.Contains("Transcript could not be generated"))
                    {
                        summary = "Cannot auto-generate summary.";
                    }
                    else
                    {
                        summary = await _geminiService.SummarizeTextAsync(transcript);
                        _logger.LogInformation("Generated summary: {Summary}", summary);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to summarize.");
                    summary = "Cannot auto-generate summary.";
                }

                var summaryEntity = new MeetingSummary
                {
                    MeetingTranscriptId = saved.MeetingId,
                    SummaryText = summary,
                    CreatedAt = DateTime.UtcNow
                };
                await _summaryRepo.AddAsync(summaryEntity);

                await LogMeetingActionAsync(dto.MeetingId, "TRANSCRIPT_CREATED");

                return _mapper.Map<MeetingTranscriptResponseDTO>(saved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UploadTranscriptAsync failed: {Error}", ex.ToString());
                throw;
            }
        }

        // ADD: update + snapshot
        public async Task<MeetingTranscriptResponseDTO> UpdateTranscriptAsync(UpdateMeetingTranscriptRequestDTO dto)
        {
            var current = await _repo.GetByMeetingIdAsync(dto.MeetingId);
            if (current == null)
                throw new Exception($"No transcript found for meeting ID {dto.MeetingId}");

            if (!string.IsNullOrWhiteSpace(dto.IfMatchHash))
            {
                var serverHash = Sha256(current.TranscriptText);
                if (!serverHash.Equals(dto.IfMatchHash, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("409_CONFLICT: Transcript changed on server. Refresh and try again.");
            }

            await SaveSnapshotAsync(current, dto.EditReason, dto.EditedByAccountId); // snapshot before overwrite

            current.TranscriptText = dto.TranscriptText ?? string.Empty;
            await _repo.UpdateAsync(current);

            try
            {
                var summary = string.IsNullOrWhiteSpace(current.TranscriptText)
                    ? "Cannot auto-generate summary."
                    : await _geminiService.SummarizeTextAsync(current.TranscriptText);

                await _summaryRepo.AddAsync(new MeetingSummary
                {
                    MeetingTranscriptId = current.MeetingId,
                    SummaryText = summary,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to regenerate summary after update.");
            }

            await LogMeetingActionAsync(dto.MeetingId, "TRANSCRIPT_UPDATED_WITH_SNAPSHOT");
            return _mapper.Map<MeetingTranscriptResponseDTO>(current);
        }

        // ADD: list history
        public async Task<IEnumerable<TranscriptHistoryItemDTO>> GetTranscriptHistoryAsync(int meetingId)
        {
            var dir = GetHistoryDir(meetingId);
            if (!Directory.Exists(dir)) return Enumerable.Empty<TranscriptHistoryItemDTO>();

            var files = Directory.EnumerateFiles(dir, "*.json", SearchOption.TopDirectoryOnly)
                .Select(p => new TranscriptHistoryItemDTO
                {
                    FileName = Path.GetFileName(p),
                    TakenAtUtc = File.GetLastWriteTimeUtc(p) // hoặc GetCreationTimeUtc
                })
                .OrderByDescending(x => x.TakenAtUtc)
                .ToList();

            _logger.LogInformation("History {MeetingId}: {Count} snapshots @ {Dir}", meetingId, files.Count, dir);
            return await Task.FromResult(files);
        }
        // ADD: restore from a snapshot
        public async Task<MeetingTranscriptResponseDTO> RestoreTranscriptAsync(RestoreTranscriptRequestDTO dto)
        {
            var dir = GetHistoryDir(dto.MeetingId);
            var path = Path.Combine(dir, dto.FileName);
            var text = await ReadSnapshotTextAsync(path);
            if (text == null) throw new FileNotFoundException("Snapshot not found.");

            return await UpdateTranscriptAsync(new UpdateMeetingTranscriptRequestDTO
            {
                MeetingId = dto.MeetingId,
                TranscriptText = text,
                EditReason = $"restore:{dto.FileName}" + (string.IsNullOrWhiteSpace(dto.Reason) ? "" : $" - {dto.Reason}"),
                EditedByAccountId = dto.EditedByAccountId
            });
        }


        public async Task<MeetingTranscriptResponseDTO> GetTranscriptByMeetingIdAsync(int meetingId)
        {
            var transcript = await _repo.GetByMeetingIdAsync(meetingId);
            if (transcript == null)
                throw new Exception($"No transcript found for meeting ID {meetingId}");

            return _mapper.Map<MeetingTranscriptResponseDTO>(transcript);
        }

        private async Task<string> GenerateTranscriptAsync(string wavPath)
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromMinutes(5);

            using var form = new MultipartFormDataContent();
            await using var fileStream = File.OpenRead(wavPath);
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/wav");

            form.Add(fileContent, "file", Path.GetFileName(wavPath));
            form.Add(new StringContent("whisper-1"), "model");

            // Use API key from environment variable
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _openAiApiKey);

            _logger.LogInformation("Using OpenAI API Key: {ApiKey}", _openAiApiKey);

            var response = await httpClient.PostAsync("https://api.openai.com/v1/audio/transcriptions", form);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Whisper API raw response: {Response}", responseString);

            using var doc = JsonDocument.Parse(responseString);

            string text = doc.RootElement.GetProperty("text").GetString() ?? "";
            return text;
        }

        private Task LogMeetingActionAsync(int meetingId, string action)
        {
            _logger.LogInformation("Meeting {Id} action {Action}", meetingId, action);
            return Task.CompletedTask;
        }
    }



    public static class AudioConverter
    {
        public static void ConvertMp3ToWav(string mp3Path, string wavPath)
        {
            using var reader = new MediaFoundationReader(mp3Path);
            var outFormat = new WaveFormat(16000, 1);
            using var resampler = new MediaFoundationResampler(reader, outFormat) { ResamplerQuality = 60 };
            WaveFileWriter.CreateWaveFile(wavPath, resampler);
        }
    }
}