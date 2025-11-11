using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;

namespace TrabalhoElvis2.Services
{
    public class PixService
    {
        private readonly IWebHostEnvironment _env;

        public PixService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public string GerarQrCodePix(string nome, decimal valor, string descricao)
        {
            // Exemplo simples — substitua pelos dados reais do Pix da administradora
            string chavePix = "adm@vizinhapp.com";
            string payload = $"00020126360014BR.GOV.BCB.PIX0114{chavePix}520400005303986540{valor:0.00}5802BR5925{nome}6009SAO PAULO62070503***6304";

            using (var qrGen = new QRCodeGenerator())
            using (var qrData = qrGen.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new QRCode(qrData))
            using (var bmp = qrCode.GetGraphic(20))
            {
                string folder = Path.Combine(_env.WebRootPath, "qrcodes");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fileName = $"qrcode_{Guid.NewGuid()}.png";
                string filePath = Path.Combine(folder, fileName);

                bmp.Save(filePath, ImageFormat.Png);
                return $"/qrcodes/{fileName}";
            }
        }
    }
}
