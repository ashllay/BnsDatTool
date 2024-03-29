﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BnsDatTool.lib
{
    public static class CharExtension
    {
		public static bool IsChineseChar(this char elem)
		{
			return elem >= '\u4E00' && elem <= '\u9FFF';
		}
        public static bool IsJapanChar(this char elem)
        {
            return (elem >= '\u3040' && elem <= '\u309F') || (elem >= '\u30A0' && elem <= '\u30FF');
        }
        public static bool IsKoreaChar(this char elem)
        {
            return (elem >= '\u3130' && elem <= '\u318F')||(elem >= '\uAC00' && elem <= '\uD7AF');
        }
        public static bool IsThaiChar(this char elem)
        {
            return elem >= '\u0E00' && elem <= '\u0E7F';
        }
        public static bool IsVietChar(this char elem)
        {
            return (elem >= '\u00C0' && elem <= '\u00FF')|| (elem >= '\u1EA0' && elem <= '\u1EFF');
        }
    }
}
