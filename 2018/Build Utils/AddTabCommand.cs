using Autodesk.Revit.UI;
using System;
using Autodesk.Revit.UI.Events;


namespace Viper
{
    internal class AddTabCommand
    {
        private UIControlledApplication _application;

        void Command_DisplayingOptionDialog(object sender, DisplayingOptionsDialogEventArgs e)
        {
      
        }


        public AddTabCommand(UIControlledApplication application)
        {
            _application = application;
        }

        public bool AddTabToOptionsDialog()
        {
            _application.DisplayingOptionsDialog +=
               new EventHandler<DisplayingOptionsDialogEventArgs>(Command_DisplayingOptionDialog);
            return true;
        }
    }
}