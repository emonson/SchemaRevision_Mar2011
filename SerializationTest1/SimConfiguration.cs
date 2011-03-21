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
        public GlobalParameters global_parameters { get; set; }
        public EntityRepository entity_repository { get; set; }

        public SimConfiguration()
        {
            experiment_name = "Experiment1";
            description = "Whole sim config description";
            scenario = new Scenario();
            global_parameters = new GlobalParameters();
            entity_repository = new EntityRepository();
        }
    }

    public class Scenario
    {
        public string description {get; set;}
        public TimeConfig time_config { get; set; }
        public Environment environment { get; set; }
        private ObservableCollection<Region> _regions = new ObservableCollection<Region>();
        public ObservableCollection<Region> regions { get { return _regions; } set { _regions = value; } }
        private ObservableCollection<Solfac> _solfacs = new ObservableCollection<Solfac>();
        public ObservableCollection<Solfac> solfacs { get { return _solfacs; } set { _solfacs = value; } }
        private ObservableCollection<CellSet> _cellsets = new ObservableCollection<CellSet>();
        public ObservableCollection<CellSet> cellsets { get { return _cellsets; } set { _cellsets = value; } }

        public Scenario()
        {
            description = "Scenario description";
            time_config = new TimeConfig();
            environment = new Environment();
            // Regions list is empty by default
        }
    }

    public class GlobalParameters
    {
        public string description { get; set; }
        public ForceParams force_params { get; set; }

        public GlobalParameters()
        {
            description = "Default parameters description";
            force_params = new ForceParams();
        }
    }

    public class EntityRepository
    {
        private ObservableCollection<SolfacType> _solfac_types = new ObservableCollection<SolfacType>();
        public ObservableCollection<SolfacType> solfac_types { get { return _solfac_types; } set { _solfac_types = value; } }
        private ObservableCollection<CellType> _cell_types = new ObservableCollection<CellType>();
        public ObservableCollection<CellType> cell_types { get { return _cell_types; } set { _cell_types = value; } }
        private ObservableCollection<GaussianGradient> _gaussian_gradients = new ObservableCollection<GaussianGradient>();
        public ObservableCollection<GaussianGradient> gaussian_gradients { get { return _gaussian_gradients; } set { _gaussian_gradients = value; } }

        public EntityRepository()
        {
        }
    }

    public class TimeConfig
    {
        public double duration { get; set; }
        public double timestep { get; set; }

        public TimeConfig()
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
        public bool region_visibility = true;
        public System.Windows.Media.Color region_color = new System.Windows.Media.Color();

        public Region()
        {
            region_name = "Default Region";
            region_type = Shape.Ellipsoid;
            region_color = System.Windows.Media.Color.FromRgb(255, 255, 255); 
        }

        public Region(string name, Shape type)
        {
            region_name = name;
            region_type = type;
            region_color = System.Windows.Media.Color.FromRgb(255, 255, 255);
        }
    }

    public class CellSet
    {
        public enum RelativePosition { Inside, Surface, Outside }

        public string cell_name { get; set; }
        public string cell_type_ref { get; set; }
        public int number { get; set; }
        // TODO: Need to abstract out positioning to include pos specification for single cell...
        public string region_name_ref { get; set; }
        public RelativePosition wrt_region { get; set; }
        public System.Windows.Media.Color cell_color = new System.Windows.Media.Color();

        public CellSet()
        {
            cell_name = "Default Cell";
            cell_type_ref = "MotileCell";
            number = 100;
            region_name_ref = "Default Region";
            wrt_region = RelativePosition.Inside;
            cell_color = System.Windows.Media.Color.FromRgb(255, 255, 255);
        }
    }

    public class CellType
    {
        public string cell_type_name;
        public MotileCellParams cell_type_parameters;

        public CellType()
        {
            cell_type_name = "Default cell type name";
            cell_type_parameters = new MotileCellParams();
        }
    }


    // SOLFACS ==================================
    public class Solfac
    {
        public string solfac_name  { get; set; }
        public string solfac_type_ref { get; set; }
        public SolfacDistribution solfac_distribution { get; set; }
        private bool _is_time_varying = false;
        public bool is_time_varying { get { return _is_time_varying; } set { _is_time_varying = value; } }
        private List<TimeAmpPair> _amplitude_keyframes = new List<TimeAmpPair>();
        public List<TimeAmpPair> amplitude_keyframes
        { 
            get { return _amplitude_keyframes; }
            set { _amplitude_keyframes = value; }
        }
        public System.Windows.Media.Color solfac_color = new System.Windows.Media.Color();

        public Solfac()
        {
            solfac_name = "Default Solfac";
            solfac_type_ref = "ccr7";
            // Default is static homogeneous level
            solfac_distribution = new SolfacHomogeneousLevel();
            solfac_color = System.Windows.Media.Color.FromRgb(255, 255, 255);
        }
    }

    public class SolfacType
    {
        public string solfac_type_name;

        public SolfacType()
        {
            solfac_type_name = "Default solfac type name";
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
        public double min_concentration;
        public double max_concentration;

        public SolfacLinearGradient()
        {
            distribution_name = "Linear Gradient";
            min_concentration = 100.0;
            max_concentration = 250.0;
        }
    }

    public class SolfacGaussianGradient : SolfacDistribution
    {
        public double peak_concentration;
        public string gaussian_gradient_name_ref;

        public SolfacGaussianGradient()
        {
            distribution_name = "Gaussian Gradient";
            peak_concentration = 100.0;
            gaussian_gradient_name_ref = "Default gaussian gradient name";
        }
    }

    public class GaussianGradient
    {
        public string gaussian_gradient_name;
        // Need to store a transform for the widget, but not clear
        // right now how that will be related to the sigmas in each
        // direction, or instead to a covariance matrix
        private double[][] _transform = {
                new double[]{1.0, 0.0, 0.0, 0.0},
                new double[]{0.0, 1.0, 0.0, 0.0},
                new double[]{0.0, 0.0, 1.0, 0.0},
                new double[]{0.0, 0.0, 0.0, 1.0} };
        public double[][] transform { get { return _transform; } set { _transform = value; } }
        private double[] _gauss_sigma = new double[3] { 1.0, 1.0, 1.0 };
        public double[] gauss_sigma { get { return _gauss_sigma; } set { _gauss_sigma = value; } }

        public GaussianGradient()
        {
            gaussian_gradient_name = "Default gaussian gradient name";
        }
    }


    // UTILITY CLASSES =======================
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
