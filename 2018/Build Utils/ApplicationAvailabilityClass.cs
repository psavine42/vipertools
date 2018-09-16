using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

namespace Viper
{
    class ApplicationAvailabilityClass : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication appdata,  CategorySet selectedCategories)
        {
            Application app = appdata.Application;
            ApplicationOptions options = ApplicationOptions.Get();

            switch (options.Availability)
            {
                case ApplicationAvailablity.ArchitectureDiscipline:
                    return app.IsArchitectureEnabled;
                case ApplicationAvailablity.StructuralAnalysis:
                    return app.IsStructuralAnalysisEnabled;
                case ApplicationAvailablity.MEP:
                    return app.IsSystemsEnabled;
            }
            return true;
        }
    }
}
