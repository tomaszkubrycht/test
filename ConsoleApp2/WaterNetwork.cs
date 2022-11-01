using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using PriorityQueues;
using Spectre.Console;

namespace ConsoleApp2;

public class WaterNetwork
{

    public class Waternetwork
    {
        public string waternetwork_id { get; set; }
        public List<Pipe> _pipes { get; set; }
    }

    public class Pipe
    {
        private static int Junktionnr = 0;
        private int _objectID;
        public double flow;

        public int ObjectID
        {
            get { return _objectID; }
        }
        public Pipe()
        {
            Junktionnr++;
            _objectID = Junktionnr;
        }
        public string reversedirection { get; set; }
        public string pump_parameter { get; set; }
        public double Roughness { get; set; }
        public double _diameter { get; set; }
        public double lenght { get; set; }
        public string pipe_id { get; set; }
        public Node start_node { get; set; }
        public Node end_node { get; set; }
        public double MinorLoss { get; set; }
        public string Status { get; set; }
        public string Type { get; set; } = null!;

        public Dictionary<string, int> _status => new Dictionary<string, int>()
        {
            { "Open", 1 },
            { "Close", 2 },
            { "Not Defined", 3 },
        };

        public int _nrru { get; set; }
    }

    public class Node
    {
        private static int Nodenr = 0;
        private int _objectID;
        public int ObjectID
        {
            get { return _objectID; }
        }
        public Node()
        {
            Nodenr++;
            _objectID = Nodenr;
        }
        public string Nodename { get; set; }

        
        public double Roughness { get; set; }
        public string Status { get; set; }
        public double _initlevel { get; set; }
        public string _type { get; set; }
        public double _reshead { get; set; }
        public string _respatern { get; set; }
        public string node_id { get; set; }
        public string _name { get; set; }
        public List<double> _demand { get; set; }
        public double _z { get; set; }
        public Coordinate coordinates { get; set; }
        public double MinLevel { get; set; }
        public double MaxLevel { get; set; }
        public double tn_Diameter { get; set; }
        public double MinVol { get; set; }
        public double Length { get; set; }
        public double _pipediameter { get; set; }
        public double MinorLoss { get; set; }
        public int _nodeid { get; set; }
        public double _head { get; set; }
    }

    public class Coordinate
    {
        public string nodename { get; set; }
        public double _x { get; set; }
        public double _y { get; set; }
    }

    public Waternetwork readwaternetwork(string filename)
    {
        Waternetwork waterNetwork = new Waternetwork();
        waterNetwork = Readtopology(filename);
        return waterNetwork;
    }
    
 
 
   

    public static Dictionary<string, double> CooeficientHW = new Dictionary<string, double>();
    private static Dictionary<string, List<string>> _arrayLists = new Dictionary<string, List<string>>();
    private static Dictionary<string, double> coefficientDarcy = new Dictionary<string, double>();

    private Waternetwork Readtopology(string filename)
    {
//private static Dictionary<string, List<string>> _arrayLists = new Dictionary<string, List<string>>();\
        Waternetwork waternet = new Waternetwork();
        StreamReader reader = new StreamReader(filename);
        string line;
        string category = "";

        List<Node> _node = new List<Node>();
        List<Pipe> _Links = new List<Pipe>();
        while (null != (line = reader.ReadLine()))
        {
            if (line.Contains(value: "["))
            {

                int pFrom = line.IndexOf("[") + "[".Length;
                int pTo = line.LastIndexOf("]");

                String result = line.Substring(pFrom, pTo - pFrom);

                category = result.Trim();
            }
            else
            {
                if (!_arrayLists.ContainsKey(category))
                {
                    List<string> stringList = new List<string>();
                    _arrayLists.Add(category, stringList);
                }

                if ((!line.ToLower().Contains("end") && (line.Trim().Length > 0)))
                {
                    _arrayLists[category].Add(line.Trim());
                }
            }
        }

        List<Coordinate> coordinateList = new List<Coordinate>();
        var coordinates = _arrayLists["COORDINATES"];
        foreach (var item in coordinates.Skip(1))
        {
            Coordinate coordinate1 = new Coordinate();
            var line2 = item.Split("\t");
            coordinate1.nodename = line2[0].Trim(' ');
            coordinate1._x = Convert.ToDouble(line2[1].Trim(' ').Replace('.', ','));
            coordinate1._y = Convert.ToDouble(line2[2].Trim(' ').Replace('.', ','));
            coordinateList.Add(coordinate1);
        }

        var _tanks = _arrayLists["TANKS"];

        foreach (var item in _tanks.Skip(1))
        {
            Node tank = new Node();
            String[] line1 = item.Split('\t');
            tank._name = line1[0].Trim(' ');
            tank._type = "Tank";

            tank._z = Convert.ToDouble(line1[1].Trim(' ').Replace('.', ','));
            tank._initlevel = Convert.ToDouble(line1[2].Trim(' ').Replace('.', ','));
            tank.MinLevel = Convert.ToDouble(line1[3].Trim(' ').Replace('.', ','));
            tank.MaxLevel = Convert.ToDouble(line1[4].Trim(' ').Replace('.', ','));
            tank.tn_Diameter = Convert.ToDouble(line1[5].Trim(' ').Replace('.', ','));
            tank.MinVol = Convert.ToDouble(line1[5].Trim(' ').Replace('.', ','));
            tank.coordinates = coordinateList.Where(x => x.nodename == tank._name).First();
            tank._nodeid = _node.Count();
            _node.Add(tank);
        }

        var _reservoir = _arrayLists["RESERVOIRS"];
        var resid = 0;
        foreach (var item in _reservoir.Skip(1))
        {
            Node reservoir = new Node();
            String[] line1 = item.Split("\t");
            reservoir._name = line1[0].Trim(' ');
            reservoir._head = Convert.ToDouble(line1[1].Trim(' ').Replace('.', ','));
            reservoir._respatern = line1[2].Trim(' ');
            reservoir._type = "Reservoirs";
            reservoir.coordinates = coordinateList.Where(x => x.nodename == reservoir._name).First();
            reservoir._nodeid = _node.Count();
            _node.Add(reservoir);
        }

        var pumps = _arrayLists["PUMPS"];
        foreach (var item in pumps.Skip(1))
        {
            Pipe pump = new Pipe();
            String[] line1 = item.Split("/t");
            pump.pipe_id = line1[0].Trim(' ');
            pump.start_node = _node.Where(x => x._name == line1[1].Trim(' ')).First();
            pump.end_node = _node.Where(x => x._name == line1[2].Trim(' ')).First();
            pump.pump_parameter = line1[3].Trim(' ');
            pump.Type = "Pump";
            pump._status.Add("open", 1);

            _Links.Add(pump);
        }

        var junctions = _arrayLists["JUNCTIONS"];
        int number = new int();
        foreach (var value in junctions.Skip(1))
        {
            WaterNetwork.Node node = new WaterNetwork.Node();
            String[] line1 = value.Split("\t");
            node._name = line1[0].Trim(' ');
            //node._z = line1[1].Trim(' ');
            node._z = Convert.ToDouble(line1[1].Trim(' ').Replace('.', ','));
            List<double> demand = new List<double>();
            //var demand1 = Convert.ToDouble(line1[2].Trim(' '));
            demand.Add(Convert.ToDouble(line1[2].Replace('.', ',').Trim(' ')));
            node._demand = demand;
            node.coordinates = coordinateList.Where(x => x.nodename == node._name).First();
            node._nodeid++;
            node._type = "Junction";
            node._nodeid = _node.Count();
            _node.Add(node);
            ;
        }

        int nrRury = 0;
        var links = _arrayLists["PIPES"];
        foreach (var _links in links.Skip(1))
        {
            Pipe pipe = new Pipe();
            if (pipe == null) throw new ArgumentNullException(nameof(pipe));
            String[] line1 = _links.Split("\t");
            pipe.pipe_id = line1[0].Trim(' ');
            pipe.start_node = _node.Where(x => x._name == line1[1].Trim(' ')).First();
            pipe.end_node = _node.Where(x => x._name == line1[2].Trim(' ')).First();
            pipe.lenght = Convert.ToDouble(line1[3].Trim(' ').Replace('.', ','));
            pipe._diameter = Convert.ToDouble(line1[4].Trim(' ').Replace('.', ','));
            //pipe.Roughness = Convert.ToDouble(line1[5].Trim(' ').Replace('.', ','));
            pipe.Roughness = 0.022;
            pipe.MinorLoss = Convert.ToDouble(line1[6].Trim(' ').Replace('.', ','));
            pipe.Status = line1[7].Trim(' ');
            pipe.Type = "Pipe";
            pipe._nrru = _Links.Count();

            _Links.Add(pipe);
        }

        //WaterNetwork.Pipe = _Links;
        waternet._pipes = _Links;

        return (waternet);
    }
    public void WriteResultsSim(Result rezultat, Waternetwork waternetwork)
        {
            var pipes=waternetwork._pipes.OrderBy(x => x.ObjectID).ToList();
            for (int i = 0; i < rezultat.flow.Count(); i++)
            {
                pipes[i].flow = rezultat.flow[i];
                AnsiConsole.WriteLine("[red]" + pipes[i].ObjectID + "[/]"+pipes[i].flow);
            }

            foreach (var item in waternetwork._pipes)
            {
                if (item.flow<0)
                {
                    item.reversedirection = "T";
                    var start = item.start_node;
                    var end = item.end_node;
                    item.start_node = end;
                    item.end_node = start;
                }
            }
        }

    public int orderNetwork(Waternetwork test, Result rezultat)
    {
        var pipes=test._pipes.OrderBy(x => x.ObjectID).ToList();
        foreach (var item in test._pipes)
        {
            if (item.flow<0)
            {
                if (item.reversedirection == "T")
                {
                    item.reversedirection = "N";
                }
                else
                {
                    item.reversedirection = "T";
                }

                var start = item.start_node;
                var end = item.end_node;
                item.start_node = end;
                item.end_node = start;
            }
        }

        return 0;
    }

    public Matrix<double> createA2matrix(Waternetwork test)
    {
        List<WaterNetwork.Pipe> pipe = new List<WaterNetwork.Pipe>();
        pipe = test._pipes.OrderBy(x => x.ObjectID).ToList();
        Dictionary<int, WaterNetwork.Node> dictionary = new Dictionary<int, WaterNetwork.Node>();
        Dictionary<int, WaterNetwork.Pipe> dictionary1 = new Dictionary<int, WaterNetwork.Pipe>();
        List<WaterNetwork.Node> nodesnohead = new List<WaterNetwork.Node>();
         
        var nodesnohead1 = pipe.Select(x => x.end_node).Where(y => y._head != 0).ToList();
        var nodesnohead2 = pipe.Select(x => x.start_node).Where(y => y._head != 0).ToList();
        nodesnohead1.AddRange(nodesnohead2);
        nodesnohead = nodesnohead1.Distinct().OrderBy(x => x._nodeid).ToList();
        for (int i = 0; i < pipe.Count; i++)
        {
            dictionary1.Add(i, pipe[i]);
        }
        for (int i = 0; i < nodesnohead.Count(); i++)
        {
            dictionary.Add(i, nodesnohead[i]);
        }
        double[,] arrayA2 = new double[pipe.Count, nodesnohead.Count()];
        for (int i = 0; i < pipe.Count; i++)
        {
            for (int j = 0; j < nodesnohead.Count(); j++)
            {
                if (pipe[i].start_node._nodeid == nodesnohead[j]._nodeid)
                {
                    arrayA2[i, j] = 1;
                }
                else if ((pipe[i].end_node._nodeid == nodesnohead[j]._nodeid))
                {
                    arrayA2[i, j] = -1;
                }
                else
                {
                    arrayA2[i, j] = 0;
                }
            }
        }

        Matrix<double> A2matrix = Matrix.Build.DenseOfArray(arrayA2);
        return A2matrix;
    }

    public Matrix<double> createA1matrix(Waternetwork test)
    {
        
        List<WaterNetwork.Pipe> pipe = new List<WaterNetwork.Pipe>();
        pipe = test._pipes.OrderBy(x => x.ObjectID).ToList();
        List<WaterNetwork.Node> nodeshead = new List<WaterNetwork.Node>();
        Dictionary<int, WaterNetwork.Node> dictionaryhead = new Dictionary<int, WaterNetwork.Node>();
        var nodeshead1 = pipe.Select(x => x.end_node).Where(y => y._head == 0).ToList();
        var nodeshead2 = pipe.Select(x => x.start_node).Where(y => y._head == 0).ToList();
        nodeshead1.AddRange(nodeshead2);
        nodeshead = nodeshead1.Distinct().OrderBy(x => x._nodeid).ToList();

        for (int i = 0; i < nodeshead.Count(); i++)
        {
            dictionaryhead.Add(i, nodeshead[i]);
        }
        double[,] arrayA1 = new double[pipe.Count(), nodeshead.Count()];
        for (int i = 0; i < nodeshead.Count(); i++)
        {
            for (int j = 0; j < pipe.Count(); j++)
            {
                if (pipe[j].start_node._nodeid == nodeshead[i]._nodeid)
                {
                    arrayA1[j, i] = 1;
                }
                else if ((pipe[j].end_node._nodeid == nodeshead[i]._nodeid))
                {
                    arrayA1[j, i] = -1;
                }
                else
                {
                    arrayA1[j, i] = 0;
                }
            }
        }
        var A1matrix = Matrix.Build.DenseOfArray(arrayA1);
        return A1matrix;
    }

    public Result waternetSim(Waternetwork test, Vector<double> initflow1)
    {
        var waterNetwork = new WaterNetwork();
        var graph = new Graph();
        
        var A2matrix1 = waterNetwork.createA2matrix(test);
        var A1matrix1 = waterNetwork.createA1matrix(test);
        var Gmatrix1 = graph.CreateGmatrix(test, initflow1);
        var elevation1 = graph.createElevationVect(test);
        var demand1 = graph.createDemandvect(test);
        
        return graph.findresult(A1matrix1, A2matrix1, Gmatrix1, elevation1, demand1, initflow1);
    }
}

