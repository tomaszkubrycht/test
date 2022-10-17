// See https://aka.ms/new-console-template for more information

using System.Collections.Immutable;
using ConsoleApp2;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Matrix = MathNet.Numerics.LinearAlgebra.Double.Matrix;

namespace ConsoleApp2
{
   public class Result
   {
      public Vector<double> head { get; set; }
      public Vector<double> flow { get; set; }
   };

   class Program
   {
      static void Main(string[] args)
      {
         var waterNetwork = new WaterNetwork();
         string? homePath = (Environment.OSVersion.Platform == PlatformID.Unix || 
                             Environment.OSVersion.Platform == PlatformID.MacOSX)
            ? Environment.GetEnvironmentVariable("HOME")
            : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
         var test = waterNetwork.readwaternetwork(homePath+@"/data/data1.inp");
         var pipenumbers = test._pipes.Count();
         var graph = new Graph();
         List<WaterNetwork.Pipe> pipe = new List<WaterNetwork.Pipe>();
         pipe = test._pipes.OrderBy(x=>x.ObjectID).ToList();
         Dictionary<int, WaterNetwork.Node> dictionary = new Dictionary<int, WaterNetwork.Node>();
         Dictionary<int, WaterNetwork.Pipe> dictionary1 = new Dictionary<int, WaterNetwork.Pipe>();
         List<WaterNetwork.Node> nodesnohead = new List<WaterNetwork.Node>();
         var nodesnohead1 = pipe.Select(x=>x.end_node).Where(y=>y._head!=0).ToList();
         var nodesnohead2 = pipe.Select(x=>x.start_node).Where(y=>y._head!=0).ToList();
         nodesnohead1.AddRange(nodesnohead2);
         nodesnohead= nodesnohead1.Distinct().OrderBy(x=>x._nodeid).ToList();
         for (int i = 0; i < pipe.Count; i++)
         {
            dictionary1.Add(i,pipe[i]);
         }
         for (int i = 0; i < nodesnohead.Count(); i++)
         {
            dictionary.Add(i,nodesnohead[i]);
         }
         
         

         double[,] arrayA1 = new double[pipe.Count(),nodesnohead.Count()];
         
         
         for (int i = 0; i < nodesnohead.Count(); i++)
         {
            for (int j = 0; j < pipe.Count(); j++)
            {
               if (pipe[j].start_node._nodeid==nodesnohead[i]._nodeid)
               {
                  arrayA1[i, j] = 1;
               } else if ((pipe[j].end_node._nodeid == nodesnohead[i]._nodeid))
               {
                  arrayA1[i, j] = -1;
               }
               else
               {
                  arrayA1[i, j] = 0;
               };
               
            }
         }

         var A1matrix = Matrix.Build.DenseOfArray(arrayA1);
         Console.WriteLine(A1matrix.ToString());
         var matrixA1 = graph.CreateAdjMatrix(test);
         var matrixA2 = graph.CreateA2Matrix(test);
         Console.Write(matrixA1.ToString());
         Console.Write(matrixA2.ToString());
         var initflow = graph.createInitFlow(test);
         var Gmatrix = graph.CreateGmatrix(test,initflow);
         var elevation = graph.createElevationVect(test);
         var demand = graph.createDemandvect(test);
        ///ustawic macierz
        
        Result result = graph.findresult(matrixA1, matrixA2, Gmatrix, elevation, demand, initflow);
      }
   }
}