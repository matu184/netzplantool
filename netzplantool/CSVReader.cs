using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netzplantool
{
    internal class CSVReader
    {
        public static List<Node> ReadCSV(string filePath)
        {
            // Lese CSV-Datei und erstelle Liste von Knoten
            List<Node> nodes = new List<Node>();
            using (var reader = new StreamReader(filePath))
            {
                reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');
                    if (values.Length != 4) continue;
                    Node node = new Node
                    {
                        Vorgang = values[0],
                        Beschreibung = values[1],
                        Dauer = int.Parse(values[2]),
                        Vorgänger = values[3].Split(',')
                    };
                    nodes.Add(node);
                }
            }
            return nodes;
        }
    }
}
