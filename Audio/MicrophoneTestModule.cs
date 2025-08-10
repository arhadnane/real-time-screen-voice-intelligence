using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;
using NAudio.CoreAudioApi;

namespace Audio
{
    public class MicrophoneTestModule
    {
        private WaveInEvent? _waveIn;
        private bool _isRecording = false;
        private List<float> _audioLevels = new List<float>();
        private DateTime _testStartTime;
        private int _sampleCount = 0;

        public void StartMicrophoneTest()
        {
            Console.WriteLine("🎤 === DÉMARRAGE DU TEST MICROPHONE ===");
            Console.WriteLine("🔧 Phase 1: Énumération des dispositifs audio");
            
            EnumerateAudioDevices();
            
            Console.WriteLine("\n🔧 Phase 2: Test de capture audio");
            TestAudioCapture();
            
            Console.WriteLine("\n🔧 Phase 3: Test de niveau audio");
            TestAudioLevels();
        }

        private void EnumerateAudioDevices()
        {
            try
            {
                // Test NAudio devices
                Console.WriteLine("📋 Dispositifs NAudio WaveIn:");
                int deviceCount = WaveInEvent.DeviceCount;
                for (int i = 0; i < deviceCount; i++)
                {
                    var capabilities = WaveInEvent.GetCapabilities(i);
                    Console.WriteLine($"  Device {i}: {capabilities.ProductName} ({capabilities.Channels} channels)");
                }

                // Test Core Audio API devices (Windows uniquement)
                Console.WriteLine("\n📋 Dispositifs Core Audio:");
                try
                {
                    var enumerator = new MMDeviceEnumerator();
                    var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
                    
                    foreach (var device in devices)
                    {
                        Console.WriteLine($"  📱 {device.FriendlyName}");
                        Console.WriteLine($"     État: {device.State}");
                        Console.WriteLine($"     ID: {device.ID}");
                        
                        try
                        {
                            var format = device.AudioClient.MixFormat;
                            Console.WriteLine($"     Format: {format.SampleRate}Hz, {format.BitsPerSample}bit, {format.Channels}ch");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"     ⚠️ Impossible d'obtenir le format: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Core Audio non disponible: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur lors de l'énumération: {ex.Message}");
            }
        }

        private void TestAudioCapture()
        {
            try
            {
                Console.WriteLine("🎙️ Configuration du test de capture...");
                
                _waveIn = new WaveInEvent();
                _waveIn.WaveFormat = new WaveFormat(16000, 1); // 16kHz mono comme Vosk
                _waveIn.BufferMilliseconds = 100;
                
                _waveIn.DataAvailable += OnTestDataAvailable;
                _waveIn.RecordingStopped += OnTestRecordingStopped;
                
                Console.WriteLine($"📊 Format configuré: {_waveIn.WaveFormat.SampleRate}Hz, {_waveIn.WaveFormat.Channels}ch");
                Console.WriteLine("🔴 Démarrage de l'enregistrement test (10 secondes)...");
                Console.WriteLine("💬 PARLEZ MAINTENANT DANS LE MICROPHONE!");
                
                _testStartTime = DateTime.UtcNow;
                _audioLevels.Clear();
                _sampleCount = 0;
                _isRecording = true;
                
                _waveIn.StartRecording();
                
                // Enregistrer pendant 10 secondes
                System.Threading.Thread.Sleep(10000);
                
                _waveIn.StopRecording();
                _isRecording = false;
                
                Console.WriteLine("🛑 Test d'enregistrement terminé");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur lors du test de capture: {ex.Message}");
            }
        }

        private void OnTestDataAvailable(object? sender, WaveInEventArgs e)
        {
            if (!_isRecording) return;

            try
            {
                // Calculer le niveau audio
                float level = CalculateAudioLevel(e.Buffer, e.BytesRecorded);
                _audioLevels.Add(level);
                _sampleCount++;

                // Afficher le niveau en temps réel
                var elapsed = DateTime.UtcNow - _testStartTime;
                var levelBar = new string('█', Math.Min(20, (int)(level * 100)));
                var levelPercent = (level * 100).ToString("F1");
                
                Console.WriteLine($"🎵 [{elapsed.TotalSeconds:F1}s] Niveau: {levelBar.PadRight(20)} {levelPercent}%");
                
                // Détecter des pics audio
                if (level > 0.1f)
                {
                    Console.WriteLine($"🔊 AUDIO DÉTECTÉ! Niveau: {levelPercent}%");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Erreur lors du traitement audio: {ex.Message}");
            }
        }

        private void OnTestRecordingStopped(object? sender, StoppedEventArgs e)
        {
            Console.WriteLine("🏁 Enregistrement arrêté");
            if (e.Exception != null)
            {
                Console.WriteLine($"❌ Erreur d'arrêt: {e.Exception.Message}");
            }
        }

        private float CalculateAudioLevel(byte[] buffer, int bytesRecorded)
        {
            if (bytesRecorded == 0) return 0f;

            float sum = 0f;
            int sampleCount = bytesRecorded / 2; // 16-bit samples

            for (int i = 0; i < bytesRecorded; i += 2)
            {
                if (i + 1 < bytesRecorded)
                {
                    short sample = (short)(buffer[i] | (buffer[i + 1] << 8));
                    sum += Math.Abs(sample) / 32768f; // Normaliser à [0,1]
                }
            }

            return sampleCount > 0 ? sum / sampleCount : 0f;
        }

        private void TestAudioLevels()
        {
            Console.WriteLine("📊 === ANALYSE DES NIVEAUX AUDIO ===");
            
            if (_audioLevels.Count == 0)
            {
                Console.WriteLine("❌ Aucune donnée audio collectée");
                return;
            }

            var maxLevel = _audioLevels.Max();
            var avgLevel = _audioLevels.Average();
            var minLevel = _audioLevels.Min();
            var samplesAboveThreshold = _audioLevels.Count(l => l > 0.01f);
            var samplesAboveHighThreshold = _audioLevels.Count(l => l > 0.1f);

            Console.WriteLine($"📈 Statistiques audio:");
            Console.WriteLine($"   Échantillons total: {_audioLevels.Count}");
            Console.WriteLine($"   Niveau maximum: {(maxLevel * 100):F2}%");
            Console.WriteLine($"   Niveau moyen: {(avgLevel * 100):F2}%");
            Console.WriteLine($"   Niveau minimum: {(minLevel * 100):F2}%");
            Console.WriteLine($"   Échantillons > 1%: {samplesAboveThreshold} ({(samplesAboveThreshold * 100.0 / _audioLevels.Count):F1}%)");
            Console.WriteLine($"   Échantillons > 10%: {samplesAboveHighThreshold} ({(samplesAboveHighThreshold * 100.0 / _audioLevels.Count):F1}%)");

            // Recommandations
            Console.WriteLine("\n💡 Diagnostic:");
            if (maxLevel < 0.01f)
            {
                Console.WriteLine("❌ PROBLÈME: Aucun signal audio détecté");
                Console.WriteLine("   - Vérifiez que le microphone est connecté");
                Console.WriteLine("   - Vérifiez les autorisations microphone");
                Console.WriteLine("   - Testez le microphone dans d'autres applications");
            }
            else if (maxLevel < 0.05f)
            {
                Console.WriteLine("⚠️  Signal audio très faible");
                Console.WriteLine("   - Rapprochez-vous du microphone");
                Console.WriteLine("   - Augmentez le volume du microphone");
                Console.WriteLine("   - Parlez plus fort");
            }
            else if (maxLevel > 0.8f)
            {
                Console.WriteLine("⚠️  Signal audio très fort (risque de saturation)");
                Console.WriteLine("   - Éloignez-vous du microphone");
                Console.WriteLine("   - Diminuez le volume du microphone");
            }
            else
            {
                Console.WriteLine("✅ Niveau audio acceptable pour la reconnaissance vocale");
                if (samplesAboveHighThreshold < _audioLevels.Count * 0.1)
                {
                    Console.WriteLine("💬 Conseil: Parlez un peu plus fort pour améliorer la reconnaissance");
                }
            }

            // Test de compatibilité Vosk
            Console.WriteLine("\n🎯 Compatibilité Vosk:");
            Console.WriteLine($"   Format requis: 16kHz, 16-bit, mono ✅");
            Console.WriteLine($"   Niveau audio détecté: {(avgLevel > 0.01f ? "✅" : "❌")}");
            Console.WriteLine($"   Signal suffisant: {(maxLevel > 0.05f ? "✅" : "⚠️")}");
        }

        public void TestSpecificPhrase()
        {
            Console.WriteLine("\n🗣️  === TEST DE PHRASE SPÉCIFIQUE ===");
            Console.WriteLine("💬 Dites la phrase suivante clairement:");
            Console.WriteLine("   'Bonjour je teste le microphone'");
            Console.WriteLine("🔴 Enregistrement dans 3 secondes...");
            
            System.Threading.Thread.Sleep(1000);
            Console.WriteLine("2...");
            System.Threading.Thread.Sleep(1000);
            Console.WriteLine("1...");
            System.Threading.Thread.Sleep(1000);
            Console.WriteLine("🎙️ PARLEZ MAINTENANT!");

            try
            {
                _waveIn = new WaveInEvent();
                _waveIn.WaveFormat = new WaveFormat(16000, 1);
                _waveIn.BufferMilliseconds = 100;
                
                var phraseAudio = new List<byte>();
                _waveIn.DataAvailable += (sender, e) => {
                    for (int i = 0; i < e.BytesRecorded; i++)
                    {
                        phraseAudio.Add(e.Buffer[i]);
                    }
                    
                    float level = CalculateAudioLevel(e.Buffer, e.BytesRecorded);
                    if (level > 0.05f)
                    {
                        Console.Write("🔊");
                    }
                    else
                    {
                        Console.Write(".");
                    }
                };

                _waveIn.StartRecording();
                System.Threading.Thread.Sleep(5000); // 5 secondes
                _waveIn.StopRecording();
                
                Console.WriteLine($"\n✅ Phrase enregistrée: {phraseAudio.Count} bytes");
                Console.WriteLine("   Cette phrase peut maintenant être testée avec Vosk");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur lors du test de phrase: {ex.Message}");
            }
        }

        public void Dispose()
        {
            try
            {
                _waveIn?.StopRecording();
                _waveIn?.Dispose();
                _waveIn = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Erreur lors de la libération des ressources: {ex.Message}");
            }
        }
    }
}
