using System;

namespace Kruskal
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Programm zur Bestimmung eines Kostenminimalen Baumes");

            Beispiel();
            //CsvDatei();

            Console.WriteLine("Zum beenden Taste drücken.");
            Console.ReadKey();
        }

        public static void Beispiel()
        {
            //Beispieldaten erstellen
            Graph BeispielNetz = ErzeugeBeispielNetz();
            //Kruskal Baum berechnen
            KruskalBaumGenerator baumGenerator = new KruskalBaumGenerator();
            baumGenerator.GeneriereBaum(BeispielNetz);

            double kumuliertesGewicht = 0d;
            Console.WriteLine("Quelle,Ziel,Gewicht,kumuliertes Gewicht");
            foreach (var kante in BeispielNetz.MinimalSpannenderBaum)
            {
                kumuliertesGewicht += kante.Gewicht;
                Console.WriteLine($"{kante.QuellKnoten.Name} - {kante.ZielKnoten.Name}, {kante.Gewicht}, {kumuliertesGewicht}");
            }
        }

        private static void CsvDatei()
        {
            Console.WriteLine("CSV Datei:");
            string csvDateiPfad = Console.ReadLine().Trim('"').Trim();
            try
            {
                Graph netzwerk = Formatierer.LeseNetzwerk(csvDateiPfad);
                ErweiterterKruskalBaumGenerator baumGenerator = new ErweiterterKruskalBaumGenerator();
                Auswertung auswertung = baumGenerator.GeneriereBaumMitAuswertung(netzwerk);
                Console.WriteLine("Baum generiert!");
                Formatierer.SchreibeLoesungInDatei(csvDateiPfad, netzwerk);
                Console.WriteLine("Lösungsdatei abgelegt.");
                Console.WriteLine(auswertung.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }            
        }
        
        public static Graph ErzeugeBeispielNetz()
        {
            // Netzwerk erzeugen
            Graph beispielNetzwerk = new Graph();
            
            // Knoten zufügen
            string[] nodeLabels = new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i" };

            for (int i = 0; i < nodeLabels.Length; i++)
            {
                beispielNetzwerk.Knoten.Add(new Knoten() { Name = nodeLabels[i] });
            }
            // Kanten zufügen
            beispielNetzwerk.NeueKante("a", "b", 4);
            beispielNetzwerk.NeueKante("a", "b", 4);
            beispielNetzwerk.NeueKante("a", "h", 8);
            beispielNetzwerk.NeueKante("b", "h", 11);
            beispielNetzwerk.NeueKante("b", "c", 8);
            beispielNetzwerk.NeueKante("c", "i", 2);
            beispielNetzwerk.NeueKante("c", "f", 4);
            beispielNetzwerk.NeueKante("c", "d", 7);
            beispielNetzwerk.NeueKante("i", "h", 7);
            beispielNetzwerk.NeueKante("h", "g", 1);
            beispielNetzwerk.NeueKante("i", "g", 6);
            beispielNetzwerk.NeueKante("g", "f", 2);
            beispielNetzwerk.NeueKante("d", "f", 14);
            beispielNetzwerk.NeueKante("f", "e", 10);
            beispielNetzwerk.NeueKante("d", "e", 9);
            // Netzwerk zurückgeben
            return beispielNetzwerk;
        }
    }
}
