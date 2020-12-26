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

    public class GameSettings
    {
        public static readonly int ResponseDelay = 15;
        public static readonly int CardShowDelay = 2;
        public static readonly int GameoverDelay = 3;
        public BoardSize BoardSize { get; set; }
        public CardBackInfo CardBack { get; set; }

        public static string FormatBoardEnum(BoardSize size)
        {
            return size.ToString().Replace("_", "");
        }

        public static void ParseWidhtAndHeight(BoardSize size, out int widht, out int height)
        {
            var stringDimensions = FormatBoardEnum(size);
            var dimensions = stringDimensions.Split("x", StringSplitOptions.RemoveEmptyEntries);
            widht = int.Parse(dimensions[0]);
            height = int.Parse(dimensions[1]);
        }
    }
    
}
