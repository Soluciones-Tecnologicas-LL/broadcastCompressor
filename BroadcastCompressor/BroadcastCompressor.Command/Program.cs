using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using FFMpegCore;
using FFMpegCore.Enums;
using Microsoft.Extensions.Configuration;
using NLog;

namespace BroadcastCompressor.Command
{
    public class Program
    {
        private const string ErrorHeader = "No se puede arrancar el programa";

        private static int _operatorNumber;
        private static Logger _logger;
        private static string _recordingFolder;
        private static string _compressedFilesDestinationFolder;
        private static string[] _filesToCompress;

        static Program()
        {
            _logger = LogManager.GetLogger("FileLogger");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.json");

            var config = builder.Build();
            _operatorNumber = config.GetSection("OperatorNumber").Get<int>();

            if (_operatorNumber <= 0)
            {
                throw new Exception($"{ErrorHeader}. Falta definir el número de operador");
            }

            _recordingFolder = config.GetSection("RecordingFolder").Get<string>();

            if (string.IsNullOrEmpty(_recordingFolder))
            {
                throw new Exception($"{ErrorHeader}. Falta definir la carpeta de grabaciones");
            }

            _compressedFilesDestinationFolder = config.GetSection("CompressedFilesDestinationFolder").Get<string>();

            if (string.IsNullOrEmpty(_compressedFilesDestinationFolder))
            {
                throw new Exception(
                    $"{ErrorHeader}. Falta definir la carpeta de destino de archivos comprimidos");
            }

            _filesToCompress = config.GetSection("FilesToCompress").Get<string[]>();

            if (_filesToCompress == null)
            {
                throw new Exception($"{ErrorHeader}. Falta definir la carpeta de grabaciones");

            }
        }

        public static void Main(string[] args)
        {
            Stopwatch stopWatch = new Stopwatch();
            try
            {
                Console.WriteLine($"****** Inicio de compresión de archivos. Operador {_operatorNumber} ******");
                Console.WriteLine($"Fecha de proceso: {DateTime.Now}" );
                stopWatch.Start();

                List<string> filesToCompress = new List<string>();
                foreach (var pattern in _filesToCompress)
                {
                    filesToCompress.AddRange(Directory.GetFiles(_recordingFolder, pattern));
                }

                if (filesToCompress.Count <= 0)
                {
                    _logger.Warn($"No se hallaron que comprimir archivos el {DateTime.Now}");
                }
                else
                {
                    var destinationSubdir =
                        _compressedFilesDestinationFolder + "\\" + DateTime.Now.ToString("yyyyMMdd");
                    Directory.CreateDirectory(destinationSubdir);

                    foreach (var file in filesToCompress)
                    {
                        Console.WriteLine($"{DateTime.Now} --> Comprimiendo {file}");
                        FileInfo currentFileInfo = new FileInfo(file);
                        var tempFile = Path.Combine(_recordingFolder, "Temp.mp4");
            
                        var inputVideo = FFProbe.Analyse(currentFileInfo.FullName);

                        var scale = (double)inputVideo.PrimaryVideoStream.Height / (int)VideoSize.Ld;
                        var outputSize = new Size((int) (inputVideo.PrimaryVideoStream.Width / scale),
                            (int) (inputVideo.PrimaryVideoStream.Height / scale));

                        if (outputSize.Width % 2 != 0)
                        {
                            outputSize.Width += 1;
                        }

                        var processSuccessful = FFMpegArguments
                            .FromInputFiles(true, inputVideo.Path)
                            .UsingMultithreading(true)
                            .WithVideoCodec(VideoCodec.LibX264)
                            .WithVideoBitrate(350)
                            .Scale(outputSize)
                            .WithSpeedPreset(Speed.SuperFast)
                            .WithAudioCodec(AudioCodec.Aac)
                            .WithAudioBitrate(AudioQuality.Low)
                            .OutputToFile(tempFile).ProcessSynchronously();


                        if (processSuccessful)
                        {
                            var mp4FileName = currentFileInfo.Name.Replace(currentFileInfo.Extension, ".mp4");
                            var sourceFile = Path.Combine(_recordingFolder, mp4FileName);

                            File.Move(tempFile, sourceFile, true);

                            var destinationFile = Path.Combine(destinationSubdir, mp4FileName);
                            File.Copy(sourceFile, destinationFile, true);

                            File.Delete(sourceFile);
                            Console.WriteLine($"{DateTime.Now} --> {file} comprimido exitosamente");
                        }
                        else
                        {
                            Console.WriteLine($"{DateTime.Now} --> {file} no se pudo comprimir");
                            _logger.Error($"{file} no pudo ser convertido. Reintente manualmente");

                        }

                    }
                }

            }
            catch (Exception e)
            {
                _logger.Error(e, "Error inesperado");
            }
            finally
            {
                Console.WriteLine($"****** Fin de compresión de archivos. Operador {_operatorNumber} ******");
                stopWatch.Stop();

                var elapsedTimeTrace =
                    $"Tiempo transcurrido: {stopWatch.Elapsed.Hours} (hh) : {stopWatch.Elapsed.Minutes} (mm) : {stopWatch.Elapsed.Seconds} (ss)";
                Console.WriteLine(elapsedTimeTrace);
                _logger.Info(elapsedTimeTrace);

            }

        }
    }
}
