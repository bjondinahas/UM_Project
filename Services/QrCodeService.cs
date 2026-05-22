using QRCoder;

namespace UM_Project.Services;

public interface IQrCodeService
{
    byte[] GeneratePng(string content, int pixelsPerModule = 8);
}

public class QrCodeService : IQrCodeService
{
    public byte[] GeneratePng(string content, int pixelsPerModule = 8)
    {
        using var generator = new QRCodeGenerator();
        var data = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        var qr = new PngByteQRCode(data);
        return qr.GetGraphic(pixelsPerModule);
    }
}
