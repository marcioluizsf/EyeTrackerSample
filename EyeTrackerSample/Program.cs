using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tobii.Interaction;
using Tobii.Interaction.Framework;

namespace EyeTrackerSample
{
    class Program
    {
        const string DATA_DIRECTORY = @"D:\Temp";
        static StreamWriter _gazeDataStreamWriter = null;
        static StreamWriter _fixationDataStreamWriter = null;
        static StreamWriter _headPoseStreamWriter = null;
        static StreamWriter _eyePositionStreamWriter = null;
        static string _fileNamePrefix = null;
        static Host _host = null;
        static int _screenWidth = Screen.PrimaryScreen.Bounds.Width;
        static int _screenHeight = Screen.PrimaryScreen.Bounds.Height;

        static void Main(string[] args)
        {
            Console.WriteLine("1: Save all streams data");
            Console.WriteLine("2: Take screen shots");
            Console.WriteLine("3: Save gaze data");
            Console.WriteLine("4: Save fixation data");
            Console.WriteLine("5: Save head pose data");
            Console.WriteLine("6: Save eye position data");
            Console.WriteLine();
            Console.WriteLine("Press any other key to quit application");

            var keyInfo = Console.ReadKey(true);

            switch (keyInfo.Key)
            {
                case ConsoleKey.D1:
                    UpdateScreen();
                    SaveAllDataIntoFile();
                    Console.ReadKey();
                    break;

                case ConsoleKey.D2:
                    UpdateScreen();
                    TakeScreenShots();
                    Console.ReadKey();
                    break;

                case ConsoleKey.D3:
                    UpdateScreen();
                    SaveGazeDataIntoFile();
                    Console.ReadKey();
                    break;

                case ConsoleKey.D4:
                    UpdateScreen();
                    SaveFixationDataIntoFile();
                    Console.ReadKey();
                    break;

                case ConsoleKey.D5:
                    UpdateScreen();
                    SaveHeadPoseDataIntoFile();
                    Console.ReadKey();
                    break;

                case ConsoleKey.D6:
                    UpdateScreen();
                    SaveEyePositionDataIntoFile();
                    Console.ReadKey();
                    break;

                default:
                    break;
            }

            if (_gazeDataStreamWriter != null)
            {
                _gazeDataStreamWriter.Flush();
                _gazeDataStreamWriter.Dispose();
                _gazeDataStreamWriter = null;
            }

            if (_fixationDataStreamWriter != null)
            {
                _fixationDataStreamWriter.Flush();
                _fixationDataStreamWriter.Dispose();
                _fixationDataStreamWriter = null;
            }

            if (_headPoseStreamWriter != null)
            {
                _headPoseStreamWriter.Flush();
                _headPoseStreamWriter.Dispose();
                _headPoseStreamWriter = null;
            }

            if (_eyePositionStreamWriter != null)
            {
                _eyePositionStreamWriter.Flush();
                _eyePositionStreamWriter.Dispose();
                _eyePositionStreamWriter = null;
            }

            if (_host != null)
                _host.DisableConnection();
        }

        private static void SaveAllDataIntoFile()
        {
            SaveGazeDataIntoFile();
            SaveFixationDataIntoFile();
            SaveHeadPoseDataIntoFile();
            SaveEyePositionDataIntoFile();
        }

        private static void UpdateScreen()
        {
            Console.Clear();
            Console.WriteLine("Collecting data...");
            Console.WriteLine("Press any key to quit application");
        }

        private static void SaveHeadPoseDataIntoFile()
        {
            var headPoseStream = GetHost().Streams.CreateHeadPoseStream();

            _headPoseStreamWriter = GetStreamWriter("HeadPoseData");
            _headPoseStreamWriter.WriteLine(@"timestamp;engine_timestamp;head_position_x;head_position_y;head_position_z;head_rotation_x;head_rotation_y;head_rotation_z");

            headPoseStream.Next += (object sender, StreamData<HeadPoseData> e) =>
            {
                if (_headPoseStreamWriter == null)
                    return;

                var timestamp = GetTimestamp();

                _headPoseStreamWriter.Write($"{timestamp};{e.Data.EngineTimestamp};");

                _headPoseStreamWriter.Write(e.Data.HasHeadPosition ?
                    GetVectorComponentsSemicolonSeparated(e.Data.HeadPosition) :
                    GetVectorComponentsSemicolonSeparated(null));

                _headPoseStreamWriter.Write(";");

                if (e.Data.HasRotation.HasRotationX)
                    _headPoseStreamWriter.Write($"{e.Data.HeadRotation.X}");

                _headPoseStreamWriter.Write(";");

                if (e.Data.HasRotation.HasRotationY)
                    _headPoseStreamWriter.Write($"{e.Data.HeadRotation.Y}");

                _headPoseStreamWriter.Write(";");

                if (e.Data.HasRotation.HasRotationZ)
                    _headPoseStreamWriter.Write($"{e.Data.HeadRotation.Z}");

                _headPoseStreamWriter.Write(Environment.NewLine);
            };
        }

        private static void SaveFixationDataIntoFile()
        {
            var fixationDataStream = GetHost().Streams.CreateFixationDataStream();

            _fixationDataStreamWriter = GetStreamWriter("FixationData");
            _fixationDataStreamWriter.WriteLine("timestamp;engine_timestamp;event_type;x;y");

            fixationDataStream.Next += (object sender, StreamData<FixationData> e) =>
            {
                if (_fixationDataStreamWriter == null)
                    return;

                var timestamp = GetTimestamp();
                var eventType = GetEventTypeAsString(e.Data.EventType);

                _fixationDataStreamWriter.WriteLine($"{timestamp};{e.Data.EngineTimestamp};{eventType};{e.Data.X};{e.Data.Y}");
            };
        }

        private static void SaveGazeDataIntoFile()
        {
            var gazePointDataStream = GetHost().Streams.CreateGazePointDataStream();

            _gazeDataStreamWriter = GetStreamWriter("GazeData");
            _gazeDataStreamWriter.WriteLine("timestamp;engine_timestamp;x;y");

            gazePointDataStream.Next += (object sender, StreamData<GazePointData> e) =>
            {
                if (_gazeDataStreamWriter == null)
                    return;

                var timestamp = GetTimestamp();
                _gazeDataStreamWriter.WriteLine($"{timestamp};{e.Data.EngineTimestamp};{e.Data.X};{e.Data.Y}");
            };
        }

        private static void SaveEyePositionDataIntoFile()
        {
            var eyePositionDataStream = GetHost().Streams.CreateEyePositionStream();

            _eyePositionStreamWriter = GetStreamWriter("EyePositionData");
            _eyePositionStreamWriter.WriteLine("timestamp;engine_timestamp;normalized_left_eye_x;normalized_left_eye_y;normalized_left_eye_z;left_eye_x;left_eye_y;left_eye_z;normalized_right_eye_x;normalized_right_eye_y;normalized_right_eye_z;right_eye_x;right_eye_y;right_eye_z");

            eyePositionDataStream.Next += (object sender, StreamData<EyePositionData> e) =>
            {
                if (_eyePositionStreamWriter == null)
                    return;

                var timestamp = GetTimestamp();

                _eyePositionStreamWriter.Write($"{timestamp};{e.Data.EngineTimestamp};");

                _eyePositionStreamWriter.Write(GetVectorComponentsSemicolonSeparated(e.Data.LeftEyeNormalized));

                _eyePositionStreamWriter.Write(";");

                _eyePositionStreamWriter.Write(
                    e.Data.HasLeftEyePosition ?
                    GetVectorComponentsSemicolonSeparated(e.Data.LeftEye) :
                    GetVectorComponentsSemicolonSeparated(null));

                _eyePositionStreamWriter.Write(";");

                _eyePositionStreamWriter.Write(GetVectorComponentsSemicolonSeparated(e.Data.RightEyeNormalized));

                _eyePositionStreamWriter.Write(";");

                _eyePositionStreamWriter.Write(
                    e.Data.HasRightEyePosition ?
                    GetVectorComponentsSemicolonSeparated(e.Data.RightEye) :
                    GetVectorComponentsSemicolonSeparated(null));

                _eyePositionStreamWriter.Write(Environment.NewLine);
            };
        }

        private static string GetVectorComponentsSemicolonSeparated(Vector3? vector)
        {
            if (!vector.HasValue)
                return ";;";

            return $"{vector.Value.X};{vector.Value.Y};{vector.Value.Z}";
        }

        private static string GetFileNamePrefix()
        {
            if (_fileNamePrefix == null)
                _fileNamePrefix = DateTime.Now.ToString("yyyyMMddHHmmss");

            return _fileNamePrefix;
        }

        private static StreamWriter GetStreamWriter(string fileName)
        {
            if (!Directory.Exists(DATA_DIRECTORY))
                Directory.CreateDirectory(DATA_DIRECTORY);

            return File.CreateText($@"{DATA_DIRECTORY}\{GetFileNamePrefix()}_{fileName}.csv");
        }

        private static object GetEventTypeAsString(FixationDataEventType eventType)
        {
            if (eventType == FixationDataEventType.Begin)
                return "begin";

            if (eventType == FixationDataEventType.Data)
                return "data";

            if (eventType == FixationDataEventType.End)
                return "end";

            return string.Empty;
        }

        private static string GetTimestamp()
        {
            return DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fffffff");
        }

        private static void TakeScreenShots()
        {
            var gazePointDataStream = GetHost().Streams.CreateGazePointDataStream();

            gazePointDataStream.Next += (object sender, StreamData<GazePointData> e) =>
            {
                var bitmap = new Bitmap(_screenWidth, _screenHeight);
                var graphics = Graphics.FromImage(bitmap as Image);

                graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);

                DrawPoint(e.Data.X, e.Data.Y, graphics);

                bitmap.Save($@"{DATA_DIRECTORY}\{DateTime.Now.ToString("yyyyMMddHHmmssfffff")}.jpg", ImageFormat.Jpeg);
            };
        }

        private static void DrawPoint(double x, double y, Graphics graphics)
        {
            var mainPoint = new Point((int)x, (int)y);

            graphics.DrawLine(new Pen(Brushes.Red, 2), mainPoint, new Point(mainPoint.X, mainPoint.Y + 10));
            graphics.DrawLine(new Pen(Brushes.Red, 2), mainPoint, new Point(mainPoint.X, mainPoint.Y - 10));
            graphics.DrawLine(new Pen(Brushes.Red, 2), mainPoint, new Point(mainPoint.X + 10, mainPoint.Y));
            graphics.DrawLine(new Pen(Brushes.Red, 2), mainPoint, new Point(mainPoint.X - 10, mainPoint.Y));

        }

        private static Host GetHost()
        {
            if (_host == null)
                _host = new Host();

            return _host;
        }
    }
}
