using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Drawing;

namespace Surat.Codes
{
    public class PdfUtil
    {
        public static iTextSharp.text.Font GetFranklinGothicCond(float fontsize, int fontstyle)
        {
            var fontName = "Franklin Gothic Medium Cond";
            if (!FontFactory.IsRegistered(fontName))
            {
                var fontPath = Environment.GetEnvironmentVariable("SystemRoot") + "\\fonts\\FRAMDCN.TTF";
                FontFactory.Register(fontPath);
            }
            return FontFactory.GetFont(fontName, BaseFont.IDENTITY_H, BaseFont.EMBEDDED, fontsize, fontstyle);
        }

        public static iTextSharp.text.Font GetBookmanOldStyle(float fontsize, int fontstyle)
        {
            var fontName = "Bookman Old Style";
            if (!FontFactory.IsRegistered(fontName))
            {
                var fontPath = Environment.GetEnvironmentVariable("SystemRoot") + "\\fonts\\BOOKOS.TTF";
                FontFactory.Register(fontPath);
            }
            return FontFactory.GetFont(fontName, BaseFont.IDENTITY_H, BaseFont.EMBEDDED, fontsize, fontstyle);
        }

        public static iTextSharp.text.Font GetCambria(float fontsize, int fontstyle)
        {
            var fontName = "Cambria";
            if (!FontFactory.IsRegistered(fontName))
            {
                var fontPath = Environment.GetEnvironmentVariable("SystemRoot") + "\\fonts\\cambria.ttc";
                FontFactory.Register(fontPath);
            }
            return FontFactory.GetFont(fontName, BaseFont.IDENTITY_H, BaseFont.EMBEDDED, fontsize, fontstyle);
        }

        public static iTextSharp.text.Font GetArial(float fontsize, int fontstyle)
        {
            var fontName = "Arial";
            if (!FontFactory.IsRegistered(fontName))
            {
                var fontPath = Environment.GetEnvironmentVariable("SystemRoot") + "\\fonts\\arial.ttf";
                FontFactory.Register(fontPath);
            }
            return FontFactory.GetFont(fontName, BaseFont.IDENTITY_H, BaseFont.EMBEDDED, fontsize, fontstyle);
        }

        public static iTextSharp.text.Font GetTimes(float fontsize, int fontstyle)
        {
            var fontName = "Arial";
            if (!FontFactory.IsRegistered(fontName))
            {
                var fontPath = Environment.GetEnvironmentVariable("SystemRoot") + "\\fonts\\times.ttf";
                FontFactory.Register(fontPath);
            }
            return FontFactory.GetFont(fontName, BaseFont.IDENTITY_H, BaseFont.EMBEDDED, fontsize, fontstyle);
        }

        public static iTextSharp.text.Font GetArialNarrow(float fontsize, int fontstyle)
        {
            var fontName = "Arial Narrow";
            if (!FontFactory.IsRegistered(fontName))
            {
                var fontPath = Environment.GetEnvironmentVariable("SystemRoot") + "\\fonts\\arialn.ttf";
                FontFactory.Register(fontPath);
            }
            return FontFactory.GetFont(fontName, BaseFont.IDENTITY_H, BaseFont.EMBEDDED, fontsize, fontstyle);
        }

        public static iTextSharp.text.Font GetFont(string fontname, string filename, float fontsize, int fontstyle)
        {
            var fontName = fontname;
            if (!FontFactory.IsRegistered(fontName))
            {
                var fontPath = Environment.GetEnvironmentVariable("SystemRoot") + "\\fonts\\" + filename;
                FontFactory.Register(fontPath);
            }
            return FontFactory.GetFont(fontName, BaseFont.IDENTITY_H, BaseFont.EMBEDDED, fontsize, fontstyle);
        }

        public Paragraph HorizontalLine()
        {
            Paragraph line = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));
            line.SpacingBefore = 0;
            line.SpacingAfter = 0;
            line.FirstLineIndent = 0;
            line.SetLeading(5f, 0);
            return line;
        }

        public Paragraph AddTitleSurat(string text, int alignment, float fontsize, int fontweight, float spacingbefore, float spacingafter, float firstlineindent, string fontname = null)
        {
            Paragraph p = new Paragraph();
            p.SpacingBefore = spacingbefore;
            p.SpacingAfter = spacingafter;
            p.FirstLineIndent = firstlineindent;

            iTextSharp.text.Font fontTitle = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, fontsize, fontweight);
            if (!string.IsNullOrEmpty(fontname))
            {
                if (fontname == "Franklin Gothic Medium Cond")
                {
                    fontTitle = GetFranklinGothicCond(fontsize, fontweight);
                }
                else if (fontname == "Bookman Old Style")
                {
                    fontTitle = GetBookmanOldStyle(fontsize, fontweight);
                }
                else if (fontname == "Cambria")
                {
                    fontTitle = GetCambria(fontsize, fontweight);
                }
                else if (fontname == "Arial")
                {
                    fontTitle = GetArial(fontsize, fontweight);
                }
                else if (fontname == "Arial Narrow")
                {
                    fontTitle = GetArialNarrow(fontsize, fontweight);
                }
            }
            p.Font = fontTitle;
            p.Alignment = alignment;
            p.Add(text);
            return p;
        }

        public Paragraph AddTitleSuratUnderline(string text, int alignment, float fontsize, float spacingbefore, float spacingafter, float firstlineindent)
        {
            Paragraph p = new Paragraph();
            p.SpacingBefore = spacingbefore;
            p.SpacingAfter = spacingafter;
            p.FirstLineIndent = firstlineindent;

            iTextSharp.text.Font fontTitle = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, fontsize, iTextSharp.text.Font.BOLD | iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);

            p.Font = fontTitle;
            p.Alignment = alignment;
            p.Add(text);
            return p;
        }

        public Paragraph AddLineSeparator(float lineheight)
        {
            Paragraph p = new Paragraph();
            p.SpacingBefore = lineheight;
            p.Add("");
            return p;
        }

        public Paragraph AddParagraph(float fontsize, float fixedleading, float multipliedleading, int fontstyle, float spacingbefore, float spacingafter, float firstlineindent, float indentation, int alignment, string text, string fontname)
        {
            Paragraph p = new Paragraph();
            p.SpacingBefore = spacingbefore;
            p.SpacingAfter = spacingafter;
            p.FirstLineIndent = firstlineindent;
            p.IndentationLeft = indentation;
            p.IndentationRight = indentation;
            p.SetLeading(fixedleading, multipliedleading);

            iTextSharp.text.Font font = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, fontsize, fontstyle);

            if (fontname == "Franklin Gothic Medium Cond")
            {
                font = GetFranklinGothicCond(fontsize, fontstyle);
            }
            else if (fontname == "Bookman Old Style")
            {
                font = GetBookmanOldStyle(fontsize, fontstyle);
            }
            else if (fontname == "Cambria")
            {
                font = GetCambria(fontsize, fontstyle);
            }
            else if (fontname == "Arial")
            {
                font = GetArial(fontsize, fontstyle);
            }

            p.Font = font;
            p.Alignment = alignment;
            p.Add(text);
            return p;
        }

        //public PdfPCell CreateCell(string content, float borderWidth, int colspan, int alignment)
        //{
        //    PdfPCell cell = new PdfPCell(new Phrase(content));
        //    cell.BorderWidth = borderWidth;
        //    cell.Colspan = colspan;
        //    cell.HorizontalAlignment = alignment;
        //    return cell;
        //}

        //public PdfPCell CreateCell(string content, int alignment)
        //{
        //    PdfPCell cell = new PdfPCell(new Phrase(content));
        //    cell.HorizontalAlignment = alignment;
        //    return cell;
        //}

        public PdfPCell CreateCell(string content)
        {
            return CreateCell(content, Element.ALIGN_CENTER, Element.ALIGN_MIDDLE, 0f, 10f, iTextSharp.text.Font.NORMAL);
        }

        public PdfPCell CreateCell(string content, int alignmentvertical)
        {
            return CreateCell(content, Element.ALIGN_CENTER, alignmentvertical, 0f, 10f, iTextSharp.text.Font.NORMAL);
        }

        public PdfPCell CreateCell(string content, int alignmentvertical, int fontstyle)
        {
            return CreateCell(content, Element.ALIGN_CENTER, alignmentvertical, 0f, 10f, fontstyle);
        }

        public PdfPCell CreateCell(string content, int alignmentvertical, float fontsize, int fontstyle)
        {
            return CreateCell(content, Element.ALIGN_CENTER, alignmentvertical, 0f, fontsize, fontstyle);
        }

        public PdfPCell CreateCell(string content, int alignment, int alignmentvertical, float padding, float fontsize, int fontstyle)
        {
            iTextSharp.text.Font font = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, fontsize, fontstyle);
            PdfPCell cell = new PdfPCell(new Phrase(content, font));
            cell.HorizontalAlignment = alignment; // Element.ALIGN_CENTER;
            cell.VerticalAlignment = alignmentvertical; // Element.ALIGN_MIDDLE;
            cell.Padding = 5f;
            return cell;
        }

        public PdfPCell CreateCellTable(string content, int alignment, int alignmentvertical, float fixedheight, float fontsize, int fontstyle, bool noborder, string fontname)
        {
            iTextSharp.text.Font font = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, fontsize, fontstyle);
            if (fontname == "Franklin Gothic Medium Cond")
            {
                font = GetFranklinGothicCond(fontsize, fontstyle);
            }
            else if (fontname == "Bookman Old Style")
            {
                font = GetBookmanOldStyle(fontsize, fontstyle);
            }
            else if (fontname == "Cambria")
            {
                font = GetCambria(fontsize, fontstyle);
            }
            else if (fontname == "Arial")
            {
                font = GetArial(fontsize, fontstyle);
            }

            PdfPCell cell = new PdfPCell(new Phrase(content, font));
            cell.HorizontalAlignment = alignment;
            cell.VerticalAlignment = alignmentvertical; //Element.ALIGN_MIDDLE;
            //cell.FixedHeight = fixedheight;
            //cell.Padding = 5f;
            if (fixedheight > 0)
            {
                cell.FixedHeight = fixedheight;
            }
            if (!noborder)
            {
                cell.Padding = 5f;
            }
            if (noborder)
            {
                cell.Border = 0;
            }
            return cell;
        }

        public PdfPCell CreateCell(Phrase phrase)
        {
            PdfPCell cell = new PdfPCell(phrase);
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.Padding = 8f;
            return cell;
        }

        public PdfPCell CreateCellLeft(Phrase phrase)
        {
            PdfPCell cell = new PdfPCell(phrase);
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            cell.Padding = 8f;
            return cell;
        }

        public PdfPCell CreateCellRight(Phrase phrase)
        {
            PdfPCell cell = new PdfPCell(phrase);
            cell.HorizontalAlignment = Element.ALIGN_RIGHT;
            cell.Padding = 8f;
            return cell;
        }

        public PdfPCell GetCell(int cm)
        {
            PdfPCell cell = new PdfPCell();
            cell.Colspan = cm;
            cell.UseAscender = true; //cell.setUseAscender(true);
            cell.UseDescender = true; //cell.setUseDescender(true);
            Paragraph p = new Paragraph(
                    string.Format("%smm", 10 * cm),
                    new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8));
            p.Alignment = Element.ALIGN_CENTER;
            cell.AddElement(p);
            return cell;
        }

    }

}