using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SerializationTest1
{
    class Simulation
    {
        private SimConfigurator Configurator;
        private bool _sim_is_dirty = false;

        public Simulation(SimConfigurator configurator)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            Configurator = configurator;
            Configurator.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(SimConfig_PropertyChanged);
        }

        void SimConfig_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _sim_is_dirty = true;
            System.Windows.MessageBox.Show(String.Format("The '{0}' property has changed!", e.PropertyName));
        }
    }
}
