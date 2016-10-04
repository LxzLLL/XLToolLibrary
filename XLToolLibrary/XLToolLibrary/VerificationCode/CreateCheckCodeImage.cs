using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XLToolLibrary.Utilities
{
    /// <summary>
    /// 生成图片验证码
    /// </summary>
    public static class CreateCheckCodeImage
    {
        /// <summary>
        /// 在内存中生成一个验证图片
        /// </summary>
        /// <returns>System.IO.MemoryStream</returns>
        public static MemoryStream Production( string StrCode )
        {
            MemoryStream ms = new MemoryStream();
            if ( string.Equals( StrCode, string.Empty ) )
            {
                return ms;
            }

            Bitmap image = new Bitmap((int)System.Math.Ceiling(StrCode.Length * 16.5), 22);
            Graphics g = Graphics.FromImage(image);

            try
            {
                System.Random random = new Random();
                g.Clear( Color.White );

                //生成背景干扰点
                for ( int i = 0 ; i < 25 ; i++ )
                {
                    int x1 = random.Next(image.Width);
                    int x2 = random.Next(image.Width);
                    int y1 = random.Next(image.Height);
                    int y2 = random.Next(image.Height);
                    g.DrawLine( new Pen( Color.Silver ), x1, x2, y1, y2 );
                }

                //生成验证字符
                Font font = new System.Drawing.Font("Arial", 16, (System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic));
                System.Drawing.Drawing2D.LinearGradientBrush brush = new System.Drawing.Drawing2D.LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height), Color.Blue, Color.DarkRed, 1.2f, true);
                g.DrawString( StrCode, font, brush, 2, 1 );

                //生成前景干扰点
                for ( int i = 0 ; i < 100 ; i++ )
                {
                    int x1 = random.Next(image.Width);
                    int y1 = random.Next(image.Height);

                    image.SetPixel( x1, y1, Color.FromArgb( random.Next() ) );
                }

                //化图片的边框
                g.DrawRectangle( new Pen( Color.Silver ), 0, 0, image.Width - 1, image.Height - 1 );

                //TwisImaige
                //TwisImaige(image, true,2.5, 0.2).Save(ms,ImageFormat.Gif);    //目前没有使用
                image.Save( ms, ImageFormat.Gif );

                return ms;
            }
            finally
            {
                g.Dispose();
                image.Dispose();
            }
        }

        /// <summary>
        /// 随即生成一段字符串，用作生成随即图片
        /// </summary>
        /// <returns>string</returns>
        public static string GenerateCheckCode(int number)
        {
            if (number <= 0)
            {
                number = 4;
            }
            string checkCode = string.Empty;
            System.Random random = new System.Random();
            int numTemp; 
            for ( int i = 0 ; i < number ; i++ )
            {
                char code;
                numTemp = random.Next();
                if ( numTemp % 2 == 0 )
                {
                    code = ( char )( '0' + ( char )( numTemp % 10 ) );
                }
                else
                {
                    code = ( char )( 'A' + ( char )( numTemp % 26 ) );
                }
                checkCode += code.ToString();
            }
            checkCode = checkCode.Replace( "0", "F" );
            checkCode = checkCode.Replace( "O", "F" );
            return checkCode;
        }

        /// <summary>
        /// 正弦曲线Wave扭曲图片
        /// </summary>
        /// <param name="srcBmp">图片</param>
        /// <param name="bXDir">如果扭曲选择为True</param>
        /// <param name="dMultValue">波形的幅度倍数，越大扭曲的程度越高，一般为3</param>
        /// <param name="dPhase">波形的起始位置，取值区间[0.2*PI]</param>
        /// <returns></returns>
        private static Bitmap TwisImaige( Bitmap srcBmp, bool bXDir, double dMultValue, double dPhase )
        {
            const double PI2 = 3.14 * 2;
            Bitmap destBmp = new Bitmap(srcBmp.Width, srcBmp.Height);
            Graphics g = Graphics.FromImage(destBmp);
            g.FillRectangle( new SolidBrush( Color.White ), 0, 0, destBmp.Width, destBmp.Height );
            g.Dispose();

            double dBaseAxisLen = bXDir ? (double)destBmp.Height : (double)destBmp.Width;
            try
            {
                for ( int i = 0 ; i < destBmp.Width ; i++ )
                {
                    for ( int j = 0 ; j < destBmp.Height ; j++ )
                    {
                        double dx = 0;
                        dx = bXDir ? ( PI2 * ( double )j ) / dBaseAxisLen : ( PI2 * ( double )i ) / dBaseAxisLen;
                        dx += dPhase;
                        double dy = System.Math.Sin(dx);

                        //取得当前点的颜色
                        int nOldX = 0, nOldY = 0;
                        nOldX = bXDir ? i + ( int )( dy * dMultValue ) : i;
                        nOldY = bXDir ? j + ( int )( dy * dMultValue ) : j;

                        Color color = srcBmp.GetPixel(i, j);
                        if ( nOldX >= 0 && nOldX < destBmp.Width && nOldY >= 0 && nOldY < destBmp.Height )
                        {
                            destBmp.SetPixel( nOldX, nOldY, color );
                        }
                    }
                }
                return destBmp;
            }
            catch ( Exception e )
            {
                string eStr = e.Message;
                return destBmp;
            }
        }
    }
}