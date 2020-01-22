using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SSD2019.Models
{
    public class GAPclass
    {
        public int n;  // numero clienti
        public int m;  // numero magazzini
        public double[,] c;  // costi assegnamento
        public int[] req;    // richieste clienti
        public int[] cap;    // capacità magazzini

        public int[] sol, solbest;    // per ogni cliente, il suo magazzino, array di 52 celle in cui c'è scritto a quale magazzino sono assegnati i magazzini
        public double zub, zlb; //zub = costo della migliore soluzione possibile trovata, quindi solbest

        const double EPS = 0.0001;
        System.Random rnd = new Random(550);

        public GAPclass()
        {
            zub = double.MaxValue; //z è il costo della miglior soluzione trovata (z upper bound)
            zlb = double.MinValue; //z lower bound è nel caso avessimo lower bound e all'inizio qnd nn ne so niente sono min e max value
        }

        public double simpleConstruct()
        {
            int[] capleft = new int[cap.Length], ind = new int[m];
            int i, j, ii;
            double[] dist = new double[m];
            Array.Copy(cap, capleft, cap.Length);
            zub = 0;
            for (j = 0; j < n; j++) //j = client
            {
                for (i = 0; i < m; i++)
                {
                    dist[i] = c[i, j];
                    ind[i] = i;
                }
                Array.Sort(dist, ind);
                ii = 0;
                while (ii < m)
                {
                    i = ind[ii];
                    if (capleft[i] >= req[j])
                    {
                        sol[j] = i;
                        capleft[i] -= req[j];
                        zub += c[i, j];
                        break;
                    }
                    ii++;
                }
                if (ii == m)
                {
                    Trace.WriteLine("[Simple Construct] Ahi Ahi. ");
                }
            }
            return zub;
        }

        public double opt10(double[,] cost)
        {

            int[] capres = new int[cap.Length];
            int i, j, isol;
            double z;
            double[] dist = new double[m];
            Array.Copy(cap, capres, cap.Length);
            z = 0;
            for (j = 0; j < n; j++)
            {
                capres[sol[j]] -= req[j];
                z += cost[sol[j], j];
            }
        l0: for (j = 0; j < n; j++)
            {
                isol = sol[j];
                for (i = 0; i < m; i++)
                {
                    if (i == isol) continue;
                    if (cost[i, j] < cost[isol, j] && capres[i] >= req[j])
                    {
                        sol[j] = i;
                        capres[i] -= req[j];
                        capres[isol] += req[j];
                        z -= (cost[isol, j] - cost[i, j]);
                        if (z < zub) zub = z;
                        goto l0;
                    }
                }
            }
            return z;
        }

        public double tabuSearch(int Ttenure, int maxIter)
        {
            int[,] TL = new int[m, n];
            int[] capres = new int[cap.Length];
            int i, j, isol, imax, jmax, iter;
            double z, deltaMax;

            Array.Copy(cap, capres, cap.Length);
            for (j = 0; j < n; j++)
            {
                capres[sol[j]] -= req[j];
            }
            z = zub;
            iter = 0;
            for (i = 0; i < m; i++)
            {
                for (j = 0; j < n; j++)
                {
                    TL[i, j] = int.MinValue;
                }
            }
            Trace.WriteLine("Starting Tabu Search");
        l1: deltaMax = imax = jmax = int.MinValue;
            iter++;
            for (j = 0; j < n; j++)
            {
                isol = sol[j];
                for (i = 0; i < m; i++)
                {
                    if (i == isol) continue;
                    if ((c[isol, j] - c[i, j] > deltaMax) && (capres[i] >= req[j]) && (TL[i, j] + Ttenure) < iter)
                    {
                        imax = i;
                        jmax = j;
                        deltaMax = c[isol, j] - c[i, j];
                    }
                }
            }
            isol = sol[jmax];
            sol[jmax] = imax;
            capres[imax] -= req[jmax];
            capres[isol] += req[jmax];
            z -= deltaMax;
            if (z < zub)
            {
                zub = z;
            }
            TL[imax, jmax] = iter;
            if (iter % 100 == 0)
            {
                Trace.WriteLine("Tabu Search z = " + z + " iter = " + iter + " deltaMax = " + deltaMax);
            }
            if (iter < maxIter)
            {
                goto l1;
            }
            else
            {
                Trace.WriteLine("Tabu search ended ");
            }

            double zCheck = 0;
            for (j = 0; j < n; j++)
            {
                zCheck += c[sol[j], j];
            }
            if (Math.Abs(zCheck - z) > EPS)
            {
                Trace.WriteLine("Tabu Search - problems");
            }
            return zub;
        }
    }
}