using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;
using System.IO;
using Q42.HueApi.Interfaces;
using Q42.HueApi.Extensions;
using Q42.HueApi.Models.Bridge;
using Q42.HueApi;
using Q42.HueApi.ColorConverters.Original;

namespace HueSync_2022
{
    class Program
    {
        static void Main(string[] args)
        {


            RunProgram();
            

            








        }

        static async void RunProgram()
        {
            bool Running = true;
            ILocalHueClient client = new LocalHueClient("192.168.1.82");
            client.Initialize("S8PiGPDDARx8vRVklq8H9siZvAgMRByKW758wEcq");
            Console.WriteLine("Client Initiliazed");
            do
            {
                var command = new LightCommand();
                Task<Color> screen = new Task<Color>(() => ScreenShotAndSendColor());
                screen.Start();





                Console.WriteLine("Setting color");
                //Awaits a task, when i remove the awaits the program runs fine, but when i await these tasks it exits the while loop and ends.
                Color clr = await screen;
                string hex = HexConverter(clr);
                command.SetColor(new Q42.HueApi.ColorConverters.RGBColor(hex));

                Console.WriteLine("Sending Command to client");
                //Awaits a task
                await client.SendCommandAsync(command, new List<string> { "13" });
                Console.WriteLine("Command Sent!");
                Thread.Sleep(50);
            }
            while (Running);
            
               
                
                   
                
                
            

            
        }

        private static String HexConverter(System.Drawing.Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }


        static Color ScreenShotAndSendColor()
        {
            Bitmap imgMap = new Bitmap(1920, 1080);
            Size s = new Size(imgMap.Width, imgMap.Height);

            Graphics graphics = Graphics.FromImage(imgMap);

            graphics.CopyFromScreen(0, 0, 0, 0, s);
            Color avrgColor = GetDominantColor(imgMap);

            return avrgColor;
        }


        public static Color GetDominantColor(Bitmap bmp)
        {

            //Used for tally
            int r = 0;
            int g = 0;
            int b = 0;

            int total = 0;

            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color clr = bmp.GetPixel(x, y);

                    r += clr.R;
                    g += clr.G;
                    b += clr.B;

                    total++;
                }
            }

            //Calculate average
            r /= total;
            g /= total;
            b /= total;

            return Color.FromArgb(r, g, b);
        }

    }
}
