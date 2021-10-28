using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using Q42.HueApi.Interfaces;
using Q42.HueApi.Extensions;
using Q42.HueApi.Models.Bridge;
using Q42.HueApi;
using Q42.HueApi.ColorConverters.HSB;
using Q42.HueApi.ColorConverters;


namespace HueSync_2022
{
    class Program
    {
        static async Task Main(string[] args)
        {


            await RunProgram();

            
            








        }

        static async Task RunProgram()
        {
            bool Running = true;
            ILocalHueClient client = new LocalHueClient("192.168.1.82");
            client.Initialize("S8PiGPDDARx8vRVklq8H9siZvAgMRByKW758wEcq");
            Console.WriteLine("Client Initiliazed");
            
            long delayDuration = 100;
            do
            {
                Stopwatch timer = new Stopwatch();
                timer.Start();
                var command = new LightCommand();
                command.On = true;
                Task<Color> screen = new Task<Color>(() => ScreenShotAndSendColor());
                screen.Start();





                Console.WriteLine("Setting color");
                //Awaits a task, when i remove the awaits the program runs fine, but when i await these tasks it exits the while loop and ends.
                Color clr = await screen;
                double bri = (double)clr.GetBrightness();
                double hue = (double)clr.GetHue();
                double sat = (double)clr.GetSaturation() * 5;
                Debug.WriteLine("Sat before correction: " + sat);
                if(sat > 1)
                {
                    sat = 1;
                }
                Debug.WriteLine("Saturation " + sat);
                Console.WriteLine("Hue " + hue);

                Color finalColor = ColorFromHSV(hue, sat, bri);





                
                

                
                string hex = HexConverter(finalColor);

                command.TurnOn().SetColor(new RGBColor(hex));
                Console.WriteLine("Sending Command to client");
                //Awaits a task
                Task<Q42.HueApi.Models.Groups.HueResults> commandsend = client.SendCommandAsync(command, new List<string> { "13" });
                
                Console.WriteLine("Command Sent!");
                
                await commandsend;
                long taskTime = timer.ElapsedMilliseconds;
                
                
                if(taskTime < delayDuration)
                {
                    int timeLeft = int.Parse((delayDuration - timer.ElapsedMilliseconds).ToString());
                    await Task.Delay(timeLeft);
                }
                else
                {

                }
                timer.Stop();
                Debug.WriteLine("Time taken: " + timer.ElapsedMilliseconds);
            }
            while (Running);
            
               
                
                   
                
                
            

            
        }

        private static String HexConverter(System.Drawing.Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }


        static Color ScreenShotAndSendColor()
        {
            
            Bitmap imgMap = new Bitmap(1920, 1080);
            Size s = new Size(imgMap.Width, imgMap.Height);

            Graphics graphics = Graphics.FromImage(imgMap);

            graphics.CopyFromScreen(0, 0, 0, 0, s);

            //Bitmap ResizedMap = new Bitmap(imgMap, new Size(imgMap.Width / 4, imgMap.Height / 4));
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
                if(x % 50 == 0)
                {
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        if(y % 50 == 0)
                        {
                            Color clr = bmp.GetPixel(x, y);

                            r += clr.R;
                            g += clr.G;
                            b += clr.B;

                            total++;
                        }
                        
                    }
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
