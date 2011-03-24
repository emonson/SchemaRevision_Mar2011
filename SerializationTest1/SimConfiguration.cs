using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace SerializationTest1
{
    public class SimConfigurator : ViewModelBase
    {
        public string FileName { get; set; }
        private XmlSerializer serializer = new XmlSerializer(typeof(SimConfiguration));
        private SimConfiguration _simConfig;
        public SimConfiguration SimConfig
        {
            get { return _simConfig; }
            set
            {
                if (value != _simConfig)
                {
                    if (_simConfig != null)
                    {
                        _simConfig.PropertyChanged -= this.On_simConfig_PropertyChanged;
                    }
                    _simConfig = value;
                    this._simConfig.PropertyChanged += this.On_simConfig_PropertyChanged;
                    // Notify of PropertyChanged if entire object is reassigned
                    base.OnPropertyChanged("SimConfig");
                }
            }
        }

        public SimConfigurator(string filename)
        {
            if (filename == null)
                throw new ArgumentNullException("filename");
            
            this.FileName = filename;
            this.SimConfig = new SimConfiguration();
        }

        // Also notify of property changed if sub-elements have changed.
        void On_simConfig_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged("SimConfig");
        }

        public void SerializeSimConfig()
        {
            TextWriter myWriter = new StreamWriter(FileName);
            serializer.Serialize(myWriter, SimConfig);
            myWriter.Close();
        }

        public void DeserializeSimConfig()
        {
            FileStream myFileStream = new FileStream(FileName, FileMode.Open);
            SimConfig = (SimConfiguration)serializer.Deserialize(myFileStream);
            myFileStream.Close();
        }
    }
    
    public class SimConfiguration : ViewModelBase
    {
        private string _experiment_name;
        public string experiment_name
        {
            get { return _experiment_name; }
            set
            {
                if (_experiment_name != value)
                {
                    _experiment_name = value;
                    base.OnPropertyChanged("experiment_name");
                }
            }
        }
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
        public ObservableCollection<Region> regions { get; set; }
        public ObservableCollection<Solfac> solfacs { get; set; }
        public ObservableCollection<CellSet> cellsets { get; set; }

        public Scenario()
        {
            description = "Scenario description";
            time_config = new TimeConfig();
            environment = new Environment();
            regions = new ObservableCollection<Region>();
            solfacs = new ObservableCollection<Solfac>();
            cellsets = new ObservableCollection<CellSet>();
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
        public ObservableCollection<SolfacType> solfac_types { get; set; }
        public ObservableCollection<CellType> cell_types { get; set; }
        public ObservableCollection<GaussianSpecification> gaussian_gradients { get; set; }

        public EntityRepository()
        {
            solfac_types = new ObservableCollection<SolfacType>();
            cell_types = new ObservableCollection<CellType>();
            gaussian_gradients = new ObservableCollection<GaussianSpecification>();
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
        public BoxSpecification region_box_spec { get; set; }
        public bool region_visibility { get; set; }
        public System.Windows.Media.Color region_color { get; set; }

        public Region()
        {
            region_name = "Default Region";
            region_type = Shape.Ellipsoid;
            region_box_spec = new BoxSpecification();
            region_visibility = true;
            region_color = new System.Windows.Media.Color();
            region_color = System.Windows.Media.Color.FromRgb(255, 255, 255);
        }

        public Region(string name, Shape type)
        {
            region_name = name;
            region_type = type;
            region_box_spec = new BoxSpecification();
            region_visibility = true;
            region_color = new System.Windows.Media.Color();
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
        public System.Windows.Media.Color cell_color { get; set; }

        public CellSet()
        {
            cell_name = "Default Cell";
            cell_type_ref = "MotileCell";
            number = 100;
            region_name_ref = "Default Region";
            wrt_region = RelativePosition.Inside;
            cell_color = new System.Windows.Media.Color();
            cell_color = System.Windows.Media.Color.FromRgb(255, 255, 255);
        }
    }

    public class CellType
    {
        public string cell_type_name { get; set; }
        public MotileCellParams cell_type_parameters { get; set; }

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
        public bool solfac_is_time_varying { get; set; }
        public List<TimeAmpPair> solfac_amplitude_keyframes { get; set; }
        public System.Windows.Media.Color solfac_color { get; set; }

        public Solfac()
        {
            solfac_name = "Default Solfac";
            solfac_type_ref = "ccr7";
            // Default is static homogeneous level
            solfac_distribution = new SolfacHomogeneousLevel();
            solfac_is_time_varying = false;
            solfac_amplitude_keyframes = new List<TimeAmpPair>();
            solfac_color = new System.Windows.Media.Color();
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
        public string distribution_name { get; set; }

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
        public double[] gradient_direction { get; set; }
        public double min_concentration { get; set; }
        public double max_concentration { get; set; }

        public SolfacLinearGradient()
        {
            distribution_name = "Linear Gradient";
            gradient_direction = new double[3] { 1.0, 0.0, 0.0 };
            min_concentration = 100.0;
            max_concentration = 250.0;
        }
    }

    public class SolfacGaussianGradient : SolfacDistribution
    {
        public double peak_concentration { get; set; }
        public string gaussian_spec_name_ref { get; set; }

        public SolfacGaussianGradient()
        {
            distribution_name = "Gaussian Gradient";
            peak_concentration = 100.0;
            gaussian_spec_name_ref = "Default gaussian gradient name";
        }
    }

    public class GaussianSpecification
    {
        public string gaussian_spec_name { get; set; }
        public BoxSpecification gaussian_box_spec { get; set; }
        public System.Windows.Media.Color gaussian_spec_color { get; set; }

        public GaussianSpecification()
        {
            gaussian_spec_name = "Default gaussian gradient name";
            gaussian_box_spec = new BoxSpecification();
            gaussian_spec_color = new System.Windows.Media.Color();
            gaussian_spec_color = System.Windows.Media.Color.FromRgb(255, 255, 255);
        }
    }


    // UTILITY CLASSES =======================
    public class BoxSpecification
    {
        public double[][] transform_matrix { get; set; }
        public bool box_visibility { get; set; }

        public BoxSpecification()
        {
            box_visibility = true;
            transform_matrix = new double[][] {
                new double[]{1.0, 0.0, 0.0, 0.0},
                new double[]{0.0, 1.0, 0.0, 0.0},
                new double[]{0.0, 0.0, 1.0, 0.0},
                new double[]{0.0, 0.0, 0.0, 1.0} };
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
