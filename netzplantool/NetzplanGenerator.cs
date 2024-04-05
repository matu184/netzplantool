using CsvHelper;
using GraphVizWrapper.Commands;
using GraphVizWrapper.Queries;
using GraphVizWrapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netzplantool
{
    internal class NetzplanGenerator
    {
        // Attribute
        private string inputFilePath;
        private string outputFilePath;
        private string outputFormat;
        // Konstruktor
        public NetzplanGenerator(string[] args)
        {
            var argumentParser = new ArgumentParser(args);
            inputFilePath = argumentParser.InputFilePath;
            outputFilePath = argumentParser.OutputFilePath;
            outputFormat = GetOutputFormat(outputFilePath);
        }
        // Generierung des Netzplans
        public void Generate()
        {
            // CSVReader
            var nodes = CSVReader.ReadCSV(inputFilePath);
            NetzplanRechner.CalculateNetworkPlan(nodes);
            string graphDefinition = GenerateGraphDefinition(nodes);
            // GraphVizWrapper
            var getStartProcessQuery = new GetStartProcessQuery();
            var getProcessStartInfoQuery = new GetProcessStartInfoQuery();
            var registerLayoutPluginCommand = new RegisterLayoutPluginCommand(getProcessStartInfoQuery, getStartProcessQuery);
            var wrapper = new GraphGeneration(getStartProcessQuery,
                                              getProcessStartInfoQuery,
                                              registerLayoutPluginCommand);
            // Generierung des Netzplans
            byte[] output = wrapper.GenerateGraph(graphDefinition, GetGraphReturnType(outputFormat));
            // Speichern des Netzplans
            using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(outputFilePath)))
            {
                writer.Write(output);
            }
        }
        // Rückgabe des Ausgabeformates
        private string GetOutputFormat(string filePath)
        {
            // Dateierweiterung
            string extension = Path.GetExtension(filePath).ToLower();
            switch (extension)
            {
                case ".png":
                    return "png";
                case ".jpg":
                case ".jpeg":
                    return "jpg";
                case ".pdf":
                    return "pdf";
                case ".svg":
                    return "svg";
                default:
                    throw new ArgumentException($"Ungültige Dateierweiterung: {extension}");
            }
        }
        // Rückgabe des Ausgabeformates
        private Enums.GraphReturnType GetGraphReturnType(string format)
        {
            // Ausgabeformat
            switch (format.ToLower())
            {
                case "png":
                    return Enums.GraphReturnType.Png;
                case "jpg":
                    return Enums.GraphReturnType.Jpg;
                case "pdf":
                    return Enums.GraphReturnType.Pdf;
                case "svg":
                    return Enums.GraphReturnType.Svg;
                default:
                    throw new ArgumentException($"Ungültiges Ausgabeformat: {format}");
            }
        }

        static string GenerateGraphDefinition(List<Node> nodes)
        {
            // Graphdefinition
            string graphDefinition = "digraph structs {\n";
            graphDefinition += "node [shape=record];\n";
            graphDefinition += "rankdir=RL;\n";
            // Knoten
            for (int i = 0; i < nodes.Count; i++)
            {
                var sourceNode = nodes[i];
                string sourceLabel = $"{{FAZ = {sourceNode.FAZ} | FEZ = {sourceNode.FEZ}}}|{{ {sourceNode.Vorgang} | {sourceNode.Beschreibung} }}|{{ Dauer: {sourceNode.Dauer} | GP = {sourceNode.Gesamtpuffer} | FP = {sourceNode.Puffer} }}|{{ SAZ = {sourceNode.SAZ} | SEZ = {sourceNode.SEZ} }}";

                string sourceNodeId = $"struct{i}";
                graphDefinition += $"{sourceNodeId} [label=\"{sourceLabel}\"];\n";
            }
            // Kanten
            for (int i = 0; i < nodes.Count; i++)
            {
                // Quelle
                var sourceNode = nodes[i];
                string sourceNodeId = $"struct{i}";
                // Vorgänger
                foreach (var vorgaenger in sourceNode.Vorgänger)
                {
                    var targetNode = nodes.FirstOrDefault(n => n.Vorgang == vorgaenger);
                    if (targetNode == null) continue;

                    string targetNodeId = $"struct{nodes.IndexOf(targetNode)}";
                    // alle knoten die auf dem kritischen Pfad liegen rot färben wo der vorgänger des aktuellen knoten auf dem kritischen Pfad liegt
                    if (sourceNode.Gesamtpuffer == 0 && targetNode.Gesamtpuffer == 0)
                    {
                        graphDefinition += $"{sourceNodeId} -> {targetNodeId} [dir=back, color=red];\n";
                    }
                    else
                    {
                        graphDefinition += $"{sourceNodeId} -> {targetNodeId} [dir=back];\n";
                    }
                }
            }
            // Ende der Graphdefinition
            graphDefinition += "}";
            return graphDefinition;
        }
    }
}