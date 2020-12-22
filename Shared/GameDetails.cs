using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POT.Pexeso.Shared
{
    public enum BoardSize
    {
        [Description("3x2")]
        _3x_2,
        [Description("4x3")]
        _4x_3,
        [Description("4x4")]
        _4x_4,
        [Description("5x4")]
        _5x_4,
        [Description("6x5")]
        _6x_5,
        [Description("6x6")]
        _6x_6,
        [Description("7x6")]
        _7x_6,
        [Description("8x7")]
        _8x_7,
        [Description("8x8")]
        _8x_8
    }

    public enum CardType
    {
        Color,
        Picture
    }

    public class GameDetails
    {
        public static readonly int ResponseDelay = 15;
        public BoardSize Board { get; set; }
        public CardInfo Card { get; set; }

        public static string FormatBoardEnum(BoardSize size)
        {
            return size.ToString().Replace("_", "");
        }
    }

    
}
