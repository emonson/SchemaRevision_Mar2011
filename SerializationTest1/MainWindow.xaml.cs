using System;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Runtime.InteropServices;
using System.Collections.Generic;
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
        public SimConfiguration SimConfig { get; set; }
        private XmlSerializer serializer = new XmlSerializer(typeof(SimConfiguration));
        private string filename = "test_native.xml";
        private RenderWindowControl rwc;
        private vtkTransform widgetTransform = vtkTransform.New();
        private vtkMatrix4x4 widgetMatrix = vtkMatrix4x4.New();
        private vtkActor sphereActor;
        private vtkBoxRepresentation boxRep;
        private vtkBoxWidget2 boxWidget;
        private double[][] _transform_matrix = {
                new double[]{2.0, 0.0, 0.0, 0.0},
                new double[]{0.0, 2.0, 0.0, 0.0},
                new double[]{0.0, 0.0, 2.0, 0.0},
                new double[]{0.0, 0.0, 0.0, 1.0} };
        public double[][] transform_matrix { get { return _transform_matrix; } set { _transform_matrix = value; } }

        public MainWindow()
        {
            InitializeComponent();

            // this.CreateAndSerializeScenario();

            this.DeserializeDocument();

            this.SetUpRenderWindow();

            // Should be able to do this in the xaml...
            CollectionViewSource ldv = this.Resources["regionsListView"] as CollectionViewSource;
            if (ldv != null)
            {
                ldv.Source = SimConfig.scenario.regions;
            }
            ldv = this.Resources["cellsListView"] as CollectionViewSource;
            if (ldv != null)
            {
                ldv.Source = SimConfig.scenario.cellsets;
            }

        }

        public void CreateAndSerializeScenario()
        {
            var sim_config = new SimConfiguration();
            sim_config.scenario.regions.Add(new Region("Sphere", Region.Shape.Ellipsoid));
            sim_config.scenario.regions.Add(new Region("Cube", Region.Shape.Rectangular));
            sim_config.scenario.cellsets.Add(new CellSet());
            sim_config.scenario.cellsets.Add(new CellSet());

            Solfac solfac = new Solfac();
            sim_config.scenario.solfacs.Add(solfac);

            solfac = new Solfac();
            solfac.is_time_varying = true;
            solfac.amplitude_keyframes.Add(new TimeAmpPair(0, 1));
            sim_config.scenario.solfacs.Add(solfac);

            this.SerializeDocument(sim_config);
        }

        private void SerializeDocument(SimConfiguration sim_config)
        {
            // Native XML serializer
            TextWriter myWriter = new StreamWriter(filename);
            serializer.Serialize(myWriter, sim_config);
            myWriter.Close();
        }

        private void DeserializeDocument()
        {
            // Writing the file requires a StreamWriter.
            FileStream myFileStream = new FileStream(filename, FileMode.Open);

            /* Deserializes the class and closes the TextWriter. */
            SimConfig = (SimConfiguration)serializer.Deserialize(myFileStream);
            
            myFileStream.Close();
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

            // add events to the iren instead of Observers
            // rwc.RenderWindow.GetInteractor().LeftButtonPressEvt += new vtkObject.vtkObjectEventHandler(leftMouseDown);
            
            // Set initial transform values
            this.TransferMatrixToVTKTransform(transform_matrix);

            // Demonstrate how to use the vtkBoxWidget 3D widget,
            vtkSphereSource sphere = vtkSphereSource.New();
            sphere.SetRadius(0.25);

            vtkPolyDataMapper sphereMapper = vtkPolyDataMapper.New();
            sphereMapper.SetInputConnection(sphere.GetOutputPort());

            sphereActor = vtkActor.New();
            sphereActor.SetMapper(sphereMapper);
            sphereActor.SetUserTransform(widgetTransform);
            sphereActor.GetProperty().SetOpacity(0.5);
            sphereActor.SetVisibility(0);

            vtkCubeSource cube = vtkCubeSource.New();
            cube.SetXLength(5.0);
            cube.SetYLength(5.0);
            cube.SetZLength(5.0);
            // cube.SetCenter(2, 0, 0);

            vtkOutlineSource outline = vtkOutlineSource.New();
            outline.SetBounds(-2, 2, -2, 2, -2, 2);

            vtkPolyDataMapper cubeMapper = vtkPolyDataMapper.New();
            cubeMapper.SetInputConnection(outline.GetOutputPort());

            vtkLODActor cubeActor = vtkLODActor.New();
            cubeActor.SetMapper(cubeMapper);
            cubeActor.VisibilityOn();

            ren.AddActor(sphereActor);
            ren.AddActor(cubeActor);

            boxRep = vtkBoxRepresentation.New();
            boxRep.SetTransform(widgetTransform);

            boxWidget = vtkBoxWidget2.New();
            boxWidget.SetInteractor( rwc.RenderWindow.GetInteractor() );
            boxWidget.SetRepresentation( boxRep );
            boxWidget.SetPriority(1);
            boxWidget.InteractionEvt += this.boxInteractionCallback;
            // boxWidget.On();

            ren.ResetCamera();
        }

        public void TransferMatrixToVTKTransform(double[][] matrix)
        {
            for (int ii = 0; ii < 4; ++ii)
            {
                for (int jj = 0; jj < 4; ++jj)
                {
                    widgetMatrix.SetElement(ii, jj, matrix[ii][jj]);
                }
            }
            widgetTransform.SetMatrix(widgetMatrix);
        }

        public void TransferVTKTransformToMatrix(double[][] matrix)
        {
            boxRep.GetTransform(widgetTransform);
            widgetTransform.GetMatrix(widgetMatrix);
            for (int ii = 0; ii < 4; ++ii)
            {
                for (int jj = 0; jj < 4; ++jj)
                {
                    matrix[ii][jj] = widgetMatrix.GetElement(ii, jj);
                }
            }
        }

        public void boxInteractionCallback(vtkObject sender, vtkObjectEventArgs e)
        {
            vtkBoxWidget2 wid = vtkBoxWidget2.SafeDownCast(sender);
            if (wid != null)
            {
                vtkBoxRepresentation rep = (vtkBoxRepresentation)wid.GetRepresentation();
                rep.GetTransform(widgetTransform);
                sphereActor.SetUserTransform(widgetTransform);
                int reg_idx = RegionsListBox.SelectedIndex;
                this.TransferVTKTransformToMatrix(this.SimConfig.scenario.regions[reg_idx].transform);
                // Console.WriteLine("X Scale, {0}, {1}", transform_matrix[0][0], widgetMatrix.GetElement(0,0));
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
            int reg_idx = RegionsListBox.SelectedIndex;
            this.TransferMatrixToVTKTransform(this.SimConfig.scenario.regions[reg_idx].transform);
            boxRep.SetTransform(this.widgetTransform);
            sphereActor.SetUserTransform(this.widgetTransform);
            boxWidget.On();
            sphereActor.SetVisibility(1);
            rwc.Invalidate();
        }

        private void RegionsListBox_GotFocus(object sender, RoutedEventArgs e)
        {
            int reg_idx = RegionsListBox.SelectedIndex;
            this.TransferMatrixToVTKTransform(this.SimConfig.scenario.regions[reg_idx].transform);
            boxRep.SetTransform(this.widgetTransform);
            sphereActor.SetUserTransform(this.widgetTransform);
            boxWidget.On();
            sphereActor.SetVisibility(1);
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
                sphereActor.SetVisibility(0);
                rwc.Invalidate();
            }
        }

        private void CellsListBox_GotFocus(object sender, RoutedEventArgs e)
        {
        }
    }
}
