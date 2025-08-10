using Vosk;
using NAudio.Wave;
using Serilog;
using System;
using System.IO;

namespace Audio
{
    public class VoskSpeechRecognizer : IDisposable
    {
        private Model? _model;
        private VoskRecognizer? _recognizer;
        private WaveInEvent? _waveIn;
        private string _latestTranscription = string.Empty;
        private DateTime _lastActivity = DateTime.UtcNow;
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _processingLock;
        private bool _disposed = false;
        private readonly int _sampleRate;
        private readonly string _modelPath;

        public event EventHandler<string>? TranscriptionReceived;
        public event EventHandler<string>? PartialTranscriptionReceived;

        public VoskSpeechRecognizer(string modelPath, int sampleRate = 16000, ILogger? logger = null)
        {
            _logger = logger ?? Log.ForContext<VoskSpeechRecognizer>();
            _sampleRate = sampleRate;
            _modelPath = modelPath;
            _processingLock = new SemaphoreSlim(1, 1);

            try
            {
                InitializeAudioSystem();
                _logger.Information("VOSK Speech Recognizer initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to initialize VOSK Speech Recognizer");
                throw;
            }
        }

        private void InitializeAudioSystem()
        {
            // Initialize VOSK model
            if (!Directory.Exists(_modelPath))
            {
                throw new DirectoryNotFoundException($"VOSK model directory not found: {_modelPath}");
            }

            _model = new Model(_modelPath);
            _recognizer = new VoskRecognizer(_model, _sampleRate);
            
            // Debug: List available audio devices
            _logger.Information("Available audio input devices:");
            for (int i = 0; i < WaveInEvent.DeviceCount; i++)
            {
                var capabilities = WaveInEvent.GetCapabilities(i);
                _logger.Information("Device {DeviceId}: {DeviceName} ({Channels} channels)", 
                    i, capabilities.ProductName, capabilities.Channels);
            }
            
            _waveIn = new WaveInEvent 
            { 
                WaveFormat = new WaveFormat(_sampleRate, 1),
                BufferMilliseconds = 100,
                NumberOfBuffers = 3
            };
            
            var selectedDevice = WaveInEvent.GetCapabilities(_waveIn.DeviceNumber);
            _logger.Information("Using audio device: {DeviceName}", selectedDevice.ProductName);
            _logger.Information("Sample rate: {SampleRate}Hz, Format: {WaveFormat}", _sampleRate, _waveIn.WaveFormat);
            
            _waveIn.DataAvailable += OnDataAvailable;
            _waveIn.RecordingStopped += OnRecordingStopped;
            
            StartRecording();
        }

        public void StartRecording()
        {
            if (_waveIn != null && !_disposed)
            {
                _waveIn.StartRecording();
                _logger.Information("🎤 Recording started - speak now!");
            }
        }

        public void StopRecording()
        {
            if (_waveIn != null)
            {
                _waveIn.StopRecording();
                _logger.Information("🛑 Recording stopped");
            }
        }

        private async void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            if (_disposed) return;

            await _processingLock.WaitAsync();
            try
            {
                _lastActivity = DateTime.UtcNow;
                
                // Calculate audio level
                var audioLevel = CalculateAudioLevel(e.Buffer, e.BytesRecorded);
                if (audioLevel > 0.01) // Only log significant audio
                {
                    _logger.Debug("🔊 Audio detected - Level: {AudioLevel:F3}", audioLevel);
                }
                
                if (_recognizer?.AcceptWaveform(e.Buffer, e.BytesRecorded) == true)
                {
                    var result = _recognizer.Result();
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        _logger.Information("🎤 Transcription: {Result}", result);
                        _latestTranscription = result;
                        TranscriptionReceived?.Invoke(this, result);
                    }
                }
                else
                {
                    var partialResult = _recognizer?.PartialResult();
                    if (!string.IsNullOrEmpty(partialResult) && partialResult != "{\"partial\":\"\"}")
                    {
                        _logger.Debug("🎤 Partial: {PartialResult}", partialResult);
                        PartialTranscriptionReceived?.Invoke(this, partialResult);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error processing audio data");
            }
            finally
            {
                _processingLock.Release();
            }
        }
        
        private void OnRecordingStopped(object? sender, StoppedEventArgs e)
        {
            if (e.Exception != null)
            {
                _logger.Error(e.Exception, "Recording error occurred");
            }
            else
            {
                _logger.Information("🛑 Recording stopped");
            }
        }
        
        private double CalculateAudioLevel(byte[] buffer, int bytesRecorded)
        {
            if (bytesRecorded == 0) return 0;

            double sum = 0;
            for (int i = 0; i < bytesRecorded; i += 2)
            {
                if (i + 1 < bytesRecorded)
                {
                    short sample = (short)((buffer[i + 1] << 8) | buffer[i]);
                    sum += sample * sample;
                }
            }
            return Math.Sqrt(sum / (bytesRecorded / 2)) / 32768.0;
        }

        public string GetLatestTranscription() => _latestTranscription;
        
        public bool IsActive => (DateTime.UtcNow - _lastActivity).TotalSeconds < 5;

        public void Dispose()
        {
            if (!_disposed)
            {
                StopRecording();
                
                _waveIn?.Dispose();
                _recognizer?.Dispose();
                _model?.Dispose();
                _processingLock?.Dispose();
                
                _disposed = true;
                _logger.Information("VOSK Speech Recognizer disposed");
            }
        }
    }
}
