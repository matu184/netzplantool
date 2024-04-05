using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netzplantool
{
    internal class NetzplanRechner
    {
        public static void CalculateNetworkPlan(List<Node> nodes)
        {
            // berechne Faz und Fez für alle Knoten im Netzplan der erste ist immer 0
            nodes[0].FAZ = 0;
            nodes[0].FEZ = nodes[0].Dauer;
            for (int i = 1; i < nodes.Count; i++)
            {
                var node = nodes[i];
                node.FAZ = nodes.Where(n => node.Vorgänger.Contains(n.Vorgang)).Max(n => n.FEZ);
                node.FEZ = node.FAZ + node.Dauer;
            }
            // berechne Saz und Sez für alle Knoten im Netzplan der erste ist immer 0
            nodes[nodes.Count - 1].SEZ = nodes[nodes.Count - 1].FEZ;
            nodes[nodes.Count - 1].SAZ = nodes[nodes.Count - 1].SEZ - nodes[nodes.Count - 1].Dauer;
            for (int i = nodes.Count - 2; i >= 0; i--)
            {
                var node = nodes[i];
                node.SEZ = nodes.Where(n => n.Vorgänger.Contains(node.Vorgang)).Min(n => n.SAZ);
                node.SAZ = node.SEZ - node.Dauer;
            }
            // berechne Puffer für alle Knoten im Netzplan FP = FAZ (Nachf.) – FEZ (Vorg.)
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                var node = nodes[i];
                var predecessorsSAZs = nodes.Where(n => n.Vorgänger.Contains(node.Vorgang)).Select(n => n.FAZ);
                var minPredecessorSAZ = predecessorsSAZs.FirstOrDefault();
                if (minPredecessorSAZ != default(int))
                {
                    node.Puffer = minPredecessorSAZ - node.FEZ;
                }
                else
                {
                    node.Puffer = 0; // Wenn es keine Vorgänger gibt, ist der Puffer 0
                }
            }
            // berechne Gesamtpuffer für alle Knoten im Netzplan GP = SAZ - FAZ
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                node.Gesamtpuffer = node.SAZ - node.FAZ;
            }
            // Berechne kritischen Pfad
            var criticalPath = nodes.Where(n => n.Gesamtpuffer == 0).ToList();

        }
    }
}
