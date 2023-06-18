using MDSDK.JPEG2000.Convert.JP2File;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MDSDK.JPEG2000.Convert
{
    class Program
    {
        public static string InputFolder { get; set; }

        public static string OutputFolder { get; set; }

        private static List<string> InputFiles { get; } = new List<string>();

        static void ParseCommandLine(string[] args)
        {
            var i = 0;
            while (i < args.Length)
            {
                var option = args[i];
                var eqPos = option.IndexOf('=');
                if (eqPos < 0)
                {
                    break;
                }
                var optionName = option.Substring(0, eqPos);
                var property = typeof(Program).GetProperty(optionName.Trim(), BindingFlags.Public | BindingFlags.Static);
                if (property == null)
                {
                    throw new ArgumentException($"Invalid option '{optionName}'");
                }
                var optionValue = option.Substring(eqPos + 1);
                property.SetValue(null, optionValue?.Trim());
                i++;
            }

            while (i < args.Length)
            {
                InputFiles.Add(args[i]);
                i++;
            }
        }

        static bool Is8BitRGB(ImageDescriptor imageDescriptor)
        {
            var components = imageDescriptor.Components;
            return (components.Length == 3) && components.All(c => (c.BitDepth == 8) && !c.IsSigned);
        }

        static void WriteBitmap(Bitmap bitmap, ImageDescriptor imageDescriptor, int[] imageData)
        {
            var bitmapPixelCreator = imageDescriptor.GetBitmapPixelCreator();

            var i = 0;
            for (var y = 0; y < bitmap.Height; y++)
            {
                for (var x = 0; x < bitmap.Width; x++)
                {
                    var color = bitmapPixelCreator.Invoke(imageData, ref i);
                    bitmap.SetPixel(x, y, color);
                }
            }
        }

        static void ConvertJP2File(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var reader = new JP2FileReader(stream);
                var imageDescriptor = reader.GetImageDescriptor();
                var imageData = reader.DecodeImage();

                var bitmap = new Bitmap(imageDescriptor.Width, imageDescriptor.Height, PixelFormat.Format24bppRgb);
                WriteBitmap(bitmap, imageDescriptor, imageData);

                var fileName = Path.GetFileNameWithoutExtension(path);
                var outputPath = Path.Combine(OutputFolder, fileName + ".png");
                bitmap.Save(outputPath, ImageFormat.Png);
            }
        }

        static void Convert(string path)
        {
            if (path.EndsWith(".jp2", StringComparison.InvariantCultureIgnoreCase))
            {
                ConvertJP2File(path);
            }
            else
            {
                Console.WriteLine($"Skipped {path}: suffix not recognized");
            }
        }

        static void ConvertInputFiles()
        {
            foreach (var inputFile in InputFiles)
            {
                var absoluteInputFile = Path.IsPathFullyQualified(inputFile)
                    ? inputFile
                    : Path.Combine(InputFolder, inputFile);

                var directory = Path.GetDirectoryName(absoluteInputFile);
                var searchPattern = Path.GetFileName(absoluteInputFile);

                foreach (var path in Directory.GetFiles(directory, searchPattern))
                {
                    Console.Write($"Converting {path}...");
                    try
                    {
                        Convert(path);
                        Console.WriteLine(" OK");
                    }
                    catch (Exception error)
                    {
                        Console.WriteLine($" Failed ({error.Message})");
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            ParseCommandLine(args);

            if (string.IsNullOrEmpty(InputFolder))
            {
                InputFolder = Environment.CurrentDirectory;
            }

            if (string.IsNullOrEmpty(OutputFolder))
            {
                OutputFolder = Environment.CurrentDirectory;
            }

            if (InputFiles.Count == 0)
            {
                InputFiles.AddRange(Directory.EnumerateFiles(InputFolder, "*.jp2"));
            }

            ConvertInputFiles();
        }
    }
}
