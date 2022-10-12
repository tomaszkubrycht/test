// See https://aka.ms/new-console-template for more information

using System.Collections.Immutable;
using ConsoleApp2;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Matrix = MathNet.Numerics.LinearAlgebra.Complex.Matrix;

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
         var test = waterNetwork.readwaternetwork(@"c:\data\data1.inp");
         var pipenumbers = test._pipes.Count();
         var graph = new Graph();
         var matrixA1 = graph.CreateAdjMatrix(test);
         var matrixA2 = graph.CreateA2Matrix(test);
         Console.Write(matrixA1.ToString());
         Console.Write(matrixA2.ToString());
         var initflow = graph.createInitFlow(test);
         var Gmatrix = graph.CreateGmatrix(test,initflow);
         var elevation = graph.createElevationVect(test);
         var demand = graph.createDemandvect(test);

        do
        {
           Result result = graph.findresult(matrixA1, matrixA2, Gmatrix, elevation, demand, initflow);
           initflow = result.flow;
           
           Gmatrix = graph.CreateGmatrix(test, initflow);

        } while (true);
        //Result result = graph.findresult(matrixA1, matrixA2, Gmatrix, elevation, demand, initflow);
      }
   }
}