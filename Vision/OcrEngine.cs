using Tesseract;
using System.Threading.Tasks;
using System.Net.Http;

namespace Vision
{
    public class OcrEngine
    {
        private TesseractEngine? _engine;
        public OcrEngine(string dataPath, string lang = "eng+fra")
        {
            _engine = new TesseractEngine(dataPath, lang, EngineMode.LstmOnly);
        }

        public string RunTesseract(OpenCvSharp.Mat image)
        {
            // Convert OpenCvSharp.Mat to Bitmap
            using var bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(image);
            // Convert Bitmap to Pix
            using var ms = new System.IO.MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            ms.Position = 0;
            using var pix = Tesseract.Pix.LoadFromMemory(ms.ToArray());
            using var page = _engine!.Process(pix);
            return page.GetText();
        }

        public async Task<string> CallFreeOCRAPI(byte[] image)
        {
            using var client = new HttpClient();
            var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(image), "file", "screen.jpg");
            var response = await client.PostAsync("https://api.ocr.space/parse/image", content);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
