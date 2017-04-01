using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gif.Components;
using System.Drawing;
using System.Drawing.Imaging;

namespace MyFunctions
{
    class GifAnimation
    {
        public GifAnimation()
        {
            String[] imageFilePaths = new String[] { "D:\\Programming\\BOT Platform\\BOT Platform\\bin\\Debug\\3.jpg",
                "D:\\Programming\\BOT Platform\\BOT Platform\\bin\\Debug\\in_mem.jpg"
            };
            String outputFilePath = "D:\\Programming\\BOT Platform\\BOT Platform\\bin\\Debug\\test.gif";
            AnimatedGifEncoder e = new AnimatedGifEncoder();
            //e.Start(outputFilePath);
            //e.SetDelay(500);
            //-1:no repeat,0:always repeat
            //e.SetRepeat(0);
            //for (int i = 0, count = imageFilePaths.Length; i < count; i++)
            //{
            //    e.AddFrame(Image.FromFile(imageFilePaths[i]));
            //}
            //e.Finish();
        }
    }
}
