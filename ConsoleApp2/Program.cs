// See https://aka.ms/new-console-template for more information

using System.Collections.Immutable;
using ConsoleApp2;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Optimization;
using Matrix = MathNet.Numerics.LinearAlgebra.Double.Matrix;
using Spectre.Console;
using Spectre.Console.Rendering;
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
         var test = waterNetwork.readwaternetwork(homePath + @"\data\data3.inp");
         var pipenumbers = test._pipes.Count();
         var graph = new Graph();
         Result rezultat = new Result();
         var initflow1 = graph.createInitFlow(test);
         
         
            rezultat = waterNetwork.waternetSim(test,initflow1);
          //  if(rezultat.flow.Where(x=>x<0).Count()>0){
          //  int order = waterNetwork.orderNetwork(test,rezultat);
        // }

         List<Result> resultat1 = new List<Result>();
         Result rezultat1 = new Result();
         
         var initflow = graph.createInitFlow(test);
         
        /* for (int i = 0; i < 100; i++)
         {
            rezultat1 = graph.findresult(A1matrix, A2matrix, Gmatrix, elevation, demand, initflow);
            initflow = rezultat1.flow;
            waterNetwork.WriteResultsSim(rezultat1,test);
            resultat1.Add(rezultat);
            Gmatrix = graph.CreateGmatrix(test, initflow);
         }
*/
        double sumoferror = 0;
         do
         {  
            sumoferror = 0;
            
            initflow = rezultat1.flow;
            
            rezultat = waterNetwork.waternetSim(test,initflow1);
            //if (rezultat.flow.Where(x => x < 0).Count() != 0)
            //{
            //   waterNetwork.orderNetwork(test, rezultat1);
            //   rezultat = waterNetwork.waternetSim(test, initflow1);
            //}
            resultat1.Add(rezultat);
           
            var iteration = resultat1.Count();
            var pipes = test._pipes.Count();
            if (iteration == 1)
            {
                sumoferror=0.00031;
            }
            else
            {
               for (int i = 0; i < pipes; i++)
               {
                  sumoferror=sumoferror+(rezultat.flow[i] - resultat1[iteration - 2].flow[i]);
               }
            }
            
            initflow1 = rezultat.flow;
         } while (Math.Abs(sumoferror)>0.000003);
            
         
      }
      
     
   }
}