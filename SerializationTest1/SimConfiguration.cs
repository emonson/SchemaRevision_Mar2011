using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml.Serialization;

namespace SerializationTest1
{
    public class SimConfiguration
    {
        public string experiment_name { get; set; }
        public string description { get; set; }
        public Scenario scenario { get; set; }
        public Parameters parameters { get; set; }

        public SimConfiguration()
        {
            experiment_name = "Experiment1";
            description = "Whole sim config description";
            scenario = new Scenario();
            parameters = new Parameters();
        }
    }

    public class Scenario
    {
        public string description {get; set;}
        public Experiment experiment { get; set; }
        public Environment environment { get; set; }
        private ObservableCollection<Region> _regions = new ObservableCollection<Region>();
        public ObservableCollection<Region> regions { get { return _regions; } set { _regions = value; } }
        private List<Solfac> _solfacs = new List<Solfac>();
        public List<Solfac> solfacs { get { return _solfacs; } set { _solfacs = value; } }
        private List<CellSet> _cellsets = new List<CellSet>();
        public List<CellSet> cellsets { get { return _cellsets; } set { _cellsets = value; } }

        public Scenario()
        {
            description = "Scenario description";
            experiment = new Experiment();
            environment = new Environment();
            // Regions list is empty by default
        }
    }

    public class Parameters
    {
        public string description { get; set; }
        public ForceParams force_params { get; set; }
        public MotileCellParams motile_cell_params { get; set; }

        public Parameters()
        {
            description = "Default parameters description";
            force_params = new ForceParams();
            motile_cell_params = new MotileCellParams();
        }
    }

    public class Experiment
    {
        public double duration { get; set; }
        public double timestep { get; set; }

        public Experiment()
        {
            duration = 100;
            timestep = 3;
        }
    }

    public class Environment
    {
        public int extent_x { get; set; }
        public int extent_y { get; set; }
        public int extent_z { get; set; }

        public Environment()
        {
            extent_x = 400;
            extent_y = 400;
            extent_z = 400;
        }
    }

    public class Region
    {
        public enum Shape { Rectangular, Ellipsoid }
        public string region_name { get; set; }
        public Shape region_type { get; set; }
        private double[][] _transform = {
                new double[]{1.0, 0.0, 0.0, 0.0},
                new double[]{0.0, 1.0, 0.0, 0.0},
                new double[]{0.0, 0.0, 1.0, 0.0},
                new double[]{0.0, 0.0, 0.0, 1.0} };
        public double[][] transform { get { return _transform; } set { _transform = value; } }

        public Region()
        {
            region_name = "Default Region";
            region_type = Shape.Ellipsoid;
        }

        public Region(string name, Shape type)
        {
            region_name = name;
            region_type = type;
        }
    }

    public class CellSet
    {
        public enum RelativePosition { Inside, Surface, Outside }

        public string cell_name { get; set; }
        public string cell_type { get; set; }
        public int number { get; set; }
        // TODO: Need to abstract out positioning to include pos specification for single cell...
        public string region { get; set; }
        public RelativePosition wrt_region { get; set; }

        public CellSet()
        {
            cell_name = "Default Cell";
            cell_type = "MotileCell";
            number = 100;
            region = "Sphere";
            wrt_region = RelativePosition.Inside;
        }
    }


    // SOLFACS ==================================
    public class Solfac
    {
        public string solfac_name  { get; set; }
        public string solfac_type { get; set; }
        public SolfacDistribution solfac_distribution { get; set; }
        private bool _is_time_varying = false;
        public bool is_time_varying { get { return _is_time_varying; } set { _is_time_varying = value; } }
        private List<TimeAmpPair> _amplitude_keyframes = new List<TimeAmpPair>();
        public List<TimeAmpPair> amplitude_keyframes
        { 
            get { return _amplitude_keyframes; }
            set { _amplitude_keyframes = value; }
        }

        public Solfac()
        {
            solfac_name = "Default Solfac";
            solfac_type = "ccr7";
            // Default is static homogeneous level
            // solfac_profile = new SolfacStaticProfile();
            solfac_distribution = new SolfacHomogeneousLevel();
        }
    }

    // Base class for homog, linear, gauss distributions
    [XmlInclude(typeof(SolfacHomogeneousLevel)),
     XmlInclude(typeof(SolfacLinearGradient)),
     XmlInclude(typeof(SolfacGaussianGradient))]
    public abstract class SolfacDistribution
    {
        public string distribution_name;

        public SolfacDistribution()
        {
            distribution_name = "BaseDistributionClass";
        }
    }

    public class SolfacHomogeneousLevel : SolfacDistribution
    {
        public double concentration { get; set; }

        public SolfacHomogeneousLevel()
        {
            distribution_name = "Homogeneous";
            concentration = 100.0;
        }
    }

    public class SolfacLinearGradient : SolfacDistribution
    {
        private double[] _gradient_direction = new double[3] { 1.0, 0.0, 0.0 };
        public double[] gradient_direction { get { return _gradient_direction; } set { _gradient_direction = value; } }
        private List<PosConcPair> _concentration_levels = new List<PosConcPair>();
        public List<PosConcPair> concentration_levels { get { return _concentration_levels; } set { _concentration_levels = value; } }

        public SolfacLinearGradient()
        {
            distribution_name = "Linear Gradient";
            // Want a default of one level on each end
            concentration_levels.Add(new PosConcPair(0, 100));
            concentration_levels.Add(new PosConcPair(1, 200));
        }
    }

    public class SolfacGaussianGradient : SolfacDistribution
    {
        // Need to store a transform for the widget, but not clear
        // right now how that will be related to the sigmas in each
        // direction, or instead to a covariance matrix
        private double[][] _transform = {
                new double[]{1.0, 0.0, 0.0, 0.0},
                new double[]{0.0, 1.0, 0.0, 0.0},
                new double[]{0.0, 0.0, 1.0, 0.0},
                new double[]{0.0, 0.0, 0.0, 1.0} };
        public double[][] transform { get { return _transform; } set { _transform = value; } }
        public double peak_concentration;
        private double[] _gauss_sigma = new double[3] { 1.0, 1.0, 1.0 };
        public double[] gauss_sigma { get { return _gauss_sigma; } set { _gauss_sigma = value; } }

        public SolfacGaussianGradient()
        {
            distribution_name = "Gaussian Gradient";
            peak_concentration = 100.0;
        }
    }


    // UTILITY CLASSES =======================
    public class PosConcPair
    {
        private double _relative_pos;
        public double relative_pos { 
            get { return _relative_pos; }
            set
            {
                if (value >= 0.0 && value <= 1.0)
                {
                    _relative_pos = value;
                }
            }
        }
        private double _concentration;
        public double concentration {
            get { return _concentration; }
            set
            {
                if (value >= 0.0)
                {
                    _concentration = value;
                }
            }
        }

        public PosConcPair()
        {
            relative_pos = 0.0;
            concentration = 0.0;
        }

        public PosConcPair(double rp, double c)
        {
            relative_pos = rp;
            concentration = c;
        }
    }

    public class TimeAmpPair
    {
        // Not clear whether this should be time step or real time value...
        private double _time_value;
        public double time_value
        {
            get { return _time_value; }
            set
            {
                if (value >= 0.0)
                {
                    _time_value = value;
                }
            }
        }
        private double _amplitude;
        public double amplitude
        {
            get { return _amplitude; }
            set
            {
                if (value >= 0.0 && value <= 1.0)
                {
                    _amplitude = value;
                }
            }
        }

        public TimeAmpPair()
        {
            time_value = 0.0;
            amplitude = 0.0;
        }

        public TimeAmpPair(double ts, double a)
        {
            time_value = ts;
            amplitude = a;
        }
    }


    // SIM PARAMETERS CLASSES ================================
    public class ForceParams
    {
        public double force_delta { get; set; }
        public double force_phi1 { get; set; }
        public double force_phi2 { get; set; }

        public ForceParams()
        {
            force_delta = 12.5;
            force_phi1 = 1.0;
            force_phi2 = 1.0;
        }
    }

    public class MotileCellParams
    {
        public LocomotorParams mc_locomotor { get; set; }
        public CkReceptorParams mc_ck_receptor { get; set; }
        public CkReceptorInitParams mc_ck_receptor_init { get; set; }

        public MotileCellParams()
        {
            mc_locomotor = new LocomotorParams();
            mc_ck_receptor = new CkReceptorParams();
            mc_ck_receptor_init = new CkReceptorInitParams();
        }
    }

    public class LocomotorParams
    {
        public double loco_gamma { get; set; }
        public double loco_sigma { get; set; }
        public double loco_zeta { get; set; }
        public double loco_chi { get; set; }

        public LocomotorParams()
        {
            loco_gamma = 1.0;
            loco_sigma = 1.0;
            loco_zeta = 1.0;
            loco_chi = 1.0;
        }
    }

    public class CkReceptorParams
    {
        public double ckr_kappa { get; set; }
        public double ckr_pi { get; set; }
        public double ckr_tau { get; set; }
        public double ckr_delta { get; set; }

        public CkReceptorParams()
        {
            ckr_kappa = 1.0;
            ckr_pi = 1.0;
            ckr_tau = 1.0;
            ckr_delta = 1.0;
        }
    }

    public class CkReceptorInitParams
    {
        public double ckri_u { get; set; }

        public CkReceptorInitParams()
        {
            ckri_u = 1.0;
        }
    }

}
