using AutoMapper;
using IntelliPM.Data.DTOs.MeetingLog.Request;
using IntelliPM.Data.DTOs.MeetingTranscript.Request;
using IntelliPM.Data.DTOs.MeetingTranscript.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.MeetingTranscriptRepos;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using System;
using System.IO;
using System.Threading.Tasks;
using Vosk;

namespace IntelliPM.Services.MeetingTranscriptServices
{
    public class MeetingTranscriptService : IMeetingTranscriptService
    {
        private readonly IMeetingTranscriptRepository _repo;
        private readonly IMapper _mapper;
        private readonly ILogger<MeetingTranscriptService> _logger;
        private readonly string _voskModelPath = @"E:\DOAN\Vosk\vosk-model-small-en-us-0.15";

        public MeetingTranscriptService(
            IMeetingTranscriptRepository repo,
            IMapper mapper,
            ILogger<MeetingTranscriptService> logger)
        {
            _repo = repo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<MeetingTranscriptResponseDTO> UploadTranscriptAsync(MeetingTranscriptRequestDTO dto)
        {
            try
            {
                // 1. Save MP3
                string uploadDir = Path.Combine("uploads");
                Directory.CreateDirectory(uploadDir);
                string mp3Path = Path.Combine(uploadDir, $"{dto.MeetingId}.mp3");
                await using (var fs = new FileStream(mp3Path, FileMode.Create))
                {
                    await dto.AudioFile.CopyToAsync(fs);
                }

                // 2. Convert MP3 to WAV
                string wavPath = Path.ChangeExtension(mp3Path, ".wav");
                AudioConverter.ConvertMp3ToWav(mp3Path, wavPath);

                // 3. Generate transcript using Vosk
                string transcript = await GenerateTranscriptAsync(wavPath);

                // 4. Save to DB
                var entity = new MeetingTranscript
                {
                    MeetingId = dto.MeetingId,
                    TranscriptText = transcript,
                    CreatedAt = DateTime.UtcNow
                };
                var saved = await _repo.AddAsync(entity);

                // 5. Log
                await LogMeetingActionAsync(dto.MeetingId, "TRANSCRIPT_CREATED");

                return _mapper.Map<MeetingTranscriptResponseDTO>(saved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UploadTranscriptAsync failed");
                throw; // Sẽ trả lỗi 500
            }
        }

        public async Task<MeetingTranscriptResponseDTO> GetTranscriptByMeetingIdAsync(int meetingId)
        {
            var transcript = await _repo.GetByMeetingIdAsync(meetingId);

            if (transcript == null)
            {
                throw new Exception($"No transcript found for meeting ID {meetingId}");
            }

            return _mapper.Map<MeetingTranscriptResponseDTO>(transcript);
        }

        private async Task<string> GenerateTranscriptAsync(string wavPath)
        {
            if (!Directory.Exists(_voskModelPath))
                throw new DirectoryNotFoundException($"Model path not found: {_voskModelPath}");

            Vosk.Vosk.SetLogLevel(0);
            using var model = new Model(_voskModelPath);
            using var rec = new VoskRecognizer(model, 16000.0f);

            byte[] pcm = await File.ReadAllBytesAsync(wavPath);
            rec.AcceptWaveform(pcm, pcm.Length);
            return rec.FinalResult();
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
            var outFormat = new WaveFormat(16000, 1); // 16kHz, mono
            using var resampler = new MediaFoundationResampler(reader, outFormat) { ResamplerQuality = 60 };
            WaveFileWriter.CreateWaveFile(wavPath, resampler);
        }
    }
}
