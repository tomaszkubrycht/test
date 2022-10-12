using System.Collections;
using System.IO.Compression;
using System.Xml;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Double;
using Matrix = MathNet.Numerics.LinearAlgebra.Complex.Matrix;
using Vector = MathNet.Numerics.LinearAlgebra.Double.Vector;

namespace ConsoleApp2;


public class Graph
{

    public class pipes
    {
        public double nr { get; set; }
        public double startnode { get; set; }
        public double endnode { get; set; }
    }

    public Dictionary<int, WaterNetwork.Pipe> Pipes = new Dictionary<int, WaterNetwork.Pipe>();
    public Matrix<double> CreateAdjMatrix(WaterNetwork.Waternetwork waterNetwork)
    {
       
        
        var tom = waterNetwork._pipes.Where(a=>a.Type=="Pipe").Select(x => x.end_node).Where(y=>(y._head==0)).ToList();
        var tom2 = waterNetwork._pipes.Where(a=>a.Type=="Pipe").Select(x => x.start_node).Where(y=>y._head==0).ToList();
        tom.AddRange(tom2);
        var nwezel = tom.Distinct().OrderBy(x => x._name).ToList();
        //wybrac wsztstkie rurociagi w ktorych nie  
        var rury = waterNetwork._pipes.Distinct().OrderBy(x=>x._nrru).ToList();
        var rurycount = rury.Count();
        var nwezelcount = nwezel.Count();
        var A1 = new double[rurycount,nwezelcount];
        //var rura = waterNetwork._pipes.Where(x => x._nrru == i).First();
                       //var marA1 = Matrix<double>.Build.DenseOfArray(new double[,] { });
                       WaterNetwork.Pipe[] lista_rur = waterNetwork._pipes
                           .Where(x => ((x.end_node._head == 0) | (x.start_node._head == 0)) & (x.Type == "Pipe"))
                           .OrderBy(x => x._nrru).ToArray();
                       
        
        for (int i = 0; i <rurycount ; i++)
        {
            var hashCode = lista_rur[i].GetHashCode();
           

        } 
        
        for (int i = 0; i < rurycount; i++)
        {
            var rura = waterNetwork._pipes.Where(x => x._nrru == i).First();
            for (int j = 0; j < nwezelcount; j++)
            {
                if (rura.start_node._nodeid==j)
                {
                    A1[i,j]=(1);
                }
                else if (rura.start_node._nodeid == j)
                {
                    A1[i,j]=(-1);
                }
                else
                {
                    A1[i,j]=(0);
                }
            }
           // var marA1=Matrix<double>.Build.DenseOfArray(new double[,] { A1 });
        }
        var marA1=Matrix<double>.Build.DenseOfArray(A1);
        Console.Write(marA1.ToString());
        return marA1;
    }
    
    public void print_matrix(int [,] matrix)
    {
        FileStream stream = new FileStream(@"c:\data\A1.txt", FileMode.Truncate);  
// Create a StreamWriter from FileStream  
        using (StreamWriter writer = new StreamWriter(stream))
        {
            writer.WriteLine("A1 Matrix");
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                writer.Write("Nr pipe " + i);
                Console.Write("Nr pipe " + i);
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    writer.Write("|" + matrix[i, j]);
                    Console.Write("|" + matrix[i, j]);
                }

                Console.WriteLine("|");
                writer.WriteLine("|");
            }
        }
    }

    public Matrix<double> CreateA2Matrix(WaterNetwork.Waternetwork test)
    {
        var notnull = test._pipes.Where(x => x.end_node._head != 0).Select(y=>y.end_node).ToList();
        var notnull1 = test._pipes.Where(x => x.start_node._head != 0).Select(y => y.start_node).ToList();
        notnull.AddRange(notnull1);
        var listnotzero = notnull.Distinct().ToList();
        var array = new double [listnotzero.Count(), test._pipes.Count()];
         //var A2matrix = Matrix<double>.Build.DenseOfArray(new double[,] { });
        for (int i = 0; i < listnotzero.Count; i++)
        {
            var rura = test._pipes.Where(x => x._nrru == i).First();
            for (int j = 0; j < test._pipes.Count(); j++)
            {
                if (rura.start_node._nodeid==j)
                {
                    array[i, j] = 1;
                }
                else if (rura.end_node._nodeid == j)
                {
                    array[i, j] = -1;
                }
                else
                {
                    array[i, j] = 0;
                }
            }
        }
        var A2matrix = Matrix<double>.Build.DenseOfArray(array);
        return A2matrix;
    }

    public Matrix<double> CreateGmatrix(WaterNetwork.Waternetwork test,Vector<double> initflow)
    {
        var CountPipes=test._pipes.Where(x => x.Type == "Pipe").Count();
        //var Gmatrix =  Matrix<double>.Build.DenseOfArray(new double[,] { });
        //var initflow=createInitFlow(test);
        var array = new double[CountPipes, CountPipes];
        for (int i = 0; i < CountPipes; i++)
        {
            for (int j = 0; j < CountPipes; j++)
            {
                if (i==j)
                {
                    array[i, j] = Calculatelosses(test,test._pipes[i],initflow[i]);
                }
                else
                {
                    array[i, j] = 0;
                }
            }            
        }
       var Gmatrix= Matrix<double>.Build.DenseOfArray(array); 
       Console.Write(Gmatrix.ToString()); 
       return Gmatrix;
    }
    
    /// <summary>
/// this function aimet to calculate losses for i-Pipe
/// parameters need to calculate are:
/// V - speed of fluid (for first iteration from initial value 
/// D- diameter of pipe
/// Re- reynolds number (from first iteration from initial value v=1.004x10^6 m^2/s at 20 deggre)
/// e- cooeficient of losses
/// f-flow in pipe (for first iteration from initial value
///
/// result is rf
/// 
/// </summary>
/// <param name="test"></param>
/// <returns></returns>
    private double Calculatelosses(WaterNetwork.Waternetwork test,WaterNetwork.Pipe pipe,double pipeflow)
{
    var q = pipeflow;
    var diameter = pipe._diameter/1000;
    var ppow = Math.PI * Math.Pow((diameter / 2),2);
    var v=(q)/(ppow);
    var Re = v * diameter*0.998 / (1.004*Math.Pow(10,-6));
    var fd = 64 / Re;
    var ri = 8 * fd * pipe.lenght / (Math.Pow(Math.PI, 2) * 9.81 * (Math.Pow(pipe._diameter, 5)));
    var epsylon = 0.25;
    var tom1 = Math.Log((epsylon / (3.7 * diameter * 1000))+(5.74 / Math.Pow(Re, 0.9)));
    var fe = 0.25 / (Math.Pow((Math.Log10((epsylon / (3.7 * diameter * 1000)) + (5.74 / (Math.Pow(Re, 0.9))))), 2));
    var rf = (8 * fe * pipe.lenght) / (Math.Pow(Math.PI, 2) * 9.81 * Math.Pow(diameter, 5));
    var gel = 1 / (rf * Math.Abs(pipeflow));
    return gel;
}


    public Vector<double> createElevationVect(WaterNetwork.Waternetwork test)
    {
        var nodes1=test._pipes.Where(y=>y.end_node._type=="Junction").Select(x => x.start_node).ToList();
        var nodes2 = test._pipes.Where(y=>y.start_node._type=="Junction").Select(x => x.end_node).ToList();
        nodes1.AddRange(nodes2);
        List<WaterNetwork.Node> nodeslist = nodes1.Distinct().OrderBy(y=>y._nodeid).ToList();
        var elevation1 = nodeslist.Select(x =>  x._z).ToArray();
        
        
        var elevation = Vector<double>.Build.DenseOfArray(new double[]{80,50});
        return elevation;
    }

    public Vector<double> createDemandvect(WaterNetwork.Waternetwork test)
    {
        var nodes1=test._pipes.Select(x => x.start_node).ToList();
        var nodes2 = test._pipes.Select(x => x.end_node).ToList();
        nodes1.AddRange(nodes2);
        List<WaterNetwork.Node> nodeslist = nodes1.Distinct().OrderBy(y=>y._nodeid).ToList();
        var nulldemand=nodeslist.Where(y => (y._demand) == null).ToList();
        var demand = nodeslist.Where(y=>(y._demand)!=null).Select(x => x._demand.First()).ToArray();
        //var demandvect = Vector<double>.Build.DenseOfArray(new double[] { });
        
        
       
        var demandvect = Vector<double>.Build.DenseOfArray(demand);
        return demandvect;
    }

    public Vector<double> createInitFlow(WaterNetwork.Waternetwork test)
    {
        
        var array = new double[test._pipes.Count()];
        for (int i = 0; i < test._pipes.Count(); i++)
        {
            array[i]=(0.02155);
        }
        var InitFlow = Vector<double>.Build.DenseOfArray(array);
        return InitFlow;
    }
    public Result findresult(Matrix<double> marA1, Matrix<double> a2, Matrix<double> g, Vector<double> elev,
        Vector<double> d, Vector<double> q)
    {
        ConsoleApp2.Result result = new Result();
        var U = 1 / (marA1.Transpose() * g * marA1);
        var U1 = g * a2.Transpose() * elev;
        result.head = (1 / U) * (-d + marA1.Transpose() * ((1 - 2) * q - g * a2.Transpose() * elev));
        result.flow = 0.5 * (q + g * (a2.Transpose() * elev + marA1 * result.head));
        return result;
    }
}