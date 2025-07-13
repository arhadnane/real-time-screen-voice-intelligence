using Vosk;
using NAudio.Wave;

namespace Audio
{
    public class VoskSpeechRecognizer
    {
        private Model? _model;
        private VoskRecognizer? _recognizer;
        private WaveInEvent? _waveIn;
        private string _latestTranscription = string.Empty;

        public VoskSpeechRecognizer(string modelPath, int sampleRate = 16000)
        {
            _model = new Model(modelPath);
            _recognizer = new VoskRecognizer(_model, sampleRate);
            _waveIn = new WaveInEvent { WaveFormat = new WaveFormat(sampleRate, 1) };
            _waveIn.DataAvailable += OnDataAvailable;
            _waveIn.StartRecording();
        }

        private void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            if (_recognizer!.AcceptWaveform(e.Buffer, e.BytesRecorded))
            {
                var result = _recognizer.Result();
                _latestTranscription = result;
            }
        }

        public string GetLatestTranscription() => _latestTranscription;
    }
}
