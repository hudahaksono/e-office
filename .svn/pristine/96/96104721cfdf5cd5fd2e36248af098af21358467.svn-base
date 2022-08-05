using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFEditor.Helper
{
    public static class FontHelper
    {

        public enum FONT
        {
            BookmanOldStyle_Regular,
            FreestyleScript_Regular
        }

        public static string GetFont(FONT font)
        {
            var value = "";
            switch (font)
            {
                case FONT.BookmanOldStyle_Regular:
                    value = BookmanOldStyle();
                    break;
                case FONT.FreestyleScript_Regular:
                    value = FreestyleScriptRegular();
                    break;
            }
            return value;
        }

        public static string BookmanOldStyle() {
            return Path.Combine(Constant.GetResourceDir(), "Bookman Old Style", "Bookman Old Style Regular.ttf");
        }

        public static string FreestyleScriptRegular()
        {
            return Path.Combine(Constant.GetResourceDir(), "Freestyle Script", "Freestyle Script Regular.ttf");
        }
    }
}
