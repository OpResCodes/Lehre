using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Kruskal
{
    [DebuggerDisplay("{Name} (in {Graph.Name})")]
    public class Knoten
    {
        public string Name { get; set; }

        public Graph Graph { get; set; }

    }

    [DebuggerDisplay("{QuellKnoten.Name} - {ZielKnoten.Name}, {Gewicht}")]
    public class Kante
    {
        public Kante(Knoten quellKnoten, Knoten zielKnoten, double gewicht)
        {
            Gewicht = gewicht;
            QuellKnoten = quellKnoten;
            ZielKnoten = zielKnoten;
            IstInLoesung = false;
        }

        public Knoten QuellKnoten { get; set; }

        public Knoten ZielKnoten { get; set; }

        public double Gewicht { get; set; }

        public bool IstInLoesung { get; set; }

    }

    [DebuggerDisplay("{Name}, {Knoten.Count}")]
    public class Graph
    {
        public List<Knoten> Knoten { get; }
        public List<Kante> Kanten { get; }
        public string Name { get; }

        public Graph()
        {
            Knoten = new List<Knoten>();
            Kanten = new List<Kante>();
            Name = "Graph";
        }

        public Graph(Knoten startKnoten) : this()
        {
            KnotenZufuegen(startKnoten);
            Name = startKnoten.Name;
        }

        public void KnotenZufuegen(Knoten knoten)
        {
            knoten.Graph = this;
            Knoten.Add(knoten);
        }

        public void NeueKante(Knoten quelle, Knoten ziel, double gewicht)
        {
            Kanten.Add(new Kante(quelle, ziel, gewicht));
        }

        public void NeueKante(string quelle, string ziel, double gewicht)
        {
            Knoten q = Knoten.Find(y => y.Name == quelle);
            Knoten z = Knoten.Find(y => y.Name == ziel);
            NeueKante(q, z, gewicht);
        }

        public double Gewicht
        {
            get
            {
                double w = 0;
                foreach (Kante kante in Kanten)
                {
                    w = w + kante.Gewicht;
                }
                return w;
            }
        }

        public List<Kante> MinimalSpannenderBaum
        {
            get
            {
                List<Kante> baumKanten = new List<Kante>();
                foreach (Kante k in Kanten)
                {
                    if (k.IstInLoesung)
                    {
                        baumKanten.Add(k);
                    }
                }
                return baumKanten;
            }
        }

        public double KruskalGewicht
        {
            get
            {
                double gewicht = 0;
                foreach (Kante k in MinimalSpannenderBaum)
                {
                    gewicht += k.Gewicht;
                }
                return gewicht;
            }
        }

    }

    public class KruskalBaumGenerator
    {
        private List<Graph> Teilbäume;

        public void GeneriereBaum(Graph Eingabedaten)
        {
            //1. Erzeuge Teilbäume
            ErzeugeWald(Eingabedaten);
            //2. Sortiere Kanten
            SortiereKanten(Eingabedaten);
            //3. Iteriere über die sortierten Kanten
            foreach (Kante kante in Eingabedaten.Kanten)
            {
                if (FindeBaum(kante.QuellKnoten) != FindeBaum(kante.ZielKnoten))
                {
                    //Knoten gehören zu unterschiedlichen Teilbäumen - Kante akzeptieren
                    kante.IstInLoesung = true;
                    //Beide Teilbäume vereinigen
                    Vereinige(kante.QuellKnoten, kante.ZielKnoten);
                }
                if (Teilbäume.Count < 2)
                {
                    break;
                }
            }
            if (Teilbäume.Count > 1)
            {
                throw new ArgumentException("Graph ist nicht zusammenhängend.");
            }
        }

        private void ErzeugeWald(Graph netzwerk)
        {
            Teilbäume = new List<Graph>();
            foreach (var knoten in netzwerk.Knoten)
            {
                Teilbäume.Add(new Graph(knoten));
            }
        }

        private void SortiereKanten(Graph netzwerk)
        {
            netzwerk.Kanten.Sort((a, b) => a.Gewicht.CompareTo(b.Gewicht));
        }

        private string FindeBaum(Knoten knoten)
        {
            return knoten.Graph.Name;
        }

        private void Vereinige(Knoten k1, Knoten k2)
        {
            Graph baum1 = k1.Graph;
            Graph baum2 = k2.Graph;
            //baum 1 erweitern
            foreach (Knoten knotenAusBaum2 in baum2.Knoten)
            {
                baum1.KnotenZufuegen(knotenAusBaum2);
            }
            foreach (Kante kanteAusBaum2 in baum2.Kanten)
            {
                baum1.Kanten.Add(kanteAusBaum2);
            }
            //baum 2 entfernen
            Teilbäume.Remove(baum2);
        }
    }

    //Erweiterung zur Darstellung
    public struct Auswertung
    {
        public Auswertung(TimeSpan rechenZeit, int kanten, int knoten, double gewicht)
        {
            Rechenzeit = rechenZeit;
            AnzahlKanten = kanten;
            AnzahlKnoten = knoten;
            Gesamtgewicht = gewicht;
        }

        public TimeSpan Rechenzeit { get; }

        public int AnzahlKanten { get; }

        public int AnzahlKnoten { get; }

        public double Gesamtgewicht { get; }

        public override string ToString()
        {
            return $"Knoten: {AnzahlKnoten}, Kanten: {AnzahlKanten}, Lösung: {Gesamtgewicht}\nRechenzeit: {Rechenzeit.TotalSeconds.ToString("N4")} Sekunden";
        }

    }

    public class ErweiterterKruskalBaumGenerator : KruskalBaumGenerator
    {
        public Auswertung GeneriereBaumMitAuswertung(Graph netzwerk)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            GeneriereBaum(netzwerk);
            stopwatch.Stop();
            Auswertung auswertung = new Auswertung(stopwatch.Elapsed, 
                netzwerk.Kanten.Count, 
                netzwerk.Knoten.Count, 
                netzwerk.KruskalGewicht);
            return auswertung;
        }
    }

    //Lesen und Schreiben von .csv Daten:
    public class Formatierer
    {
        //GeoWKT;Knoten1;Knoten2;Gewicht
        public static Graph LeseNetzwerk(string csvDateiPfad)
        {
            Graph n = new Graph();
            Dictionary<string, Knoten> geleseneKnoten = new Dictionary<string, Knoten>();
            Dictionary<string, Kante> geleseneKanten = new Dictionary<string, Kante>();

            using (var reader = new StreamReader(csvDateiPfad, Encoding.Default))
            {
                //Kopfzeile
                reader.ReadLine();
                while (reader.Peek() >= 0)
                {
                    string zeile = reader.ReadLine().Trim();
                    if (!string.IsNullOrWhiteSpace(zeile))
                    {
                        string[] spaltenArray = zeile.Split(';');
                        string k1 = spaltenArray[1];
                        string k2 = spaltenArray[2];
                        if (double.TryParse(spaltenArray[3], out double w))
                        {
                            //check ob Kante (oder Gegenkante) bereits zugefügt
                            if (!(geleseneKanten.ContainsKey($"{k2}_{k1}") | geleseneKanten.ContainsKey($"{k1}_{k2}")))
                            {
                                //merke Knoten, falls noch nicht eingelesen
                                for (int i = 1; i <= 2; i++)
                                {
                                    if (!geleseneKnoten.TryGetValue(spaltenArray[i], out Knoten knoten))
                                    {
                                        knoten = new Knoten() { Name = spaltenArray[i] };
                                        geleseneKnoten.Add(knoten.Name, knoten);
                                    }
                                }
                                //merke Kante
                                Kante kante = new Kante(geleseneKnoten[k1], geleseneKnoten[k2], w);
                                geleseneKanten.Add($"{k1}_{k2}", kante);
                            }
                        }
                    }
                }
            }
            n.Knoten.AddRange(geleseneKnoten.Values);
            n.Kanten.AddRange(geleseneKanten.Values);
            return n;
        }

        //GeoWKT;Knoten1;Knoten2;Gewicht
        public static void SchreibeLoesungInDatei(string csvDateiPfad, Graph n)
        {
            //loesung merken (codieren)
            HashSet<string> codes = new HashSet<string>();
            foreach (var k in n.MinimalSpannenderBaum)
            {
                codes.Add($"{k.QuellKnoten.Name}_{k.ZielKnoten.Name}");
            }

            //Ausgabe string zusammenbauen
            StringBuilder sb = new StringBuilder();
            //lesen aus der .csv Datei
            using (var reader = new StreamReader(csvDateiPfad, Encoding.Default))
            {
                //Kopfzeile der csv Datei schreiben
                sb.AppendLine($"{reader.ReadLine()};IstInLoesung");
                //lese Kanten und ergänze Spalte mit Wert 1 für Lösungskanten 0 für Nichtlösungskanten
                while (reader.Peek() >= 0)
                {
                    string zeile = reader.ReadLine().Trim();
                    if (string.IsNullOrWhiteSpace(zeile))
                    {
                        continue;
                    }

                    var spalten = zeile.Split(';');
                    string kantenCode = $"{spalten[1]}_{spalten[2]}";
                    if (codes.Contains(kantenCode))
                    {
                        sb.AppendLine($"{zeile};1");
                        codes.Remove(kantenCode);
                    }
                    else
                    {
                        sb.AppendLine($"{zeile};0");
                    }
                }
            }

            //schreibe Ergebnis in Textdatei
            using (var writer = new StreamWriter(csvDateiPfad.Replace(".csv", "_ergebnis.csv"), false, Encoding.Default))
            {
                writer.Write(sb.ToString());
            }
        }

    }

}
