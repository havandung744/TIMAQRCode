using System.ComponentModel.DataAnnotations;

namespace TIMAQRCode.Models
{
    public class QRCodeModel
    {
        [Display(Name = "Enter QRCode Text")]
        public string QRCodeText { get; set; }

        [Display(Name = "Mã định danh của người nhận thanh toán")]
        public string MerchantID { get; set; }

        [Display(Name = "Số tài khoản")]
        public string NumberAccount { get; set; }

        [Display(Name = "Số tiền cần thanh toán")]
        public string TransactionAmount { get; set; }

        //Mã tiền tệ ISO cho VNĐ là 704
        [Display(Name = "Mã tiền tệ ISO cho VNĐ")]
        public string CurrencyCode { get; set; } = "704";

        [Display(Name = "Nội dung chuyển khoản")]
        public string Contentck { get; set; }

        //các mã khác tham khảo tại link https://developers.momo.vn/v3/docs/payment/api/result-handling/bankcode/
        [Display(Name = "Mã ngân hàng")]
        public string BankId { get; set; }
    }
}
