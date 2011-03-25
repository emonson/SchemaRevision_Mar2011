using System;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Kitware.VTK;

namespace SerializationTest1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SimConfigurator configurator;
        private string filename = "Config\\test_native.xml";
        private RenderWindowControl rwc;
        private List<vtkActor> sphereActorList = new List<vtkActor>();
        private vtkBoxRepresentation boxRep;
        private vtkBoxWidget2 boxWidget;
        private Simulation sim;

        public MainWindow()
        {
            InitializeComponent();

            // NOTE: Uncomment this to recreate initial XML scenario file
            this.CreateAndSerializeScenario();

            configurator = new SimConfigurator(filename);
            configurator.DeserializeSimConfig();

            // DEBUG: Bad hard-coded test...
            configurator.SimConfig.scenario.regions[0].region_box_spec.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(SimConfig_PropertyChanged);

            sim = new Simulation(configurator);

            this.SetUpRenderWindow();

            // Should be able to do this in the xaml...
            CollectionViewSource ldv = this.Resources["regionsListView"] as CollectionViewSource;
            if (ldv != null)
            {
                ldv.Source = configurator.SimConfig.scenario.regions;
            }
            ldv = this.Resources["cellsListView"] as CollectionViewSource;
            if (ldv != null)
            {
                ldv.Source = configurator.SimConfig.scenario.cellsets;
            }

        }

        public void CreateAndSerializeScenario()
        {
            var config = new SimConfigurator(filename);
            var sim_config = config.SimConfig;

            // Experiment
            sim_config.experiment_name = "Test experiment for XML serialization";
            sim_config.scenario.time_config.duration = 100;
            sim_config.scenario.time_config.timestep = 3;

            // Entity Repository
            EntityRepository repository = new EntityRepository();
            // Possible solfac types
            SolfacType st = new SolfacType();
            st.solfac_type_name = "ccr7";
            repository.solfac_types.Add(st);
            st = new SolfacType();
            st.solfac_type_name = "cxcl13";
            repository.solfac_types.Add(st);
            // Possible cell types
            CellType ct = new CellType();
            ct.cell_type_name = "bcell";
            repository.cell_types.Add(ct);
            ct = new CellType();
            ct.cell_type_name = "tcell";
            repository.cell_types.Add(ct);
            sim_config.entity_repository = repository;
            // Gaussian Gradients
            GaussianSpecification gg = new GaussianSpecification();
            gg.gaussian_spec_name = "Default on-center gradient";
            sim_config.entity_repository.gaussian_gradients.Add(gg);

            // Regions (not part of repository right now...)
            sim_config.scenario.regions.Add(new Region("Sphere", RegionShape.Ellipsoid));
            sim_config.scenario.regions.Add(new Region("Cube", RegionShape.Rectangular));

            // Cells
            CellSet cs = new CellSet();
            cs.cell_name = "Generic motile bcell";
            cs.cell_type_ref = sim_config.entity_repository.cell_types[0].cell_type_name;
            cs.number = 200;
            cs.region_name_ref = sim_config.scenario.regions[0].region_name;
            cs.wrt_region = CellSet.RelativePosition.Inside;
            sim_config.scenario.cellsets.Add(cs);
            cs = new CellSet();
            cs.cell_name = "Generic motile tcell";
            cs.cell_type_ref = sim_config.entity_repository.cell_types[1].cell_type_name;
            cs.number = 200;
            cs.region_name_ref = sim_config.scenario.regions[1].region_name;
            cs.wrt_region = CellSet.RelativePosition.Outside;
            sim_config.scenario.cellsets.Add(cs);

            // Solfacs
            Solfac solfac = new Solfac();
            solfac.solfac_type_ref = sim_config.entity_repository.solfac_types[0].solfac_type_name;
            SolfacGaussianGradient sgg = new SolfacGaussianGradient();
            sgg.gaussian_spec_name_ref = sim_config.entity_repository.gaussian_gradients[0].gaussian_spec_name;
            solfac.solfac_distribution = sgg;
            sim_config.scenario.solfacs.Add(solfac);
            solfac = new Solfac();
            solfac.solfac_type_ref = sim_config.entity_repository.solfac_types[1].solfac_type_name;
            solfac.solfac_is_time_varying = true;
            solfac.solfac_amplitude_keyframes.Add(new TimeAmpPair(0, 1));
            sim_config.scenario.solfacs.Add(solfac);

            // Write out XML file
            config.SerializeSimConfigToFile();
        }

        private void SetUpRenderWindow()
        {
            // create a VTK output control and make the forms host point to it
            rwc = new RenderWindowControl();
            rwc.CreateGraphics();
            windowsFormsHost.Child = rwc;

            // set up basic viewing
            vtkRenderer ren = rwc.RenderWindow.GetRenderers().GetFirstRenderer();

            // background color
            ren.SetBackground(0.0, 0.0, 0.0);

            // interactor style
            vtkInteractorStyleSwitch istyle = vtkInteractorStyleSwitch.New();
            rwc.RenderWindow.GetInteractor().SetInteractorStyle(istyle);
            rwc.RenderWindow.GetInteractor().SetPicker(vtkCellPicker.New());
            (istyle).SetCurrentStyleToTrackballCamera();

            // Demonstrate how to use the vtkBoxWidget 3D widget,
            vtkSphereSource sphere = vtkSphereSource.New();
            sphere.SetRadius(0.25);

            vtkPolyDataMapper sphereMapper = vtkPolyDataMapper.New();
            sphereMapper.SetInputConnection(sphere.GetOutputPort());

            vtkActor sphereActor;
            vtkTransform widgetTransform = vtkTransform.New();
            List<Region> region_list = configurator.SimConfig.scenario.regions.ToList();
            for (int ii = 0; ii < region_list.Count; ++ii)
            {
                this.TransferMatrixToVTKTransform(region_list[ii].region_box_spec.transform_matrix, widgetTransform);
                sphereActor = vtkActor.New();
                sphereActor.SetMapper(sphereMapper);
                sphereActor.SetUserTransform(widgetTransform);
                sphereActor.GetProperty().SetOpacity(0.5);
                sphereActor.SetVisibility(region_list[ii].region_visibility ? 1 : 0);
                sphereActorList.Add(sphereActor);
                ren.AddActor(sphereActorList[ii]);
            }

            vtkCubeSource cube = vtkCubeSource.New();
            cube.SetXLength(5.0);
            cube.SetYLength(5.0);
            cube.SetZLength(5.0);

            vtkOutlineSource outline = vtkOutlineSource.New();
            outline.SetBounds(-2, 2, -2, 2, -2, 2);

            vtkPolyDataMapper cubeMapper = vtkPolyDataMapper.New();
            cubeMapper.SetInputConnection(outline.GetOutputPort());

            vtkLODActor cubeActor = vtkLODActor.New();
            cubeActor.SetMapper(cubeMapper);
            cubeActor.VisibilityOn();

            ren.AddActor(cubeActor);

            boxRep = vtkBoxRepresentation.New();
            boxRep.SetTransform(widgetTransform);

            boxWidget = vtkBoxWidget2.New();
            boxWidget.SetInteractor( rwc.RenderWindow.GetInteractor() );
            boxWidget.SetRepresentation( boxRep );
            boxWidget.SetPriority(1);
            boxWidget.InteractionEvt += this.boxInteractionCallback;

            ren.ResetCamera();
        }

        // For reading C# matrices stored in scenario into VTK transforms
        public void TransferMatrixToVTKTransform(double[][] matrix, vtkTransform transform)
        {
            vtkMatrix4x4 vtk_matrix = vtkMatrix4x4.New();
            for (int ii = 0; ii < 4; ++ii)
            {
                for (int jj = 0; jj < 4; ++jj)
                {
                    vtk_matrix.SetElement(ii, jj, matrix[ii][jj]);
                }
            }
            transform.SetMatrix(vtk_matrix);
        }

        // For storing the VTK transform generated by a box widget in a C# matrix
        public void TransferVTKBoxWidgetTransformToMatrix(BoxSpecification box_spec)
        {
            vtkMatrix4x4 vtk_matrix = vtkMatrix4x4.New();
            vtkTransform vtk_transform = vtkTransform.New();
            boxRep.GetTransform(vtk_transform);
            vtk_transform.GetMatrix(vtk_matrix);
            for (int ii = 0; ii < 4; ++ii)
            {
                for (int jj = 0; jj < 4; ++jj)
                {
                    box_spec.SetMatrixElement(ii,jj,vtk_matrix.GetElement(ii, jj));
                }
            }
        }

        public void boxInteractionCallback(vtkObject sender, vtkObjectEventArgs e)
        {
            vtkBoxWidget2 wid = vtkBoxWidget2.SafeDownCast(sender);
            if (wid != null)
            {
                vtkTransform vtk_transform = vtkTransform.New();
                vtkBoxRepresentation rep = (vtkBoxRepresentation)wid.GetRepresentation();
                rep.GetTransform(vtk_transform);
                sphereActorList[RegionsListBox.SelectedIndex].SetUserTransform(vtk_transform);
                int reg_idx = RegionsListBox.SelectedIndex;
                this.TransferVTKBoxWidgetTransformToMatrix(this.configurator.SimConfig.scenario.regions[reg_idx].region_box_spec);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddCellButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RemoveCellButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RegionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vtkTransform vtk_transform = vtkTransform.New();
            int reg_idx = RegionsListBox.SelectedIndex;
            this.TransferMatrixToVTKTransform(this.configurator.SimConfig.scenario.regions[reg_idx].region_box_spec.transform_matrix, vtk_transform);
            boxRep.SetTransform(vtk_transform);
            sphereActorList[reg_idx].SetUserTransform(vtk_transform);
            boxWidget.On();
            // sphereActor.SetVisibility(1);
            rwc.Invalidate();
        }

        private void RegionsListBox_GotFocus(object sender, RoutedEventArgs e)
        {
            vtkTransform vtk_transform = vtkTransform.New();
            int reg_idx = RegionsListBox.SelectedIndex;
            this.TransferMatrixToVTKTransform(this.configurator.SimConfig.scenario.regions[reg_idx].region_box_spec.transform_matrix, vtk_transform);
            boxRep.SetTransform(vtk_transform);
            sphereActorList[reg_idx].SetUserTransform(vtk_transform);
            boxWidget.On();
            // sphereActor.SetVisibility(1);
            rwc.Invalidate();
        }

        private void RegionsListBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Don't want to get rid of widget if focus is lost to another region listboxitem
            // or to the VTK window
            IInputElement fe = FocusManager.GetFocusedElement(Application.Current.MainWindow);
            var lbi_focus = fe as ListBoxItem;
            var wfh_focus = fe as System.Windows.Forms.Integration.WindowsFormsHost;
            Region lbi_content = null;
            if (lbi_focus != null)
                lbi_content = lbi_focus.Content as Region;

            if (!(lbi_content != null || wfh_focus != null))
            {
                boxWidget.Off();
                // sphereActor.SetVisibility(0);
                rwc.Invalidate();
            }
        }

        private void SerializeButton_Click(object sender, RoutedEventArgs e)
        {
            // DEBUG property change notification
            // configurator.SimConfig.experiment_name = "NewExperiment";
            // configurator.SimConfig.description = "test debug description";

            configurator.SerializeSimConfigToFile();

            if (sim.ScenarioChanged())
            {
                System.Windows.MessageBox.Show("Scenario changed");
            }
        }

        void SimConfig_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.rwc.Invalidate();
        }
    }
}
