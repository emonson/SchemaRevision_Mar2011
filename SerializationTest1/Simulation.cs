using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SerializationTest1
{
    class Simulation
    {
        private SimConfigurator _configurator;
        private string _previous_config_string;

        public Simulation(SimConfigurator configurator)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            _configurator = configurator;
            _previous_config_string = _configurator.SerializeSimConfigToString();
        }

        public bool ScenarioChanged()
        {
            string current_sim_string = _configurator.SerializeSimConfigToString();
            bool changed_since_last_check = (current_sim_string != _previous_config_string);

            // Reset for next check
            _previous_config_string = current_sim_string;

            return changed_since_last_check;
        }
    }
}
