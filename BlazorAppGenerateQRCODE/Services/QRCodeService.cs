using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using QRCoder;
using SkiaSharp;
using System.Drawing;
using System.IO;
using ZXing;

namespace BlazorAppGenerateQRCODE.Services
{
    public class QRCodeService
    {
        public byte[] GenerateQRCode(string activationCode, string appUrl, string format)
        {
            try
            {
                // Combiner les données
                // string combinedData = $"Activation Code: {activationCode}\nDownload App: {appUrl}";
                string combinedData = $"{appUrl}?={activationCode}";

                // Générer le QR Code
                using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                {
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(combinedData, QRCodeGenerator.ECCLevel.Q);
                    using (QRCode qrCode = new QRCode(qrCodeData))
                    {
                        using (Bitmap qrCodeImage = qrCode.GetGraphic(20))
                        {
                            return format.ToLower() switch
                            {
                                "pdf" => GeneratePDF(qrCodeImage),
                                _ => GenerateImage(qrCodeImage, format)
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            
        }
       
        private byte[] GenerateImage(Bitmap qrCodeImage, string format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                if (format.ToLower() == "png")
                {
                    qrCodeImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                }
                else if (format.ToLower() == "jpg" || format.ToLower() == "jpeg")
                {
                    qrCodeImage.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
                return ms.ToArray();
            }
        }

        private byte[] GeneratePDF(Bitmap qrCodeImage)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Créer un document PDF
                PdfDocument pdf = new PdfDocument();
                PdfPage page = pdf.AddPage();
                XGraphics gfx = XGraphics.FromPdfPage(page);

                // Dessiner l'image QR code dans le PDF
                using (MemoryStream imgStream = new MemoryStream())
                {
                    qrCodeImage.Save(imgStream, System.Drawing.Imaging.ImageFormat.Png);
                  //  XImage xImage = XImage.FromStream(imgStream);
                  //  gfx.DrawImage(xImage, 100, 100);  // Positionner l'image sur la page
                }

                // Enregistrer le PDF dans un flux mémoire
                pdf.Save(ms);
                return ms.ToArray();
            }
        }
        public string DecodeQRCode(byte[] qrCodeImage)
        {
            using var ms = new MemoryStream(qrCodeImage);
            var barcodeReader = new ZXing.BarcodeReaderGeneric();

            try
            {
                // Decode image into Bitmap
             
                using var bitmap = SKBitmap.Decode(ms); // Decode image using SkiaSharp
                var barcodeReader1 = new ZXing.SkiaSharp.BarcodeReader();
                var result = barcodeReader1.Decode(bitmap);

                if (result != null)
                {
                    return  result.Text;

                }
                else
                {
                    throw new Exception("No QR code detected in the image.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Decoding failed: " + ex.Message);
            }
        }

    }
}
