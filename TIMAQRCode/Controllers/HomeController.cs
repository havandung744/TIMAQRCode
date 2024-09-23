using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TIMAQRCode.Models;
using System.Text;
using System.Net.Http;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using static TIMAQRCode.Controllers.BitmapExtension;

namespace TIMAQRCode.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult CreateQRCode()
        {
            return View();
        }

        //[HttpPost]
        //public IActionResult CreateQRCode(QRCodeModel qRCode)
        //{
        //    QRCodeGenerator QrGenerator = new QRCodeGenerator();
        //    QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(qRCode.QRCodeText, QRCodeGenerator.ECCLevel.Q);
        //    QRCode QrCode = new QRCode(QrCodeInfo);
        //    Bitmap QrBitmap = QrCode.GetGraphic(60);
        //    byte[] BitmapArray = QrBitmap.BitmapToByteArray();
        //    string QrUri = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(BitmapArray));
        //    ViewBag.QrCodeUri = QrUri;
        //    return View();
        //}

        [HttpPost]
        public async Task<IActionResult> CreateQRCode(QRCodeModel qRCode)
        {
            //qRCode = new QRCodeModel()
            //{
            //    //MerchantID = "123",
            //    NumberAccount = "2926092001",
            //    TransactionAmount = "1000",
            //    CurrencyCode = "704",
            //    Contentck = "hvd chuyen khoan",
            //    BankId = "970407"
            //};
            //qRCode.MerchantID = GenAccountInformation(qRCode.BankId, qRCode.NumberAccount);
            //qRCode.Contentck = GenContentCK(qRCode.Contentck);
            //string emvCoQRData = GenerateEMVCoQRData(qRCode.MerchantID, qRCode.TransactionAmount, qRCode.CurrencyCode, qRCode.Contentck);
            ////emvCoQRData = "00020101021238590010A0000007270129000697044101150087040600864380208QRIBFTTA530370454061000005802VN62160812DTxxx dau tu6304A143";
            ////Bitmap qrCodeImage = BitmapExtension.GenerateQRCode(emvCoQRData);
            ////Bitmap qrCodeImage = BitmapExtension.GenerateQRCodeWithLogo(emvCoQRData, "Images/logo.png");
            ////var banks = await BitmapExtension.GetBanks();
            ////var logoBank = banks.FirstOrDefault(c => c.Bin == qRCode.BankId).Logo;
            ////Bitmap qrCodeImage = BitmapExtension.GenerateQRCodeWithLogo(emvCoQRData, "Images/logo.png");
            //Bitmap qrCodeImage = await BitmapExtension.GenerateQRCodeWithLogoAndFooter(emvCoQRData, "Images/logo.png", "Images/logoTIMA.png", "Images/napas.png", "haha");

            string merchantID = GenAccountInformation(qRCode.BankId, qRCode.NumberAccount);
            qRCode.Contentck = GenContentCK(qRCode.Contentck);
            string emvCoQRData = GenerateEMVCoQRData(merchantID, qRCode.TransactionAmount, "704", qRCode.Contentck);

            var banks = await GetBankByBinId(qRCode.BankId);
            var logoBank = banks.Logo;
            Bitmap qrCodeImage = await BitmapExtension.GenerateQRCodeWithLogoAndFooter(emvCoQRData, "Images/logo.png", "Images/logoTIMA.png", "Images/napas.png", logoBank);

            byte[] BitmapArray = qrCodeImage.BitmapToByteArray();
            string QrUri = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(BitmapArray));
            ViewBag.QrCodeUri = QrUri;
            return View();
        }

        //public static string CalcCRC16(byte[] data)
        //{
        //    ushort crc = 0xFFFF;
        //    for (int i = 0; i < data.Length; i++)
        //    {
        //        crc ^= (ushort)(data[i] << 8);
        //        for (int j = 0; j < 8; j++)
        //        {
        //            if ((crc & 0x8000) > 0)
        //                crc = (ushort)((crc << 1) ^ 0x1021);
        //            else
        //                crc <<= 1;
        //        }
        //    }
        //    return crc.ToString("X4");
        //}
        //string GenAccountInformation(string bankCode, string accountNumber)
        //{
        //    string bank_code = GenAcquier(bankCode) + GenAccountNo(accountNumber);

        //    string merchantID = "0010" + "A000000727";
        //    string bankID = "01" + bank_code.Length.ToString("D2") + bank_code;
        //    //string accountId = "01" + accountNumber.Length.ToString("D2") + accountNumber;
        //    string services = "0208" + "QRIBFTTA";
        //    return merchantID + bankID + services;
        //}

        //private static string GenContentCK(string content)
        //{
        //    return "08" + content.Length.ToString("D2") + content;
        //}

        //private static string GenAcquier(string bankCode)
        //{
        //    return "00" + bankCode.Length.ToString("D2") + bankCode;
        //}

        //private static string GenAccountNo(string accountNumber)
        //{
        //    return "01" + accountNumber.Length.ToString("D2") + accountNumber;
        //}

        //private static string GenerateEMVCoQRData(string merchantID, string amount, string currencyCode, string content)
        //{
        //    StringBuilder emvCoQRData = new StringBuilder();

        //    // Phiên bản của EMVCo (ID 00)
        //    emvCoQRData.Append("00");
        //    emvCoQRData.Append("02"); // Độ dài dữ liệu
        //    emvCoQRData.Append("01"); // Phiên bản 01 của EMVCo

        //    // Phương thức khởi tạo
        //    emvCoQRData.Append("01");
        //    emvCoQRData.Append("02");
        //    emvCoQRData.Append("12");

        //    // Merchant account information (ID 26)
        //    //emvCoQRData.Append("26"); // ID của trường Merchant
        //    //emvCoQRData.Append(merchantID.Length.ToString("D2")); // Độ dài thông tin
        //    //emvCoQRData.Append(merchantID); // Thông tin Merchant

        //    // Mã thông tin người thụ hưởng (ID 38)
        //    emvCoQRData.Append("38");
        //    emvCoQRData.Append(merchantID.Length.ToString("D2"));
        //    emvCoQRData.Append(merchantID);

        //    // Mã tiền tệ (ID 53)
        //    emvCoQRData.Append("53");
        //    emvCoQRData.Append(currencyCode.Length.ToString("D2"));
        //    emvCoQRData.Append(currencyCode);

        //    // Số tiền thanh toán (ID 54)
        //    emvCoQRData.Append("54");
        //    emvCoQRData.Append(amount.Length.ToString("D2"));
        //    emvCoQRData.Append(amount);

        //    // Mã quốc gia (ID 58)
        //    emvCoQRData.Append("58");
        //    emvCoQRData.Append("02");
        //    emvCoQRData.Append("VN");

        //    //// Mã Nội dung ck (ID 62)
        //    emvCoQRData.Append("62");
        //    emvCoQRData.Append(content.Length.ToString("D2"));
        //    emvCoQRData.Append(content);

        //    // Thêm các trường khác nếu cần thiết (ví dụ: tham số bảo mật, mô tả giao dịch, ...)

        //    return emvCoQRData.ToString() + "6304" + CalcCRC16(Encoding.ASCII.GetBytes((emvCoQRData.ToString() + "6304")));
        //}

        async Task<Bank> GetBankByBinId(string binToFind)
        {
            string jsonString = System.IO.File.ReadAllText("Jsons/banks.json");

            BankResponse bankResponse = JsonConvert.DeserializeObject<BankResponse>(jsonString);
            Bank bank = bankResponse.Data.FirstOrDefault(b => b.Bin == binToFind);

            if (bank != null)
            {
                return bank;
            }
            else
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, "https://api.vietqr.io/v2/banks");
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                bankResponse = JsonConvert.DeserializeObject<BankResponse>(json);
                return bankResponse.Data.FirstOrDefault(b => b.Bin == binToFind);
            }
        }

        string GenAccountInformation(string bankCode, string accountNumber)
        {
            string bank_code = GenAcquier(bankCode) + GenAccountNo(accountNumber);

            string merchantID = "0010" + "A000000727";
            string bankID = "01" + bank_code.Length.ToString("D2") + bank_code;
            //string accountId = "01" + accountNumber.Length.ToString("D2") + accountNumber;
            string services = "0208" + "QRIBFTTA";
            return merchantID + bankID + services;
        }

        string GenContentCK(string content)
        {
            return "08" + content.Length.ToString("D2") + content;
        }

        string GenAcquier(string bankCode)
        {
            return "00" + bankCode.Length.ToString("D2") + bankCode;
        }

        string GenAccountNo(string accountNumber)
        {
            return "01" + accountNumber.Length.ToString("D2") + accountNumber;
        }

        string GenerateEMVCoQRData(string merchantID, string amount, string currencyCode, string content)
        {
            StringBuilder emvCoQRData = new StringBuilder();

            // Phiên bản của EMVCo (ID 00)
            emvCoQRData.Append("00");
            emvCoQRData.Append("02"); // Độ dài dữ liệu
            emvCoQRData.Append("01"); // Phiên bản 01 của EMVCo

            // Phương thức khởi tạo
            emvCoQRData.Append("01");
            emvCoQRData.Append("02");
            emvCoQRData.Append("12");

            // Merchant account information (ID 26)
            //emvCoQRData.Append("26"); // ID của trường Merchant
            //emvCoQRData.Append(merchantID.Length.ToString("D2")); // Độ dài thông tin
            //emvCoQRData.Append(merchantID); // Thông tin Merchant

            // Mã thông tin người thụ hưởng (ID 38)
            emvCoQRData.Append("38");
            emvCoQRData.Append(merchantID.Length.ToString("D2"));
            emvCoQRData.Append(merchantID);

            // Mã tiền tệ (ID 53)
            emvCoQRData.Append("53");
            emvCoQRData.Append(currencyCode.Length.ToString("D2"));
            emvCoQRData.Append(currencyCode);

            // Số tiền thanh toán (ID 54)
            emvCoQRData.Append("54");
            emvCoQRData.Append(amount.Length.ToString("D2"));
            emvCoQRData.Append(amount);

            // Mã quốc gia (ID 58)
            emvCoQRData.Append("58");
            emvCoQRData.Append("02");
            emvCoQRData.Append("VN");

            //// Mã Nội dung ck (ID 62)
            emvCoQRData.Append("62");
            emvCoQRData.Append(content.Length.ToString("D2"));
            emvCoQRData.Append(content);

            // Thêm các trường khác nếu cần thiết (ví dụ: tham số bảo mật, mô tả giao dịch, ...)

            return emvCoQRData.ToString() + "6304" + CalcCRC16(Encoding.ASCII.GetBytes((emvCoQRData.ToString() + "6304")));

        }

        public string CalcCRC16(byte[] data)
        {
            ushort crc = 0xFFFF;
            for (int i = 0; i < data.Length; i++)
            {
                crc ^= (ushort)(data[i] << 8);
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x8000) > 0)
                        crc = (ushort)((crc << 1) ^ 0x1021);
                    else
                        crc <<= 1;
                }
            }
            return crc.ToString("X4");
        }
    }

    //Extension method to convert Bitmap to Byte Array
    public static class BitmapExtension
    {
        public static Bitmap GenerateQRCode(string data)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            return qrCode.GetGraphic(20); // Độ lớn của mã QR
        }

        public static byte[] BitmapToByteArray(this Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }
        public static async Task<List<Bank>> GetBanks()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.vietqr.io/v2/banks");
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var bankResponse = JsonConvert.DeserializeObject<BankResponse>(json);
            return bankResponse.Data;
        }

        public static Bitmap GenerateQRCodeWithLogo(string data, string logoRelativePath, int qrSize = 250, int logoSizePercentage = 20)
        {
            // Create the QR code
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeBitmap = qrCode.GetGraphic(qrSize);

            // Load the logo from file
            Bitmap logoBitmap;
            try
            {
                logoBitmap = new Bitmap(Path.Combine(Directory.GetCurrentDirectory(), logoRelativePath));
            }
            catch (Exception ex)
            {
                throw new FileNotFoundException("The specified logo file could not be found.", ex);
            }

            // Resize the logo to fit within the QR code nicely
            int maxLogoWidth = (int)(qrCodeBitmap.Width * logoSizePercentage / 100.0);
            int maxLogoHeight = (int)(qrCodeBitmap.Height * logoSizePercentage / 100.0);
            if (logoBitmap.Width > maxLogoWidth || logoBitmap.Height > maxLogoHeight)
            {
                int logoSize = Math.Min(maxLogoWidth, maxLogoHeight);
                logoBitmap = new Bitmap(logoBitmap, new Size(logoSize, logoSize));
            }
            else
            {
                double scale = Math.Min(maxLogoWidth / (double)logoBitmap.Width, maxLogoHeight / (double)logoBitmap.Height);
                int newWidth = (int)(logoBitmap.Width * scale);
                int newHeight = (int)(logoBitmap.Height * scale);
                logoBitmap = new Bitmap(logoBitmap, new Size(newWidth, newHeight));
            }

            // Create a new image to contain the QR code and the logo
            using (Graphics graphics = Graphics.FromImage(qrCodeBitmap))
            {
                int logoX = (qrCodeBitmap.Width - logoBitmap.Width) / 2;
                int logoY = (qrCodeBitmap.Height - logoBitmap.Height) / 2;
                Rectangle logoRect = new Rectangle(logoX, logoY, logoBitmap.Width, logoBitmap.Height);
                graphics.DrawImage(logoBitmap, logoRect);
            }

            return qrCodeBitmap;
        }

        public static async Task<Bitmap> GenerateQRCodeWithLogoAndFooter(string data, string logoRelativePath, string headerPath, string footerRelativePath, string footerLogoRelativePath, int qrSize = 20, int logoSizePercentage = 20, int footerSizePercentage = 20)
        {
            try
            {

                // Generate the QR code with the logo
                Bitmap qrCodeWithLogo = GenerateQRCodeWithLogo(data, logoRelativePath, qrSize, logoSizePercentage);

                // Load the header image from file
                Bitmap headerBitmap;
                headerBitmap = new Bitmap(Path.Combine(Directory.GetCurrentDirectory(), headerPath));

                // Resize the header to fit within the QR code nicely
                int maxHeaderWidth = (int)(qrCodeWithLogo.Width * 25 / 100.0);
                int maxHeaderHeight = (int)(qrCodeWithLogo.Height * 25 / 100.0);
                if (headerBitmap.Width > maxHeaderWidth || headerBitmap.Height > maxHeaderHeight)
                {
                    int footerSize = Math.Min(maxHeaderWidth, maxHeaderHeight);
                    headerBitmap = new Bitmap(headerBitmap, new Size(footerSize, footerSize));
                }
                else
                {
                    double scale = Math.Min(maxHeaderWidth / (double)headerBitmap.Width, maxHeaderHeight / (double)headerBitmap.Height);
                    int newWidth = (int)(headerBitmap.Width * scale);
                    int newHeight = (int)(headerBitmap.Height * scale);
                    headerBitmap = new Bitmap(headerBitmap, new Size(newWidth, newHeight));
                }

                // Load the footer image from file
                Bitmap footerBitmap;
                footerBitmap = new Bitmap(Path.Combine(Directory.GetCurrentDirectory(), footerRelativePath));

                // Resize the footer to fit within the QR code nicely
                int maxFooterWidth = (int)(qrCodeWithLogo.Width * footerSizePercentage / 100.0);
                int maxFooterHeight = (int)(qrCodeWithLogo.Height * footerSizePercentage / 100.0);
                if (footerBitmap.Width > maxFooterWidth || footerBitmap.Height > maxFooterHeight)
                {
                    int footerSize = Math.Min(maxFooterWidth, maxFooterHeight);
                    footerBitmap = new Bitmap(footerBitmap, new Size(footerSize, footerSize));
                }
                else
                {
                    double scale = Math.Min(maxFooterWidth / (double)footerBitmap.Width, maxFooterHeight / (double)footerBitmap.Height);
                    int newWidth = (int)(footerBitmap.Width * scale);
                    int newHeight = (int)(footerBitmap.Height * scale);
                    footerBitmap = new Bitmap(footerBitmap, new Size(newWidth, newHeight));
                }

                // Load the footer logo from file
                Bitmap footerLogoBitmap;
                //footerLogoBitmap = new Bitmap(Path.Combine(Directory.GetCurrentDirectory(), footerLogoRelativePath));
                footerLogoBitmap = await LoadBitmapFromUrlAsync(footerLogoRelativePath);
                // Resize the footer logo
                int maxFooterLogoWidth = (int)(qrCodeWithLogo.Width * logoSizePercentage / 100.0);
                int maxFooterLogoHeight = (int)(qrCodeWithLogo.Height * logoSizePercentage / 100.0);
                if (footerLogoBitmap.Width > maxFooterLogoWidth || footerLogoBitmap.Height > maxFooterLogoHeight)
                {
                    int footerLogoSize = Math.Min(maxFooterLogoWidth, maxFooterLogoHeight);
                    footerLogoBitmap = new Bitmap(footerLogoBitmap, new Size(footerLogoSize, footerLogoSize));
                }
                else
                {
                    double scale = Math.Min(maxFooterLogoWidth / (double)footerLogoBitmap.Width, maxFooterLogoHeight / (double)footerLogoBitmap.Height);
                    int newWidth = (int)(footerLogoBitmap.Width * scale);
                    int newHeight = (int)(footerLogoBitmap.Height * scale);
                    footerLogoBitmap = new Bitmap(footerLogoBitmap, new Size(newWidth, newHeight));
                }

                Bitmap finalBitmap = new Bitmap(qrCodeWithLogo.Width, qrCodeWithLogo.Height + footerBitmap.Height + headerBitmap.Height);
                using (Graphics graphics = Graphics.FromImage(finalBitmap))
                {
                    // Draw the QR code with the logo
                    graphics.DrawImage(qrCodeWithLogo, 0, headerBitmap.Height);

                    // Draw the header image
                    int headerX = (finalBitmap.Width - headerBitmap.Width) / 2;
                    int headerY = 0; // Position the footer below the QR code
                    graphics.DrawImage(headerBitmap, new Rectangle(headerX, headerY, headerBitmap.Width, headerBitmap.Height));

                    // Draw the footer image (aligned right)
                    int footerX = ((qrCodeWithLogo.Width / 2) - (footerBitmap.Width * 2)); // Align footer image to the left
                    int footerY = qrCodeWithLogo.Height + headerBitmap.Height; // Position the footer below the QR code
                    graphics.DrawImage(footerBitmap, new Rectangle(footerX, footerY, footerBitmap.Width, footerBitmap.Height));

                    // Draw footer logo (aligned right)
                    int footerLogoX = (qrCodeWithLogo.Width / 2); // Align footer logo to the right
                    int footerLogoY = footerY + (footerBitmap.Height - footerLogoBitmap.Height) / 2; // Center vertically
                    graphics.DrawImage(footerLogoBitmap, new Rectangle(footerLogoX, footerLogoY, footerLogoBitmap.Width * 2, footerLogoBitmap.Height));
                }
                return finalBitmap;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return new Bitmap(1, 1);
        }

        public static async Task<Bitmap> LoadBitmapFromUrlAsync(string imageUrl)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                // Download the image data from the URL
                byte[] imageBytes = await httpClient.GetByteArrayAsync(imageUrl);

                // Create a memory stream from the image data
                using (MemoryStream memoryStream = new MemoryStream(imageBytes))
                {
                    // Create a bitmap from the memory stream
                    return new Bitmap(memoryStream);
                }
            }
        }

        public class Bank
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Code { get; set; }
            public string Bin { get; set; }
            public string ShortName { get; set; }
            public string Logo { get; set; }
            public int TransferSupported { get; set; }
            public int LookupSupported { get; set; }
            public string Short_name { get; set; }
            public int Support { get; set; }
            public int IsTransfer { get; set; }
            public string Swift_code { get; set; }
        }

        public class BankResponse
        {
            public string Code { get; set; }
            public string Desc { get; set; }
            public List<Bank> Data { get; set; }
        }
    }
}
