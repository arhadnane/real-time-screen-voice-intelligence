using Tesseract;
using System.Threading.Tasks;
using System.Net.Http;
using OpenCvSharp;
using System.Collections.Concurrent;
using Serilog;
using System;
using System.IO;

namespace Vision
{
    public class OcrEngine : IDisposable
    {
        private TesseractEngine? _engine;
        private readonly ILogger _logger;
        private readonly ConcurrentQueue<byte[]> _imagePool;
        private readonly SemaphoreSlim _semaphore;
        private bool _disposed = false;

        public OcrEngine(string dataPath, string lang = "eng+fra", ILogger? logger = null)
        {
            _logger = logger ?? Log.ForContext<OcrEngine>();
            _imagePool = new ConcurrentQueue<byte[]>();
            _semaphore = new SemaphoreSlim(1, 1); // Single OCR operation at a time
            
            try
            {
                _engine = new TesseractEngine(dataPath, lang, EngineMode.LstmOnly);
                _logger.Information("OCR Engine initialized with languages: {Languages}", lang);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to initialize OCR engine");
                throw;
            }
        }

        public async Task<string> RunTesseractAsync(Mat image)
        {
            if (_disposed || _engine == null)
                throw new ObjectDisposedException(nameof(OcrEngine));

            await _semaphore.WaitAsync();
            try
            {
                return await Task.Run(() => RunTesseract(image));
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public string RunTesseract(Mat image)
        {
            if (_disposed || _engine == null)
                throw new ObjectDisposedException(nameof(OcrEngine));

            try
            {
                // Convert OpenCvSharp.Mat to Bitmap
                using var bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(image);
                
                // Convert Bitmap to Pix
                using var ms = new System.IO.MemoryStream();
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                ms.Position = 0;
                
                using var pix = Tesseract.Pix.LoadFromMemory(ms.ToArray());
                using var page = _engine.Process(pix);
                
                var text = page.GetText();
                var confidence = page.GetMeanConfidence();
                
                _logger.Debug("OCR completed with confidence: {Confidence:F2}%", confidence * 100);
                
                return text;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "OCR processing failed");
                return string.Empty;
            }
        }

        public async Task<string> CallFreeOCRAPI(byte[] image)
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(30);
                
                var content = new MultipartFormDataContent();
                content.Add(new ByteArrayContent(image), "file", "screen.jpg");
                
                var response = await client.PostAsync("https://api.ocr.space/parse/image", content);
                response.EnsureSuccessStatusCode();
                
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Free OCR API call failed");
                return string.Empty;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _engine?.Dispose();
                _semaphore?.Dispose();
                _disposed = true;
                _logger.Information("OCR Engine disposed");
            }
        }
    }
}
