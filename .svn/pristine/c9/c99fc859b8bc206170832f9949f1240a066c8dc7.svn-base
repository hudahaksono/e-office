using PDFEditor.Helper;
using iText.Kernel.Colors;
using iText.Layout;
using iText.Layout.Properties;

namespace PDFEditor.Template
{
    public static class FooterTemplate
    {
        public static void SetTemplate(Document document, int page, int pageTTE, float widthDoc, float heightDoc)
        {
            if (page == pageTTE)
            {
                Divider(document, page, widthDoc);
                SetIconBsre(document, page);
                //SetQrCode(document, page);
                KeteranganBsre(document, page, widthDoc);
            }
            
            if (page == 1)
                KataKataMutiara(document, page, widthDoc);
        }

        private static void Divider(Document document, int page, float widthDoc)
        {
            ComponentHelper.HorizontalLine(
                document, 
                page, 
                widthDoc - 84 - 2f, 
                42f, 
                2.5f * Constant.cm1);
        }

        private static void SetIconBsre(Document document, int page)
        {
            ComponentHelper.AddImageBSRE(
                document, 
                page, 
                30, 
                30, 
                42f, 
                2 * Constant.cm1);
        }

        private static void SetQrCode(Document document, int page)
        {
            var iQrCode = ComponentHelper.ImageQrCode(40, 40);
            var pQrCode = ComponentHelper.Paragraph(iQrCode);
            document.ShowTextAligned(
                    pQrCode,
                    42f + 30 + 10,
                    (2 * Constant.cm1) + 3,
                    page,
                    TextAlignment.LEFT,
                    VerticalAlignment.TOP,
                    0);
        }

        private static void KeteranganBsre(Document document, int page, float widthDoc)
        {
            var textKeteranganBsre = ComponentHelper.Text(
                "Dokumen ini sah dan telah ditandatangani secara elektronik melalui e-Office ATR/BPN. Untuk memastikan keasliannya, silakan pindai Kode QR dan pastikan menuju ke alamat https://eoffice.atrbpn.go.id/",
                textAlign: TextAlignment.LEFT, 
                eFont: FontHelper.FONT.BookmanOldStyle_Regular, 
                fontSize: 7.9f, 
                fontColor: ColorConstants.BLACK);

            var textVersi = ComponentHelper.Text(
                "v 1.03", 
                textAlign: TextAlignment.RIGHT, 
                eFont: FontHelper.FONT.BookmanOldStyle_Regular, 
                fontSize: 5, 
                fontColor: ColorConstants.BLACK);

            var pKeteranganBsre = ComponentHelper
                .Paragraph(textKeteranganBsre)
                .Add(
                    ComponentHelper.Paragraph(textVersi, TextAlignment.RIGHT).SetWidth(widthDoc - 175)
                    )
                .SetTextAlignment(TextAlignment.LEFT);
            pKeteranganBsre.SetWidth(widthDoc - 175);

            document.ShowTextAligned(
                    pKeteranganBsre,
                    131.5f,
                    2 * Constant.cm1,
                    page,
                    TextAlignment.LEFT,
                    VerticalAlignment.TOP,
                    0);
        }

        private static void KataKataMutiara(Document document, int page, float widthDoc)
        {
            var tKataKataMutiara = ComponentHelper.Text(
                "Melayani, Profesional, Terpercaya", 
                textAlign: TextAlignment.CENTER, 
                eFont: FontHelper.FONT.FreestyleScript_Regular, 
                fontSize: 14f, 
                fontColor: new DeviceRgb(0, 204, 255));
            var pKataKataMutiara = ComponentHelper.Paragraph(tKataKataMutiara, TextAlignment.CENTER);
            pKataKataMutiara.SetWidth(widthDoc);

            document.ShowTextAligned(
                    pKataKataMutiara,
                    0,
                    Constant.cm1,
                    page,
                    TextAlignment.LEFT,
                    VerticalAlignment.TOP,
                    0);
        }

    }
}
