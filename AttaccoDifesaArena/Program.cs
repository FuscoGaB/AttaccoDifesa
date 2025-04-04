using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AttaccoDifesaArena
{
    internal class Program
    {
        static Semaphore semaforoArena = new Semaphore(2, 2);
        static List<Giocatore> listaGiocatori = new List<Giocatore>();
        static Random casualeGlobale = new Random();
        static List<Giocatore> arena = new List<Giocatore>();
        static void Main(string[] args)
        {
            for (int i = 1; i <= 10; i++)
            {
                listaGiocatori.Add(new Giocatore($"G{i}"));
            }

            Console.WriteLine("Combattimento iniziato!");

            List<Thread> listaThread = new List<Thread>();
            foreach (var g in listaGiocatori)
            {
                Thread thread = new Thread(() => CicloGiocatore(g));
                listaThread.Add(thread);
                thread.Start();
            }

            foreach (var thread in listaThread)
            {
                thread.Join();
            }

            if (listaGiocatori.Count == 1)
            {
                Console.WriteLine($"Vincitore: {listaGiocatori[0].Nome}");
            }
            else
            {
                Console.WriteLine("Nessun vincitore!");
            }
            Console.ReadKey();
        }

        static void CicloGiocatore(Giocatore giocatore)
        {
            while (giocatore.PV > 0 && listaGiocatori.Count > 1)
            {
                semaforoArena.WaitOne();
                arena.Add(giocatore);

                while (arena.Count < 2)
                {
                    Thread.Sleep(500);
                }

                if (giocatore == arena[0])
                {
                    Console.WriteLine($"{arena[0].Nome} vs {arena[1].Nome}");
                    Combattimento(arena[0], arena[1]);

                    if (arena[0].PV <= 0) listaGiocatori.Remove(arena[0]);
                    if (arena[1].PV <= 0) listaGiocatori.Remove(arena[1]);

                    arena.Clear();
                    Console.WriteLine($"Giocatori rimasti: {listaGiocatori.Count}");
                }

                semaforoArena.Release();
            }
        }

        static void Combattimento(Giocatore attaccante, Giocatore difensore)
        {
            while (attaccante.PV > 0 && difensore.PV > 0)
            {
                int danno = casualeGlobale.Next(1, 101);

                if (difensore.ProvaParata())
                {
                    double percentualeBlocco = casualeGlobale.NextDouble() * 0.5 + 0.5;
                    danno = (int)(danno * (1 - percentualeBlocco));
                }

                difensore.PV -= danno;
                Console.WriteLine($"{attaccante.Nome} -> {difensore.Nome} : {danno} danni ({difensore.PV} PV)");

                if (difensore.PV <= 0) break;

                Giocatore temp = attaccante;
                attaccante = difensore;
                difensore = temp;
            }

            Console.WriteLine($"Vince: {(attaccante.PV > 0 ? attaccante.Nome : difensore.Nome)}");
        }
    }

    public class Giocatore
    {
        public string Nome { get; }
        public int PV { get; set; } // Punti Vita
        public int PercentualeDifesa { get; }

        public Giocatore(string nome)
        {
            Nome = nome;
            var casuale = new Random(Guid.NewGuid().GetHashCode());
            PV = casuale.Next(200, 501);
            PercentualeDifesa = casuale.Next(1, 101);
        }

        public bool ProvaParata()
        {
            var casuale = new Random(Guid.NewGuid().GetHashCode());
            return casuale.NextDouble() >= (PercentualeDifesa / 100.0);
        }
    }
}