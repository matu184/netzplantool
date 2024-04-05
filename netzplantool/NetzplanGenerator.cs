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
        private string inputFilePath;
        private string outputFilePath;

        public NetzplanGenerator(string[] args)
        {
            var argumentParser = new ArgumentParser(args);
            inputFilePath = argumentParser.InputFilePath;
            outputFilePath = argumentParser.OutputFilePath;
        }

        public void Generate()
        {
            var nodes = CSVReader.ReadCSV(inputFilePath);
            NetzplanRechner.CalculateNetworkPlan(nodes);
            string graphDefinition = GenerateGraphDefinition(nodes);

            var getStartProcessQuery = new GetStartProcessQuery();
            var getProcessStartInfoQuery = new GetProcessStartInfoQuery();
            var registerLayoutPluginCommand = new RegisterLayoutPluginCommand(getProcessStartInfoQuery, getStartProcessQuery);
            var wrapper = new GraphGeneration(getStartProcessQuery,
                                              getProcessStartInfoQuery,
                                              registerLayoutPluginCommand);

            byte[] output = wrapper.GenerateGraph(graphDefinition, Enums.GraphReturnType.Png);

            using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(outputFilePath)))
            {
                writer.Write(output);
            }
        }
        static string GenerateGraphDefinition(List<Node> nodes)
        {

            string graphDefinition = "digraph structs {\n";
            graphDefinition += "node [shape=record];\n";
            graphDefinition += "rankdir=RL;\n";

            for (int i = 0; i < nodes.Count; i++)
            {
                var sourceNode = nodes[i];
                string sourceLabel = $"{{FAZ = {sourceNode.FAZ} | FEZ = {sourceNode.FEZ}}}|{{ {sourceNode.Vorgang} | {sourceNode.Beschreibung} }}|{{ Dauer: {sourceNode.Dauer} | GP = {sourceNode.Gesamtpuffer} | FP = {sourceNode.Puffer} }}|{{ SAZ = {sourceNode.SAZ} | SEZ = {sourceNode.SEZ} }}";

                string sourceNodeId = $"struct{i}";
                graphDefinition += $"{sourceNodeId} [label=\"{sourceLabel}\"];\n";
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                var sourceNode = nodes[i];
                string sourceNodeId = $"struct{i}";

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
            graphDefinition += "}";
            return graphDefinition;
        }
    }
}
