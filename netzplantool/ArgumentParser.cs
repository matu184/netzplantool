using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netzplantool
{
    internal class ArgumentParser
    {
        private string _inputFilePath;
        private string _outputFilePath;

        public ArgumentParser(string[] args)
        {
            ParseArguments(args);
        }

        public string InputFilePath => _inputFilePath;
        public string OutputFilePath => _outputFilePath;

        private void ParseArguments(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-i":
                    case "--input":
                        if (i + 1 < args.Length)
                        {
                            _inputFilePath = args[i + 1];
                            i++;
                        }
                        else
                        {
                            PrintHelp();
                            Environment.Exit(1);
                        }
                        break;
                    case "-o":
                    case "--output":
                        if (i + 1 < args.Length)
                        {
                            _outputFilePath = args[i + 1];
                            i++;
                        }
                        else
                        {
                            PrintHelp();
                            Environment.Exit(1);
                        }
                        break;
                    case "-h":
                    case "--help":
                        PrintHelp();
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine($"Ungültige Option: {args[i]}");
                        PrintHelp();
                        Environment.Exit(1);
                        break;
                }
            }

            if (string.IsNullOrEmpty(_inputFilePath))
            {
                Console.WriteLine("Fehler: Eingabedatei muss angegeben werden.");
                PrintHelp();
                Environment.Exit(1);
            }

            if (string.IsNullOrEmpty(_outputFilePath))
            {
                string inputDirectory = Path.GetDirectoryName(_inputFilePath);
                string inputFileName = Path.GetFileNameWithoutExtension(_inputFilePath);
                _outputFilePath = Path.Combine(inputDirectory, $"{inputFileName}.png");
            }
        }

        private void PrintHelp()
        {
            Console.WriteLine("netzplantool -i <input-file> [-o <output-file>]");
            Console.WriteLine("Generiert einen Netzplan aus einer CSV-Datei.");
            Console.WriteLine();
            Console.WriteLine("Optionen:");
            Console.WriteLine("  -i, --input <input-file>   Pfad zur Eingabedatei (CSV)");
            Console.WriteLine("  -o, --output <output-file> Pfad zur Ausgabedatei (PNG)");
            Console.WriteLine("  -h, --help                 Zeigt diese Hilfe an");
        }
    }
}
