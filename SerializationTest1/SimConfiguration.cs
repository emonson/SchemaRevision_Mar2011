using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Data;
using System.Xml.Serialization;

namespace SerializationTest1
{
    public class SimConfigurator
    {
        public string FileName { get; set; }
        private XmlSerializer serializer = new XmlSerializer(typeof(SimConfiguration));
        public SimConfiguration SimConfig { get; set; }

        public SimConfigurator(string filename)
        {
            if (filename == null)
                throw new ArgumentNullException("filename");
            
            this.FileName = filename;
            this.SimConfig = new SimConfiguration();
        }

        public void SerializeSimConfigToFile()
        {
            TextWriter myWriter = new StreamWriter(FileName);
            serializer.Serialize(myWriter, SimConfig);
            myWriter.Close();
        }

        public string SerializeSimConfigToString()
        {
            StringWriter outStream = new StringWriter();
            serializer.Serialize(outStream, SimConfig);
            return outStream.ToString();
        }

        public void DeserializeSimConfig()
        {
            FileStream myFileStream = new FileStream(FileName, FileMode.Open);
            SimConfig = (SimConfiguration)serializer.Deserialize(myFileStream);
            myFileStream.Close();
        }
    }
    
    public class SimConfiguration
    {
        public string experiment_name { get; set; }
        public string description { get; set; }
        public Scenario scenario { get; set; }
        public ObservableCollection<GlobalParameters> global_parameters { get; set; }
        public EntityRepository entity_repository { get; set; }

        public SimConfiguration()
        {
            experiment_name = "Experiment1";
            description = "Whole sim config description";
            scenario = new Scenario();
            global_parameters = new ObservableCollection<GlobalParameters>();
            entity_repository = new EntityRepository();
        }

        public void LoadDefaultGlobalParameters()
        {
            var force_params = new ForceParams();
            global_parameters.Add(force_params);
            var junk_params = new JunkParams();
            global_parameters.Add(junk_params);
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

    public enum RegionShape { Rectangular, Ellipsoid }
    
    public class Region
    {
        public string region_name { get; set; }
        public RegionShape region_type { get; set; }
        public BoxSpecification region_box_spec { get; set; }
        public bool region_visibility { get; set; }
        public System.Windows.Media.Color region_color { get; set; }

        public Region()
        {
            region_name = "Default Region";
            region_type = RegionShape.Ellipsoid;
            region_box_spec = new BoxSpecification();
            region_visibility = true;
            region_color = new System.Windows.Media.Color();
            region_color = System.Windows.Media.Color.FromRgb(255, 255, 255);
        }

        public Region(string name, RegionShape type)
        {
            region_name = name;
            region_type = type;
            region_box_spec = new BoxSpecification();
            region_visibility = true;
            region_color = new System.Windows.Media.Color();
            region_color = System.Windows.Media.Color.FromRgb(255, 255, 255);
        }
    }

    public enum RelativePosition { Inside, Surface, Outside }
    
    public class CellSet
    {
        public string cell_name { get; set; }
        public string cell_type_ref { get; set; }
        public int number { get; set; }
        // TODO: Need to abstract out positioning to include pos specification for single cell...
        public bool cells_constrained_to_region { get; set; }
        public string region_name_ref { get; set; }
        public RelativePosition wrt_region { get; set; }
        public System.Windows.Media.Color cell_color { get; set; }

        public CellSet()
        {
            cell_name = "Default Cell";
            cell_type_ref = "MotileCell";
            number = 100;
            cells_constrained_to_region = false;
            region_name_ref = "Default Region";
            wrt_region = RelativePosition.Inside;
            cell_color = new System.Windows.Media.Color();
            cell_color = System.Windows.Media.Color.FromRgb(255, 255, 255);
        }
    }

    public class CellType
    {
        public string cell_type_name { get; set; }
        public ObservableCollection<MotileCellParams> cell_type_parameters { get; set; }

        public CellType()
        {
            cell_type_name = "Default cell type name";
            cell_type_parameters = new ObservableCollection<MotileCellParams>();
        }

        public void LoadDefaultMotileCellParams()
        {
            var loco = new LocomotorParams();
            var ckr = new CkReceptorParams();
            var ckri = new CkReceptorInitParams();

            cell_type_parameters.Add(loco);
            cell_type_parameters.Add(ckr);
            cell_type_parameters.Add(ckri);
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
        public string solfac_type_name { get; set; }

        public SolfacType()
        {
            solfac_type_name = "Default solfac type name";
        }
    }

    public enum SolfacDistributionType { Homogeneous, LinearGradient, GaussianGradient }

    // Base class for homog, linear, gauss distributions
    [XmlInclude(typeof(SolfacHomogeneousLevel)),
     XmlInclude(typeof(SolfacLinearGradient)),
     XmlInclude(typeof(SolfacGaussianGradient))]
    public abstract class SolfacDistribution : EntityModelBase
    {
        [XmlIgnore]
        public SolfacDistributionType solfac_distribution_type { get; protected set; }

        public SolfacDistribution()
        {
        }
    }

    public class SolfacHomogeneousLevel : SolfacDistribution
    {
        public double concentration { get; set; }

        public SolfacHomogeneousLevel()
        {
            solfac_distribution_type = SolfacDistributionType.Homogeneous;
            concentration = 100.0;
        }
    }

    public class SolfacLinearGradient : SolfacDistribution
    {
        public double[] gradient_direction { get; set; }
        public double min_concentration { get; set; }
        public double max_concentration { get; set; }
        public double x_direction
        {
            get { return gradient_direction[0]; }
            set
            {
                if (value != gradient_direction[0])
                {
                    gradient_direction[0] = value;
                }
            }
        }
        public double y_direction
        {
            get { return gradient_direction[1]; }
            set
            {
                if (value != gradient_direction[1])
                {
                    gradient_direction[1] = value;
                }
            }
        }
        public double z_direction
        {
            get { return gradient_direction[2]; }
            set
            {
                if (value != gradient_direction[2])
                {
                    gradient_direction[2] = value;
                }
            }
        }

        public SolfacLinearGradient()
        {
            solfac_distribution_type = SolfacDistributionType.LinearGradient;
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
            solfac_distribution_type = SolfacDistributionType.GaussianGradient;
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
    public class BoxSpecification : EntityModelBase
    {
        public double[][] transform_matrix { get; set; }
        public bool box_visibility { get; set; }
        public double x_scale
        {
            get { return transform_matrix[0][0]; }
            set
            {
                if (value != transform_matrix[0][0])
                {
                    transform_matrix[0][0] = value;
                    base.OnPropertyChanged("x_scale");
                }
            }
        }
        public double y_scale
        {
            get { return transform_matrix[1][1]; }
            set
            {
                if (value != transform_matrix[1][1])
                {
                    transform_matrix[1][1] = value;
                    base.OnPropertyChanged("y_scale");
                }
            }
        }
        public double z_scale
        {
            get { return transform_matrix[2][2]; }
            set
            {
                if (value != transform_matrix[2][2])
                {
                    transform_matrix[2][2] = value;
                    base.OnPropertyChanged("z_scale");
                }
            }
        }

        public double x_trans
        {
            get { return transform_matrix[0][3]; }
            set
            {
                if (value != transform_matrix[0][3])
                {
                    transform_matrix[0][3] = value;
                    base.OnPropertyChanged("x_trans");
                }
            }
        }
        public double y_trans
        {
            get { return transform_matrix[1][3]; }
            set
            {
                if (value != transform_matrix[1][3])
                {
                    transform_matrix[1][3] = value;
                    base.OnPropertyChanged("y_trans");
                }
            }
        }
        public double z_trans
        {
            get { return transform_matrix[2][3]; }
            set
            {
                if (value != transform_matrix[2][3])
                {
                    transform_matrix[2][3] = value;
                    base.OnPropertyChanged("z_trans");
                }
            }
        }

        public BoxSpecification()
        {
            box_visibility = true;
            transform_matrix = new double[][] {
                new double[]{1.0, 0.0, 0.0, 0.0},
                new double[]{0.0, 1.0, 0.0, 0.0},
                new double[]{0.0, 0.0, 1.0, 0.0},
                new double[]{0.0, 0.0, 0.0, 1.0} };
        }

        public void SetMatrixElement(int ii, int jj, double value)
        {
            if (value != transform_matrix[ii][jj])
            {
                transform_matrix[ii][jj] = value;
                base.OnPropertyChanged("transform_matrix");

                if (ii == 0 && jj == 0)
                    base.OnPropertyChanged("x_scale");
                if (ii == 1 && jj == 1)
                    base.OnPropertyChanged("y_scale");
                if (ii == 2 && jj == 2)
                    base.OnPropertyChanged("z_scale");

                if (ii == 0 && jj == 3)
                    base.OnPropertyChanged("x_trans");
                if (ii == 1 && jj == 3)
                    base.OnPropertyChanged("y_trans");
                if (ii == 2 && jj == 3)
                    base.OnPropertyChanged("z_trans");
            }
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

    public enum GlobalParameterType { ForceParams, JunkParams }

    /// <summary>
    /// Converter to go between enum values and "human readable" strings for GUI
    /// </summary>
    public class GlobalParamTypeToStringConverter : IValueConverter
    {
        // NOTE: This method is a bit fragile since the list of strings needs to 
        // correspond in length and index with the GlobalParameterType enum...
        private List<string> _global_param_type_strings = new List<string>()
	                            {
	                                "Force Parameters",
                                    "Junk Params"
	                            };

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return _global_param_type_strings[(int)value];
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string str = (string)value;
            int idx = _global_param_type_strings.FindIndex(item => item == str);
            return (GlobalParameterType)Enum.ToObject(typeof(GlobalParameterType), (int)idx);
        }
    }

    // Base class for any global parameter types
    [XmlInclude(typeof(ForceParams)),
     XmlInclude(typeof(JunkParams))]
    public class GlobalParameters
    {
        [XmlIgnore]
        public GlobalParameterType global_parameter_type { get; protected set; }

        public GlobalParameters()
        {
        }
    }

    public class ForceParams : GlobalParameters
    {
        public double force_delta { get; set; }
        public double force_phi1 { get; set; }
        public double force_phi2 { get; set; }

        public ForceParams()
        {
            global_parameter_type = GlobalParameterType.ForceParams;

            force_delta = 12.5;
            force_phi1 = 1.0;
            force_phi2 = 1.0;
        }
    }

    public class JunkParams : GlobalParameters
    {
        public double junk_1 { get; set; }
        public int junk_2 { get; set; }
        public string junk_3 { get; set; }

        public JunkParams()
        {
            global_parameter_type = GlobalParameterType.JunkParams;

            junk_1 = -12.5;
            junk_2 = 100;
            junk_3 = "sample string";
        }
    }

    public enum MotileCellParameterType { Locomotor, CkReceptor, CkReceptorInit }

    /// <summary>
    /// Converter to go between enum values and "human readable" strings for GUI
    /// </summary>
    public class MotileCellParamTypeToStringConverter : IValueConverter
    {
        // NOTE: This method is a bit fragile since the list of strings needs to 
        // correspond in length and index with the MotileCellParameterType enum...
        private List<string> _cell_param_type_strings = new List<string>()
	                            {
	                                "Locomotor params",
	                                "CK Receptor Params",
	                                "CK Receptor Init Params"
	                            };
        
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return _cell_param_type_strings[(int)value];
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string str = (string)value;
            int idx = _cell_param_type_strings.FindIndex(item => item == str);
            return (MotileCellParameterType)Enum.ToObject(typeof(MotileCellParameterType), (int)idx);
        }
    }
    
    // Base class for all cell parameters
    [XmlInclude(typeof(LocomotorParams)),
     XmlInclude(typeof(CkReceptorParams)),
     XmlInclude(typeof(CkReceptorInitParams))]
    public class MotileCellParams
    {
        [XmlIgnore]
        public MotileCellParameterType motile_cell_parameter_type { get; protected set; }

        public MotileCellParams()
        {
        }
    }

    public class LocomotorParams : MotileCellParams
    {
        public double loco_gamma { get; set; }
        public double loco_sigma { get; set; }
        public double loco_zeta { get; set; }
        public double loco_chi { get; set; }

        public LocomotorParams()
        {
            motile_cell_parameter_type = MotileCellParameterType.Locomotor;

            loco_gamma = 1.0;
            loco_sigma = 1.0;
            loco_zeta = 1.0;
            loco_chi = 1.0;
        }
    }

    public class CkReceptorParams : MotileCellParams
    {
        public double ckr_kappa { get; set; }
        public double ckr_pi { get; set; }
        public double ckr_tau { get; set; }
        public double ckr_delta { get; set; }

        public CkReceptorParams()
        {
            motile_cell_parameter_type = MotileCellParameterType.CkReceptor;

            ckr_kappa = 1.0;
            ckr_pi = 1.0;
            ckr_tau = 1.0;
            ckr_delta = 1.0;
        }
    }

    public class CkReceptorInitParams : MotileCellParams
    {
        public double ckri_u { get; set; }

        public CkReceptorInitParams()
        {
            motile_cell_parameter_type = MotileCellParameterType.CkReceptorInit;

            ckri_u = 1.0;
        }
    }

    /// <summary>
    /// Base class for all EntityModel classes.
    /// It provides support for property change notifications 
    /// and disposal.  This class is abstract.
    /// </summary>
    public abstract class EntityModelBase : INotifyPropertyChanged, IDisposable
    {
        #region Constructor

        protected EntityModelBase()
        {
        }

        #endregion // Constructor

        #region DisplayName

        /// <summary>
        /// Returns the user-friendly name of this object.
        /// Child classes can set this property to a new value,
        /// or override it to determine the value on-demand.
        /// </summary>
        // public virtual string DisplayName { get; protected set; }

        #endregion // DisplayName

        #region Debugging Aides

        /// <summary>
        /// Warns the developer if this object does not have
        /// a public property with the specified name. This 
        /// method does not exist in a Release build.
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,  
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                if (this.ThrowOnInvalidPropertyName)
                    throw new Exception(msg);
                else
                    Debug.Fail(msg);
            }
        }

        /// <summary>
        /// Returns whether an exception is thrown, or if a Debug.Fail() is used
        /// when an invalid property name is passed to the VerifyPropertyName method.
        /// The default value is false, but subclasses used by unit tests might 
        /// override this property's getter to return true.
        /// </summary>
        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        #endregion // Debugging Aides

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);

            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion // INotifyPropertyChanged Members

        #region IDisposable Members

        /// <summary>
        /// Invoked when this object is being removed from the application
        /// and will be subject to garbage collection.
        /// </summary>
        public void Dispose()
        {
            this.OnDispose();
        }

        /// <summary>
        /// Child classes can override this method to perform 
        /// clean-up logic, such as removing event handlers.
        /// </summary>
        protected virtual void OnDispose()
        {
        }

#if DEBUG
        /// <summary>
        /// Useful for ensuring that ViewModel objects are properly garbage collected.
        /// </summary>
        ~EntityModelBase()
        {
            // string msg = string.Format("{0} ({1}) ({2}) Finalized", this.GetType().Name, this.DisplayName, this.GetHashCode());
            string msg = string.Format("{0} ({1}) Finalized", this.GetType().Name, this.GetHashCode());
            System.Diagnostics.Debug.WriteLine(msg);
        }
#endif

        #endregion // IDisposable Members
    }
}
