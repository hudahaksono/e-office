using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;
using MessagingToolkit.QRCode.Codec;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QRCoder;
using Surat.Models.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Surat.Codes
{
    public class Functions
    {
        Models.HakAksesModel hakAksesModel = new Models.HakAksesModel();
        Models.DataMasterModel dataMasterModel = new Models.DataMasterModel();

        public class userIdentity
        {
            public string UserId { get; set; }
            public string PegawaiId { get; set; }
            public string NamaPegawai { get; set; }
            public string KantorId { get; set; }
            public string NamaKantor { get; set; }
            public string ProfileIdTU { get; set; }
            public string UnitKerjaId { get; set; }
            public string Email { get; set; }
        }

        public userIdentity claimUser()
        {
            var kc = HttpContext.Current.User.Identity as InternalUserIdentity;
            var userlogin = new userIdentity();
            if(kc != null)
            {
                userlogin.UserId = kc.UserId;
                userlogin.PegawaiId = kc.PegawaiId;
                userlogin.NamaPegawai = kc.NamaPegawai;
                userlogin.KantorId = kc.KantorId;
                userlogin.NamaKantor = kc.NamaKantor;
                userlogin.ProfileIdTU = kc.ProfileIdTU;
                userlogin.UnitKerjaId = kc.UnitKerjaId;
                userlogin.Email = kc.Email;
            }
            else
            {
                userlogin = null;
            }

            return userlogin;
        }

        public string MyProfiles(string pegawaiid, string kantorid)
        {
            string[] userProfiles = hakAksesModel.GetProfileIdForUser(pegawaiid, kantorid);

            string profileid = "";


            if (!String.IsNullOrEmpty(HttpContext.Current.Request.QueryString["profileid"]))
            {
                profileid += "'" + HttpContext.Current.Request.QueryString["profileid"] + "',";
            }

            if (!String.IsNullOrEmpty(HttpContext.Current.Request.QueryString["profilename"]))
            {
                string profilename = HttpContext.Current.Request.QueryString["profilename"];

                profileid += "'" + dataMasterModel.GetProfileIdFromName(profilename) + "',";
            }
            else
            {
                foreach (string strProfileId in userProfiles)
                {
                    profileid += "'" + strProfileId + "',";
                }
            }

            if (profileid.Length > 0)
            {
                profileid = profileid.Remove(profileid.Length - 1, 1);
            }

            return profileid;
        }

        public static string ProperCase(string strIn)
        {
            try
            {
                System.Globalization.TextInfo textInfo = new System.Globalization.CultureInfo("id-ID", false).TextInfo;

                return textInfo.ToTitleCase(strIn);
            }
            catch (RegexMatchTimeoutException)
            {
                return String.Empty;
            }
        }

        public static string NomorRomawi(int pNomor)
        {
            List<string> romanNumerals = new List<string>() { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };
            List<int> numerals = new List<int>() { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };

            var romanNumeral = string.Empty;
            while (pNomor > 0)
            {
                // find biggest numeral that is less than equal to number
                var index = numerals.FindIndex(x => x <= pNomor);
                // subtract it's value from your number
                pNomor -= numerals[index];
                // tack it onto the end of your roman numeral
                romanNumeral += romanNumerals[index];
            }
            return romanNumeral;
        }

        public static string MyClientId
        {
            get
            {
                string result = "";
                string errmess = "";

                var context = HttpContext.Current;

                try
                {
                    string nip = (context.User.Identity as InternalUserIdentity).PegawaiId;

                    string ipAddress = context.Request.ServerVariables["REMOTE_ADDR"];
                    ipAddress = ipAddress.Replace("::1", "127.0.0.1");
                    result = nip + "." + ipAddress;
                }
                catch (Exception ex)
                {
                    errmess = ex.Message;
                    result = "";
                }

                return result;
            }
        }

        #region Stream methods
        public static void WriteFile2Disk(string path, string filename, byte[] bytefile)
        {
            try
            {
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }

                string fullfilename = path + "\\" + filename;

                using (System.IO.BinaryWriter binWriter = new System.IO.BinaryWriter(System.IO.File.Open(fullfilename, System.IO.FileMode.OpenOrCreate)))
                {
                    binWriter.Write(bytefile);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void DeleteFile2Disk(string path, string filename)
        {
            try
            {
                string fullfilename = path + "\\" + filename;

                if (!String.IsNullOrEmpty(fullfilename))
                {
                    // Delete File
                    System.IO.FileInfo file = new System.IO.FileInfo(fullfilename);
                    if (file.Exists)
                    {
                        file.Delete();
                    }

                    // Delete Folder DOKUMENID
                    var dir = new System.IO.DirectoryInfo(@path);
                    dir.Attributes = dir.Attributes & ~System.IO.FileAttributes.ReadOnly;
                    dir.Delete(true);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static byte[] BytesFromFile(string path, string filename)
        {
            byte[] result = null;

            string fullfilename = path + "\\" + filename;
            System.IO.FileInfo fileinfo = new System.IO.FileInfo(fullfilename);
            if (fileinfo.Exists)
            {
                int nFileLen = Convert.ToInt32(fileinfo.Length);
                if (nFileLen > 0)
                {
                    result = Functions.ReadAllBytes(fullfilename);
                }
            }

            return result;
        }

        public static byte[] ReadAllBytes(string filename)
        {
            byte[] buffer = null;
            using (System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                buffer = new byte[fs.Length];
                fs.Read(buffer, 0, (int)fs.Length);
            }
            return buffer;
        }

        public static byte[] ReadImageBytes(string filename)
        {
            System.IO.Stream filestream = new System.IO.FileStream(@filename, System.IO.FileMode.Open);

            byte[] byteImage = new byte[filestream.Length];

            try
            {
                var maxSize = 800;
                var newWidth = 0;
                var newHeight = 0;

                System.Drawing.Image uploadedImage = System.Drawing.Image.FromStream(filestream);
                int UploadedImageWidth = Convert.ToInt32(uploadedImage.PhysicalDimension.Width);
                int UploadedImageHeight = Convert.ToInt32(uploadedImage.PhysicalDimension.Height);

                if (UploadedImageWidth > maxSize || UploadedImageHeight > maxSize)
                {
                    var ratioX = (double)maxSize / UploadedImageWidth;
                    var ratioY = (double)maxSize / UploadedImageHeight;
                    var ratio = Math.Min(ratioX, ratioY);

                    newWidth = (int)(UploadedImageWidth * ratio);
                    newHeight = (int)(UploadedImageHeight * ratio);
                }
                else
                {
                    newWidth = UploadedImageWidth;
                    newHeight = UploadedImageHeight;
                }

                var newImage = new System.Drawing.Bitmap(newWidth, newHeight);
                System.Drawing.Graphics.FromImage(newImage).DrawImage(uploadedImage, 0, 0, newWidth, newHeight);
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(newImage);

                using (var memoryStream = new System.IO.MemoryStream())
                {
                    bmp.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    byteImage = memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                filestream.Close();
            }

            return byteImage;
        }

        public static System.Drawing.Image ByteArrayToImage(byte[] byteArrayIn)
        {
            System.Drawing.Image returnImage = null;

            try
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream(byteArrayIn);
                returnImage = System.Drawing.Image.FromStream(ms); // parameter is invalid

            }
            catch (Exception ex)
            {
                string a = ex.ToString();
            }

            return returnImage;
        }

        public static byte[] StreamToByteArray(System.IO.Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public static List<System.Drawing.Image> GetAllTiffImageFromFile(string file)
        {
            List<System.Drawing.Image> images = new List<System.Drawing.Image>();
            System.Drawing.Bitmap bitmap = (System.Drawing.Bitmap)System.Drawing.Image.FromFile(file);
            int count = bitmap.GetFrameCount(System.Drawing.Imaging.FrameDimension.Page);
            for (int idx = 0; idx < count; idx++)
            {
                // save each frame to a bytestream
                bitmap.SelectActiveFrame(System.Drawing.Imaging.FrameDimension.Page, idx);
                System.IO.MemoryStream byteStream = new System.IO.MemoryStream();
                bitmap.Save(byteStream, System.Drawing.Imaging.ImageFormat.Tiff);

                // and then create a new Image from it
                images.Add(System.Drawing.Image.FromStream(byteStream));
            }
            return images;
        }

        public static List<System.Drawing.Image> GetAllTiffImageFromBuffer(byte[] byteArrayIn)
        {
            List<System.Drawing.Image> images = new List<System.Drawing.Image>();

            System.ComponentModel.TypeConverter tc = System.ComponentModel.TypeDescriptor.GetConverter(typeof(System.Drawing.Bitmap));
            System.Drawing.Bitmap bitmap = (System.Drawing.Bitmap)tc.ConvertFrom(byteArrayIn);

            int count = bitmap.GetFrameCount(System.Drawing.Imaging.FrameDimension.Page);
            for (int idx = 0; idx < count; idx++)
            {
                // save each frame to a bytestream
                bitmap.SelectActiveFrame(System.Drawing.Imaging.FrameDimension.Page, idx);
                System.IO.MemoryStream byteStream = new System.IO.MemoryStream();
                bitmap.Save(byteStream, System.Drawing.Imaging.ImageFormat.Tiff);

                // and then create a new Image from it
                images.Add(System.Drawing.Image.FromStream(byteStream));
            }
            return images;
        }
        public Stream MergeFiles(List<Stream> sourceFiles)
        {
            try
            {
                PdfReader reader;
                Document sourceDocument;
                PdfCopy pdfCopyProvider;
                //PdfImportedPage importedPage;
                string outputPdfPath = Path.Combine(Path.GetTempPath(), "merge.pdf");

                sourceDocument = new Document();
                pdfCopyProvider = new PdfCopy(sourceDocument, new FileStream(outputPdfPath, FileMode.Create, FileAccess.ReadWrite));
                //output file Open  
                sourceDocument.Open();

                //files list wise Loop  
                int totalPages = 0;
                for (int f = 0; f < sourceFiles.Count; f++)
                {
                    try
                    {
                        reader = new PdfReader(sourceFiles[f]);
                        int pages = reader.NumberOfPages;//TotalPageCount(fileArray[f]);
                                                         //Add pages in new file  
                        for (int i = 1; i <= pages; i++)
                        {
                            try
                            {
                                PdfImportedPage importedPage = pdfCopyProvider.GetImportedPage(reader, i);
                                PdfCopy.PageStamp pageStamp = pdfCopyProvider.CreatePageStamp(importedPage);

                                pageStamp.AlterContents();
                                pdfCopyProvider.AddPage(importedPage);
                            }
                            catch (Exception e)
                            {
                                throw e;
                            }
                        }
                        totalPages = totalPages + pages;
                        reader.Close();
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
                sourceDocument.Close();
                var tempByte = System.IO.File.ReadAllBytes(outputPdfPath);
                var tempStream = new MemoryStream(tempByte);

                if (System.IO.File.Exists(outputPdfPath))
                {
                    System.IO.File.Delete(outputPdfPath);
                }
                return tempStream;
            }
            catch (Exception e)
            {
                _ = e.StackTrace;
            }
            return null;
        }


        #endregion

        #region Send Mail

        public string SendEmail(string emailTo, string emailSubyek, string emailBody, List<Models.Entities.SessionLampiranSurat> dataSessionLampiran)
        {
            string result = "";

            try
            {
                string strSmtpClient = ConfigurationManager.AppSettings["SmtpClient"].ToString();
                string strSmtpPort = ConfigurationManager.AppSettings["SmtpPort"].ToString();
                string strAddrEmail = ConfigurationManager.AppSettings["AddrEmail"].ToString();
                string strPassEmail = ConfigurationManager.AppSettings["PassEmail"].ToString();

                System.Net.Mail.MailMessage mailMessage = new System.Net.Mail.MailMessage();
                System.Net.Mail.SmtpClient smtpClient = new System.Net.Mail.SmtpClient(strSmtpClient);

                string fromEmail = strAddrEmail;
                string passEmail = strPassEmail;

                foreach (Models.Entities.SessionLampiranSurat lampiranSurat in dataSessionLampiran)
                {
                    if (lampiranSurat.ObjectFile.Length > 0)
                    {
                        System.IO.Stream filestream = new System.IO.MemoryStream(lampiranSurat.ObjectFile);
                        System.Net.Mail.Attachment data = new System.Net.Mail.Attachment(filestream, System.Net.Mime.MediaTypeNames.Application.Pdf);
                        mailMessage.Attachments.Add(data);
                    }
                }

                mailMessage.From = new System.Net.Mail.MailAddress(fromEmail);
                mailMessage.To.Add(emailTo);
                mailMessage.Subject = emailSubyek;

                mailMessage.Body += "<html>";
                mailMessage.Body += "<body>";
                mailMessage.Body += "<table>";
                mailMessage.Body += "<tr>";
                mailMessage.Body += "<td>Surat:</td><td>" + emailBody + "</td>";
                mailMessage.Body += "</tr>";
                mailMessage.Body += "</table>";
                mailMessage.Body += "</body>";
                mailMessage.Body += "</html>";

                mailMessage.IsBodyHtml = true;

                smtpClient.Port = Convert.ToInt32(strSmtpPort);
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new System.Net.NetworkCredential(fromEmail, passEmail);
                smtpClient.EnableSsl = false;

                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return result;
        }

        public string SendEmail2(string emailTo, string emailSubyek, string emailBody, List<Models.Entities.LampiranSurat> dataLampiranSurat)
        {
            string result = "";

            try
            {
                string strSmtpClient = ConfigurationManager.AppSettings["SmtpClient"].ToString();
                string strSmtpPort = ConfigurationManager.AppSettings["SmtpPort"].ToString();
                string strAddrEmail = ConfigurationManager.AppSettings["AddrEmail"].ToString();
                string strPassEmail = ConfigurationManager.AppSettings["PassEmail"].ToString();

                System.Net.Mail.MailMessage mailMessage = new System.Net.Mail.MailMessage();
                System.Net.Mail.SmtpClient smtpClient = new System.Net.Mail.SmtpClient(strSmtpClient);

                string fromEmail = strAddrEmail;
                string passEmail = strPassEmail;

                foreach (Models.Entities.LampiranSurat lampiranSurat in dataLampiranSurat)
                {
                    if (lampiranSurat.ObjectFile.Length > 0)
                    {
                        System.IO.Stream filestream = new System.IO.MemoryStream(lampiranSurat.ObjectFile);
                        System.Net.Mail.Attachment data = new System.Net.Mail.Attachment(filestream, System.Net.Mime.MediaTypeNames.Application.Pdf);
                        mailMessage.Attachments.Add(data);
                    }
                }

                mailMessage.From = new System.Net.Mail.MailAddress(fromEmail);
                mailMessage.To.Add(emailTo);
                mailMessage.Subject = emailSubyek;

                mailMessage.Body += "<html>";
                mailMessage.Body += "<body>";
                mailMessage.Body += "<table>";
                mailMessage.Body += "<tr>";
                mailMessage.Body += "<td>Surat:</td><td>" + emailBody + "</td>";
                mailMessage.Body += "</tr>";
                mailMessage.Body += "</table>";
                mailMessage.Body += "</body>";
                mailMessage.Body += "</html>";

                mailMessage.IsBodyHtml = true;

                smtpClient.Port = Convert.ToInt32(strSmtpPort);
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new System.Net.NetworkCredential(fromEmail, passEmail);
                smtpClient.EnableSsl = false;

                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return result;
        }

        #endregion

        private static char[] _basechars =
            "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"
            .ToCharArray();

        private static Random _random = new Random();
        public string RndCode(int length)
        {
            var sb = new StringBuilder(length);

            for (int i = 0; i < length; i++)
                sb.Append(_basechars[_random.Next(36)]);

            return sb.ToString();
        }

        public interface IEncryptionService
        {
            string Encrypt(string plainText, string secret, string salt);
            string Decrypt(string cipherText, string secret, string salt);
        }

        public class EncryptionService : IEncryptionService
        {
            private const PaddingMode PaddingMode = System.Security.Cryptography.PaddingMode.PKCS7;

            public string Encrypt(string plainText, string secret, string salt)
            {
                var saltBytes = Encoding.UTF8.GetBytes(salt);
                using (var key = new Rfc2898DeriveBytes(secret, saltBytes))
                {
                    using (var aes = new RijndaelManaged())
                    {
                        aes.Key = key.GetBytes(aes.KeySize / 8);
                        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                        using (var msEncrypt = new MemoryStream())
                        {
                            aes.Padding = PaddingMode;
                            // prepend the IV
                            msEncrypt.Write(BitConverter.GetBytes(aes.IV.Length), 0, sizeof(int));
                            msEncrypt.Write(aes.IV, 0, aes.IV.Length);
                            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                            {
                                using (var swEncrypt = new StreamWriter(csEncrypt))
                                {
                                    swEncrypt.Write(plainText);
                                    swEncrypt.Flush();
                                }
                                csEncrypt.Flush();
                            }
                            msEncrypt.Flush();
                            return Convert.ToBase64String(msEncrypt.ToArray());

                        }
                    }
                }
            }

            public string Decrypt(string cipherText, string secret, string salt)
            {
                try
                {
                    var saltBytes = Encoding.UTF8.GetBytes(salt);
                    using (var key = new Rfc2898DeriveBytes(secret, saltBytes))
                    {
                        var toDecrypt = Convert.FromBase64String(cipherText);
                        using (var ms = new MemoryStream(toDecrypt))
                        {
                            using (var aes = new RijndaelManaged())
                            {
                                aes.Padding = PaddingMode;
                                aes.Key = key.GetBytes(aes.KeySize / 8);
                                //read prepended IV
                                aes.IV = ReadByteArray(ms);

                                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                                using (var csDecrypt = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                                using (var srDecrypt = new StreamReader(csDecrypt))
                                    return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
                catch
                {
                    return "";
                }
            }

            private static byte[] ReadByteArray(Stream stream)
            {
                const int intSize = sizeof(int);
                var ivLengthBytes = new byte[intSize];
                stream.Read(ivLengthBytes, 0, intSize);
                var ivLength = BitConverter.ToInt32(ivLengthBytes, 0);
                var bytes = new byte[ivLength];
                stream.Read(bytes, 0, ivLength);

                return bytes;
            }
        }

        public string TextEncode(string str)
        {
            try
            {
                string _result = HttpUtility.UrlEncode(str).Replace(".", "%2E");
                return _result;
            }
            catch (RegexMatchTimeoutException)
            {
                return String.Empty;
            }
        }

        public string bsreEncode(string str)
        {
            try
            {
                string _result = str.Replace("#", "%23").Replace("&", "%26");
                return _result;
            }
            catch (RegexMatchTimeoutException)
            {
                return String.Empty;
            }
        }

        public string HideEmail(string str)
        {
            try
            {
                var _email = str.Split('@');
                return string.Concat(_email[0].Length > 4 ? _email[0].Substring(0, 4) : _email[0].Substring(0, 1), "****@", _email[1]);
            }
            catch (RegexMatchTimeoutException)
            {
                return String.Empty;
            }
        }

        public bool isIp(string _ip)
        {
            try
            {
                bool _result = false;
                if (!string.IsNullOrEmpty(_ip))
                {
                    if (!_ip.Equals("::1"))
                    {
                        var _part = _ip.Split('.');
                        if(_part.Count() == 4)
                        {
                            _result = true;
                        }
                    }
                }
                return _result;
            }
            catch
            {
                return false;
            }
        }

        public string getIpHeader(string ip)
        {
            try
            {
                var _ip = ip.Split('.');
                return string.Concat(_ip[0], ".", _ip[1], ".");
            }
            catch (RegexMatchTimeoutException)
            {
                return String.Empty;
            }
        }

        public double getDistance(string pLat, string pLon, string kLat, string kLon)
        {
            double result = 0;
            try
            {
                var _radius = 6371000;
                var dLat = toRad(double.Parse(pLat.Replace(".", ",")) - double.Parse(kLat.Replace(".", ",")));
                var dLon = toRad(double.Parse(pLon.Replace(".", ",")) - double.Parse(kLon.Replace(".", ",")));
                var lat1 = toRad(double.Parse(kLat.Replace(".", ",")));
                var lat2 = toRad(double.Parse(pLat.Replace(".", ",")));

                var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
                var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                var d = _radius * c;
                result = Math.Round(Math.Abs(d));
            }
            catch (RegexMatchTimeoutException)
            {
                result = 0;
            }
            return result;
        }

        private double toRad(double val)
        {
            return val * Math.PI / 180;
        }

        public string createQR(string str, bool useLogo)
        {
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(str, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrCodeData);
            var qrCodeImage = qrCode.GetGraphic(20);
            byte[] byteGambar = null;
            if (useLogo)
            {
                var logo = System.Drawing.Image.FromFile(string.Concat(System.Web.Hosting.HostingEnvironment.MapPath("\\"),"Reports\\logoqr_small.png"));
                var destRect = new System.Drawing.Rectangle(0, 0, 150, 150);
                var destImage = new System.Drawing.Bitmap(150, 150);
                destImage.SetResolution(logo.HorizontalResolution, logo.VerticalResolution);
                using (var graphics = System.Drawing.Graphics.FromImage(destImage))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    using (var wrapMode = new ImageAttributes())
                    {
                        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                        graphics.DrawImage(logo, destRect, 0, 0, logo.Width, logo.Height, System.Drawing.GraphicsUnit.Pixel, wrapMode);
                    }
                }
                logo = destImage;

                int left = (qrCodeImage.Width / 2) - ((int)logo.Width / 2);
                int top = (qrCodeImage.Height / 2) - ((int)logo.Height / 2);
                var g = System.Drawing.Graphics.FromImage(qrCodeImage);
                g.DrawImage(logo, new System.Drawing.Point(left, top));
            }
            try
            {
                using (var stream = new MemoryStream())
                {
                    qrCodeImage.Save(stream, ImageFormat.Bmp);
                    byteGambar = stream.ToArray();
                }
            }
            catch (ArgumentNullException ex)
            {
                throw ex;
            }
            return "data:image/png;base64, " + Convert.ToBase64String(byteGambar);
        }

        public string imgstring(string str)
        {
            var encoder = new QRCodeEncoder();
            byte[] byteGambar = null;
            var img = encoder.Encode(str);
            var logo = System.Drawing.Image.FromFile(string.Concat(System.Web.Hosting.HostingEnvironment.MapPath("\\"), "Reports\\logoqr_small.png"));

            var destRect = new System.Drawing.Rectangle(0, 0, 72, 72);
            var destImage = new System.Drawing.Bitmap(72, 72);
            destImage.SetResolution(logo.HorizontalResolution, logo.VerticalResolution);
            using (var graphics = System.Drawing.Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(logo, destRect, 0, 0, logo.Width, logo.Height, System.Drawing.GraphicsUnit.Pixel, wrapMode);
                }
            }
            logo = destImage;

            int left = (img.Width / 2) - ((int)logo.Width / 2);
            int top = (img.Height / 2) - ((int)logo.Height / 2);
            var g = System.Drawing.Graphics.FromImage(img);
            g.DrawImage(logo, new System.Drawing.Point(left, top));
            using (var memoryStream = new MemoryStream())
            {
                img.Save(memoryStream, ImageFormat.Jpeg);
                byteGambar = memoryStream.ToArray();
            }
            return "data:image/png;base64, " + Convert.ToBase64String(byteGambar);
        }
    }

    public class Security
    {
        public static bool cekWhitelist(string url)
        {
            bool _rst = false;
            string[] wl = ConfigurationManager.AppSettings["PageWhitelist"].ToString().Split('|');
            if (string.IsNullOrEmpty(url))
            {
                _rst = true;
                return _rst;
            }
            foreach (string w in wl)
            {
                if (url.Contains(w))
                {
                    _rst = true;
                }
            }

            return _rst;
        }
    }

    public class Kafka
    {
        private static async Task PostBasicAsync(object content, string vUserName, string vEmail, string vAct, string vTarget, CancellationToken cancellationToken)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Post, "https://applog.atrbpn.go.id/producer/publish-message/atrbpnlog"))
            {
                var json = JsonConvert.SerializeObject(new
                {
                    app = "Web Eoffice",
                    version = "1.0.3",
                    username = vUserName,
                    email = vEmail,
                    type = vTarget,
                    contentId = "",
                    description = vAct,
                    platform = "",
                    device = "",
                    ipAddress = "",
                    userAgent = ""
                });
                using (var stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    request.Content = stringContent;

                    using (var response = await client
                        .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                        .ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();
                    }
                }
            }
        }
    }

    public class link
    {
        public static bool CheckLink(string url)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "HEAD";
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                return false;
            }
        }
    }

    public class Mobile
    {
        public void KirimNotifikasi(string vNipTujuan, string vTipeUser, string vNamaPengirim, string vPesan, string vJenis)
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://10.20.21.21:3000/fcm/api/sendfcm");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    var json = JsonConvert.SerializeObject(new
                    {
                        tujuan = vNipTujuan,
                        tipe = vTipeUser,
                        pengirim = vNamaPengirim,
                        pesan = vPesan,
                        jenis = vJenis
                    });
                    streamWriter.Write(json);
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                }
            }
            catch(Exception ex)
            {
                string msg = ex.Message;
            }
        }
        
        public void KirimBroadcast(string vTarget, string vJudul, string vKonten, string vIcon = null, string vImage = null, string vUrl = null)
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers.Add("Authorization", "key=AAAAVruMTXc:APA91bFLbSbt39-qsVT0-bcPoa1dBhTVZ1F310rr2G4osDurt23cSdN0rUz7a7UnSNOw2RSW2iok8mQEex4DUNZeIsr2kcrA-EraQoUWfsy8aEMATVDvIysv93cQBoDAVN5IYPHMNRLt");

                vJudul = HttpUtility.UrlDecode(vJudul).Replace("\"", "'");
                vKonten = HttpUtility.UrlDecode(vKonten).Replace("\"","'");

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    JObject vData = JObject.FromObject(new { });
                    JObject vJson = JObject.FromObject(new { });
                    vJson.Add("to", string.Concat("/topics/", vTarget.Equals("All") ? "eofficenotification" : string.Concat("unitkerja_", vTarget)));
                    vJson.Add("icon", string.IsNullOrEmpty(vIcon) ? "" : vIcon);
                    JObject vNotif = JObject.FromObject(new { });
                    vNotif.Add("title", vJudul);
                    vNotif.Add("body", vKonten);
                    if (!string.IsNullOrEmpty(vImage))
                    {
                        vImage = HttpUtility.UrlDecode(vImage);
                        vNotif.Add("image", vImage);
                    }
                    vNotif.Add("click_action", "FLUTTER_NOTIFICATION_CLICK");
                    vNotif.Add("android_channel_id", "noti_push_app_1");
                    vJson.Add("notification", vNotif);

                    if (!string.IsNullOrEmpty(vImage))
                    {
                        vData.Add("action", "images");
                        vData.Add("url-image", vImage);
                        vJson.Add("data", vData);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(vUrl))
                        {
                            vUrl = HttpUtility.UrlDecode(vUrl);
                            vData.Add("action", "open-url");
                            vData.Add("title", vJudul);
                            vData.Add("url", vUrl);
                            vJson.Add("data", vData);
                        }
                    }

                    vJson.Add("content_available", true);
                    vJson.Add("priority", "high");

                    //var json = JsonConvert.SerializeObject(new
                    //{
                    //    to = string.Concat("/topics/", vTarget.Equals("All") ? "eofficenotification" : string.Concat("unitkerja_",vTarget)),
                    //    icon = string.IsNullOrEmpty(vIcon) ? "" : vIcon,
                    //    notification = new
                    //    {
                    //        title = vJudul,
                    //        body = vKonten,
                    //        image = string.IsNullOrEmpty(vImage) ? "" : vImage,
                    //        click_action = "FLUTTER_NOTIFICATION_CLICK",
                    //        android_channel_id = "noti_push_app_1"
                    //    },
                    //    data = vData,
                    //    content_available = true,
                    //    priority = "high"
                    //});
                    streamWriter.Write(vJson.ToString());
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
        }
    }

    public class DigitalSignature
    {
        public TransactionResult validasiUploadV2(HttpPostedFileBase file)
        {
            var result = new TransactionResult() { Status = true, Pesan = "" };
            PdfReader pdfreader = null;

            try
            {
                pdfreader = new PdfReader(new ReaderProperties().SetPartialRead(true), file.InputStream);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Pesan = "Gagal membaca file pdf";
                return result;
            }
            AcroFields fields = pdfreader.AcroFields;
            AcroFields.Item item = fields.GetFieldItem("Signature1");
            List<string> names = fields.GetSignatureNames();
            if (names.Count == 0)
            {
                result.Status = false;
                result.Pesan = "File ini belum memiliki Stempel Digital";
            }
            else
            {
                foreach (string name in names)
                {
                    PdfPKCS7 pk = fields.VerifySignature(name);
                    var cal = pk.SignDate;
                    var pkc = pk.Certificates;

                    if (!pk.Verify())
                    {
                        result.Status = false;
                        result.Pesan = "Stempel Digital tidak valid";
                        break;
                    }
                    if (!pk.VerifyTimestampImprint())
                    {
                        result.Status = false;
                        result.Pesan = "Tanggal Stempel Digital tidak valid";
                        break;
                    }
                    if (pkc == null)
                    {
                        result.Status = false;
                        result.Pesan = "Sertifikasi Stempel Digital tidak temukan";
                        break;
                    }
                }
            }
            return result;
        }
        public TransactionResult validasiStream(Stream file)
        {
            var result = new TransactionResult() { Status = true, Pesan = "" };
            PdfReader pdfreader = null;

            try
            {
                pdfreader = new PdfReader(new ReaderProperties().SetPartialRead(true), file);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Pesan = "Gagal membaca file pdf";
                return result;
            }
            AcroFields fields = pdfreader.AcroFields;
            AcroFields.Item item = fields.GetFieldItem("Signature1");
            List<string> names = fields.GetSignatureNames();
            if (names.Count == 0)
            {
                result.Status = false;
                result.Pesan = "File ini belum memiliki Stempel Digital";
            }
            else
            {
                foreach (string name in names)
                {
                    PdfPKCS7 pk = fields.VerifySignature(name);
                    var cal = pk.SignDate;
                    var pkc = pk.Certificates;

                    if (!pk.Verify())
                    {
                        result.Status = false;
                        result.Pesan = "Stempel Digital tidak valid";
                        break;
                    }
                    //if (!pk.VerifyTimestampImprint())
                    //{
                    //    result.Status = false;
                    //    result.Pesan = "Tanggal Stempel Digital tidak valid";
                    //    break;
                    //}
                    if (pkc == null)
                    {
                        result.Status = false;
                        result.Pesan = "Sertifikasi Stempel Digital tidak temukan";
                        break;
                    }
                }
            }
            pdfreader.Close();
            return result;
        }

        public TransactionResult validasiUpload(HttpPostedFileBase file)
        {
            var result = new TransactionResult() { Status = true, Pesan = "" };

            // pdf = new byte[file.ContentLength];
            BinaryReader rdr = new BinaryReader(file.InputStream);
            byte[] pdf = rdr.ReadBytes((int)file.ContentLength);
            file.InputStream.Read(pdf, 0, pdf.Length);

            PdfReader pdfreader = null;

            try
            {
                pdfreader = new PdfReader(pdf);
            }
            catch (Exception ex)
            {
                result.Status = true;
                result.Pesan = "Gagal membaca file pdf";
                return result;
            }
            AcroFields fields = pdfreader.AcroFields;
            AcroFields.Item item = fields.GetFieldItem("Signature1");
            List<string> names = fields.GetSignatureNames();
            if (names.Count == 0)
            {
                result.Status = false;
                result.Pesan = "File ini belum memiliki Stempel Digital";
            }
            else
            {
                foreach (string name in names)
                {
                    PdfPKCS7 pk = fields.VerifySignature(name);
                    var cal = pk.SignDate;
                    var pkc = pk.Certificates;

                    if (!pk.Verify())
                    {
                        result.Status = false;
                        result.Pesan = "Stempel Digital tidak valid";
                        break;
                    }
                    if (!pk.VerifyTimestampImprint())
                    {
                        result.Status = false;
                        result.Pesan = "Tanggal Stempel Digital tidak valid";
                        break;
                    }
                    if (pkc == null)
                    {
                        result.Status = false;
                        result.Pesan = "Sertifikasi Stempel Digital tidak temukan";
                        break;
                    }
                }
            }
            return result;
        }

        public TransactionResult validasiKonten(Stream strm)
        {
            var result = new TransactionResult() { Status = true, Pesan = "" };

            MemoryStream _ms = (MemoryStream)strm;
            byte[] pdf = _ms.ToArray();
            PdfReader pdfreader = new PdfReader(pdf);
            AcroFields fields = pdfreader.AcroFields;
            AcroFields.Item item = fields.GetFieldItem("Signature1");
            List<string> names = fields.GetSignatureNames();
            if (names.Count == 0)
            {
                result.Status = false;
                result.Pesan = "File ini belum memiliki Stempel Digital";
            }
            else
            {
                foreach (string name in names)
                {
                    PdfPKCS7 pk = fields.VerifySignature(name);
                    var cal = pk.SignDate;
                    var pkc = pk.Certificates;

                    if (!pk.Verify())
                    {
                        result.Status = false;
                        result.Pesan = "Stempel Digital tidak valid";
                        break;
                    }
                    if (!pk.VerifyTimestampImprint())
                    {
                        result.Status = false;
                        result.Pesan = "Tanggal Stempel Digital tidak valid";
                        break;
                    }
                    if (pkc == null)
                    {
                        result.Status = false;
                        result.Pesan = "Sertifikasi Stempel Digital tidak temukan";
                        break;
                    }
                }
            }
            return result;
        }
    }

    public class CustomCatchORA
    {
        public string GetErrorMessage(string ex)
        {
            string msg = ex;

            if (ex.Substring(0, 9) == "ORA-00904")
            {
                msg = "Field tidak dikenali, Silahkan Laporkan ke Administrator.";
            }
            else if (ex.Substring(0, 9) == "ORA-01722")
            {
                msg = "Tipe data tidak sesuai, Silahkan Laporkan ke Administrator.";
            }
            else if (ex.Substring(0, 9) == "ORA-01400")
            {
                msg = "Terdapat data \"Null\", Silahkan Laporkan ke Administrator.";
            }
            else if (ex.Substring(0, 9) == "ORA-00054")
            {
                msg = "Proses terputus, Silahkan coba kembali.";
            }
            else if (ex.Substring(0, 9) == "ORA-01422")
            {
                msg = "Kesalahan pengambilan jumlah data, Silahkan Laporkan ke Administrator.";
            }
            else if (ex.Substring(0, 9) == "ORA-06512")
            {
                msg = "Query tidak sesuai, Silahkan Laporkan ke Administrator.";
            }
            else if (ex.Substring(0, 9) == "ORA-04098")
            {
                msg = "Kesalahan menjalankan trigger, Silahkan Laporkan ke Administrator.";
            }
            else
            {
                msg = "Terjadi kesalahan. Silahkan Laporkan ke Administrator.";
            }

            return msg;
        }
    }
}

