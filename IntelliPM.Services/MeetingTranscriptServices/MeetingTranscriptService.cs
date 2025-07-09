
//using AutoMapper;
//using IntelliPM.Data.DTOs.MeetingTranscript.Request;
//using IntelliPM.Data.DTOs.MeetingTranscript.Response;
//using IntelliPM.Data.Entities;
//using IntelliPM.Repositories.MeetingTranscriptRepos;
//using IntelliPM.Repositories.MeetingSummaryRepos;
//using IntelliPM.Services.GeminiServices;
//using Microsoft.Extensions.Logging;
//using NAudio.Wave;
//using System;
//using System.IO;
//using System.Threading.Tasks;
//using Vosk;

//namespace IntelliPM.Services.MeetingTranscriptServices
//{
//    public class MeetingTranscriptService : IMeetingTranscriptService
//    {
//        private readonly IMeetingTranscriptRepository _repo;
//        private readonly IMeetingSummaryRepository _summaryRepo;
//        private readonly IGeminiService _geminiService;
//        private readonly IMapper _mapper;
//        private readonly ILogger<MeetingTranscriptService> _logger;

//        // Đường dẫn mô hình Vosk được đặt trong thư mục gốc của dự án
//        private readonly string _voskModelPath = Path.Combine(Directory.GetCurrentDirectory(), "VoskModels", "vosk-model-small-en-us-0.15");

//        public MeetingTranscriptService(
//            IMeetingTranscriptRepository repo,
//            IMeetingSummaryRepository summaryRepo,
//            IGeminiService geminiService,
//            IMapper mapper,
//            ILogger<MeetingTranscriptService> logger)
//        {
//            _repo = repo;
//            _summaryRepo = summaryRepo;
//            _geminiService = geminiService;
//            _mapper = mapper;
//            _logger = logger;
//        }

//        //public async Task<MeetingTranscriptResponseDTO> UploadTranscriptAsync(MeetingTranscriptRequestDTO dto)
//        //{
//        //    try
//        //    {
//        //        // 1. Save MP3
//        //        string uploadDir = Path.Combine("uploads");
//        //        Directory.CreateDirectory(uploadDir);
//        //        string mp3Path = Path.Combine(uploadDir, $"{dto.MeetingId}.mp3");
//        //        await using (var fs = new FileStream(mp3Path, FileMode.Create))
//        //        {
//        //            await dto.AudioFile.CopyToAsync(fs);
//        //        }

//        //        // 2. Convert MP3 to WAV
//        //        string wavPath = Path.ChangeExtension(mp3Path, ".wav");
//        //        AudioConverter.ConvertMp3ToWav(mp3Path, wavPath);

//        //        // 3. Generate transcript using Vosk
//        //        string transcript = await GenerateTranscriptAsync(wavPath);

//        //        // 4. Save transcript to DB
//        //        var entity = new MeetingTranscript
//        //        {
//        //            MeetingId = dto.MeetingId,
//        //            TranscriptText = transcript,
//        //            CreatedAt = DateTime.UtcNow
//        //        };
//        //        var saved = await _repo.AddAsync(entity);

//        //        // 5. Summarize transcript using Gemini (GPT)
//        //        string summary = await _geminiService.SummarizeTextAsync(transcript);

//        //        // 6. Save summary to DB
//        //        var summaryEntity = new MeetingSummary
//        //        {
//        //            MeetingTranscriptId = saved.MeetingId,
//        //            SummaryText = summary,
//        //            CreatedAt = DateTime.UtcNow
//        //        };
//        //        await _summaryRepo.AddAsync(summaryEntity);

//        //        // 7. Log
//        //        await LogMeetingActionAsync(dto.MeetingId, "TRANSCRIPT_CREATED");

//        //        return _mapper.Map<MeetingTranscriptResponseDTO>(saved);
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        _logger.LogError(ex, "UploadTranscriptAsync failed");
//        //        throw;
//        //    }
//        //}
//        //public async Task<MeetingTranscriptResponseDTO> UploadTranscriptAsync(MeetingTranscriptRequestDTO dto)
//        //{
//        //    try
//        //    {
//        //        // 1. Save MP3
//        //        string uploadDir = Path.Combine("uploads");
//        //        Directory.CreateDirectory(uploadDir);
//        //        string mp3Path = Path.Combine(uploadDir, $"{dto.MeetingId}.mp3");
//        //        await using (var fs = new FileStream(mp3Path, FileMode.Create))
//        //        {
//        //            await dto.AudioFile.CopyToAsync(fs);
//        //        }

//        //        // 2. Convert MP3 to WAV
//        //        string wavPath = Path.ChangeExtension(mp3Path, ".wav");
//        //        AudioConverter.ConvertMp3ToWav(mp3Path, wavPath);

//        //        // 3. Generate transcript using Vosk
//        //        string transcript = await GenerateTranscriptAsync(wavPath);

//        //        // XÓA FILE SAU KHI XỬ LÝ
//        //        try
//        //        {
//        //            if (File.Exists(mp3Path))
//        //                File.Delete(mp3Path);
//        //            if (File.Exists(wavPath))
//        //                File.Delete(wavPath);
//        //        }
//        //        catch (Exception ex)
//        //        {
//        //            _logger.LogWarning(ex, "Không thể xóa file tạm sau khi xử lý.");
//        //        }

//        //        // 4. Save transcript to DB
//        //        var entity = new MeetingTranscript
//        //        {
//        //            MeetingId = dto.MeetingId,
//        //            TranscriptText = transcript,
//        //            CreatedAt = DateTime.UtcNow
//        //        };
//        //        var saved = await _repo.AddAsync(entity);

//        //        // 5. Summarize transcript using Gemini (GPT)
//        //        string summary = await _geminiService.SummarizeTextAsync(transcript);

//        //        // 6. Save summary to DB
//        //        var summaryEntity = new MeetingSummary
//        //        {
//        //            MeetingTranscriptId = saved.MeetingId,
//        //            SummaryText = summary,
//        //            CreatedAt = DateTime.UtcNow
//        //        };
//        //        await _summaryRepo.AddAsync(summaryEntity);

//        //        // 7. Log
//        //        await LogMeetingActionAsync(dto.MeetingId, "TRANSCRIPT_CREATED");

//        //        return _mapper.Map<MeetingTranscriptResponseDTO>(saved);
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        _logger.LogError(ex, "UploadTranscriptAsync failed");
//        //        throw;
//        //    }
//        //}

//        public async Task<MeetingTranscriptResponseDTO> UploadTranscriptAsync(MeetingTranscriptRequestDTO dto)
//        {
//            try
//            {
//                // 1. Save MP3
//                string uploadDir = Path.Combine("uploads");
//                string absUploadDir = Path.GetFullPath(uploadDir);
//                _logger.LogInformation("Upload directory (absolute): {AbsUploadDir}", absUploadDir);

//                try
//                {
//                    Directory.CreateDirectory(uploadDir);
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogError(ex, "Không thể tạo thư mục uploads: {AbsUploadDir}", absUploadDir);
//                    throw;
//                }

//                string mp3Path = Path.Combine(uploadDir, $"{dto.MeetingId}.mp3");
//                _logger.LogInformation("MP3 path: {Mp3Path}", mp3Path);

//                try
//                {
//                    await using (var fs = new FileStream(mp3Path, FileMode.Create))
//                    {
//                        await dto.AudioFile.CopyToAsync(fs);
//                    }
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogError(ex, "Không thể ghi file MP3: {Mp3Path}", mp3Path);
//                    throw;
//                }

//                // 2. Convert MP3 to WAV
//                string wavPath = Path.ChangeExtension(mp3Path, ".wav");
//                _logger.LogInformation("WAV path: {WavPath}", wavPath);

//                try
//                {
//                    AudioConverter.ConvertMp3ToWav(mp3Path, wavPath);
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogError(ex, "Không thể convert MP3 sang WAV: {Mp3Path} -> {WavPath}", mp3Path, wavPath);
//                    throw;
//                }

//                // 3. Generate transcript using Vosk
//                _logger.LogInformation("Vosk model path: {VoskModelPath}", _voskModelPath);
//                if (!Directory.Exists(_voskModelPath))
//                {
//                    _logger.LogError("Vosk model directory does not exist: {VoskModelPath}", _voskModelPath);
//                    throw new DirectoryNotFoundException($"Vosk model directory does not exist: {_voskModelPath}");
//                }

//                string transcript;
//                try
//                {
//                    transcript = await GenerateTranscriptAsync(wavPath);
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogError(ex, "Không thể tạo transcript từ WAV: {WavPath}", wavPath);
//                    throw;
//                }

//                // XÓA FILE SAU KHI XỬ LÝ
//                try
//                {
//                    if (File.Exists(mp3Path))
//                        File.Delete(mp3Path);
//                    if (File.Exists(wavPath))
//                        File.Delete(wavPath);
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogWarning(ex, "Không thể xóa file tạm sau khi xử lý.");
//                }

//                // 4. Save transcript to DB
//                var entity = new MeetingTranscript
//                {
//                    MeetingId = dto.MeetingId,
//                    TranscriptText = transcript,
//                    CreatedAt = DateTime.UtcNow
//                };
//                var saved = await _repo.AddAsync(entity);

//                // 5. Summarize transcript using Gemini (GPT)
//                string summary;
//                try
//                {
//                    summary = await _geminiService.SummarizeTextAsync(transcript);
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogError(ex, "Không thể tạo summary từ transcript.");
//                    summary = "Không thể tạo summary tự động.";
//                }

//                // 6. Save summary to DB
//                var summaryEntity = new MeetingSummary
//                {
//                    MeetingTranscriptId = saved.MeetingId,
//                    SummaryText = summary,
//                    CreatedAt = DateTime.UtcNow
//                };
//                await _summaryRepo.AddAsync(summaryEntity);

//                // 7. Log
//                await LogMeetingActionAsync(dto.MeetingId, "TRANSCRIPT_CREATED");

//                return _mapper.Map<MeetingTranscriptResponseDTO>(saved);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "UploadTranscriptAsync failed. Chi tiết lỗi: {Error}", ex.ToString());
//                throw;
//            }
//        }




//        // Sửa lại hàm này để truyền path model động
//        private async Task<string> GenerateTranscriptAsync(string wavPath, string voskModelPath)
//        {
//            _logger.LogInformation("Vosk model path: " + voskModelPath);

//            if (!Directory.Exists(voskModelPath))
//            {
//                _logger.LogError($"Model path not found: {voskModelPath}");
//                throw new DirectoryNotFoundException($"Model path not found: {voskModelPath}");
//            }

//            Vosk.Vosk.SetLogLevel(0);
//            using var model = new Model(voskModelPath);
//            using var rec = new VoskRecognizer(model, 16000.0f);

//            byte[] pcm = await File.ReadAllBytesAsync(wavPath);
//            rec.AcceptWaveform(pcm, pcm.Length);
//            return rec.FinalResult();
//        }
//        public async Task<MeetingTranscriptResponseDTO> GetTranscriptByMeetingIdAsync(int meetingId)
//        {
//            var transcript = await _repo.GetByMeetingIdAsync(meetingId);

//            if (transcript == null)
//            {
//                throw new Exception($"No transcript found for meeting ID {meetingId}");
//            }

//            return _mapper.Map<MeetingTranscriptResponseDTO>(transcript);
//        }

//        private async Task<string> GenerateTranscriptAsync(string wavPath)
//        {
//            _logger.LogInformation("Vosk model path: " + _voskModelPath);

//            if (!Directory.Exists(_voskModelPath))
//            {
//                _logger.LogError($"Model path not found: {_voskModelPath}");
//                throw new DirectoryNotFoundException($"Model path not found: {_voskModelPath}");
//            }

//            Vosk.Vosk.SetLogLevel(0);
//            using var model = new Model(_voskModelPath);
//            using var rec = new VoskRecognizer(model, 16000.0f);

//            byte[] pcm = await File.ReadAllBytesAsync(wavPath);
//            rec.AcceptWaveform(pcm, pcm.Length);
//            return rec.FinalResult();
//        }

//        private Task LogMeetingActionAsync(int meetingId, string action)
//        {
//            _logger.LogInformation("Meeting {Id} action {Action}", meetingId, action);
//            return Task.CompletedTask;
//        }
//    }

//    public static class AudioConverter
//    {
//        // Convert MP3 to WAV with specific settings (16kHz, mono)
//        public static void ConvertMp3ToWav(string mp3Path, string wavPath)
//        {
//            using var reader = new MediaFoundationReader(mp3Path);
//            var outFormat = new WaveFormat(16000, 1);
//            using var resampler = new MediaFoundationResampler(reader, outFormat) { ResamplerQuality = 60 };
//            WaveFileWriter.CreateWaveFile(wavPath, resampler);
//        }
//    }
//}
/*
using AutoMapper;
using IntelliPM.Data.DTOs.MeetingTranscript.Request;
using IntelliPM.Data.DTOs.MeetingTranscript.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.MeetingTranscriptRepos;
using IntelliPM.Repositories.MeetingSummaryRepos;
using IntelliPM.Services.GeminiServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Vosk;

namespace IntelliPM.Services.MeetingTranscriptServices
{
    public class MeetingTranscriptService : IMeetingTranscriptService
    {
        private readonly IMeetingTranscriptRepository _repo;
        private readonly IMeetingSummaryRepository _summaryRepo;
        private readonly IGeminiService _geminiService;
        private readonly IMapper _mapper;
        private readonly ILogger<MeetingTranscriptService> _logger;

        private readonly string _voskModelPath;
        private readonly string _uploadPath;

        public MeetingTranscriptService(
            IMeetingTranscriptRepository repo,
            IMeetingSummaryRepository summaryRepo,
            IGeminiService geminiService,
            IMapper mapper,
            ILogger<MeetingTranscriptService> logger,
            IConfiguration config)
        {
            _repo = repo;
            _summaryRepo = summaryRepo;
            _geminiService = geminiService;
            _mapper = mapper;
            _logger = logger;

            _voskModelPath = Path.Combine(AppContext.BaseDirectory, config["VoskModelPath"]);
            _uploadPath = Path.Combine(AppContext.BaseDirectory, config["UploadPath"]);
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

                string transcript = await GenerateTranscriptAsync(wavPath);

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

                await LogMeetingActionAsync(dto.MeetingId, "TRANSCRIPT_CREATED");

                return _mapper.Map<MeetingTranscriptResponseDTO>(saved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UploadTranscriptAsync failed: {Error}", ex.ToString());
                throw;
            }
        }

        private async Task<string> GenerateTranscriptAsync(string wavPath)
        {
            if (!Directory.Exists(_voskModelPath))
            {
                _logger.LogError("Vosk model path not found: {Path}", _voskModelPath);
                throw new DirectoryNotFoundException($"Vosk model path not found: {_voskModelPath}");
            }

            Vosk.Vosk.SetLogLevel(0);
            using var model = new Model(_voskModelPath);
            using var rec = new VoskRecognizer(model, 16000.0f);

            byte[] pcm = await File.ReadAllBytesAsync(wavPath);
            rec.AcceptWaveform(pcm, pcm.Length);
            return rec.FinalResult();
        }

        public async Task<MeetingTranscriptResponseDTO> GetTranscriptByMeetingIdAsync(int meetingId)
        {
            var transcript = await _repo.GetByMeetingIdAsync(meetingId);
            if (transcript == null)
                throw new Exception($"No transcript found for meeting ID {meetingId}");

            return _mapper.Map<MeetingTranscriptResponseDTO>(transcript);
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

        // Nếu deploy Linux thì dùng FFMpeg:
        // public static void ConvertMp3ToWav(string mp3Path, string wavPath)
        // {
        //     var psi = new ProcessStartInfo
        //     {
        //         FileName = "ffmpeg",
        //         Arguments = $"-i \"{mp3Path}\" -ar 16000 -ac 1 \"{wavPath}\"",
        //         RedirectStandardOutput = true,
        //         RedirectStandardError = true,
        //         UseShellExecute = false,
        //         CreateNoWindow = true
        //     };
        //     using var process = Process.Start(psi);
        //     process.WaitForExit();
        //     if (process.ExitCode != 0)
        //     {
        //         throw new Exception("FFMpeg conversion failed.");
        //     }
        // }
    }
}
*/