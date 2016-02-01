using System.Windows;

namespace Senesco.WPF.Windows.Main
{
   /// <summary>
   /// Interaction logic for App.xaml
   /// </summary>
   public partial class App : Application
   {
      private Presenter m_presenter = null;

      public App()
         : base()
      {
      }

      /// <summary>
      /// This event handler stores the command line (startup event) arguments
      /// in App.Properties for use later when the program is ready to handle
      /// processing them.
      /// </summary>
      private void Application_Startup(object sender, StartupEventArgs e)
      {
         // Create a new presenter with the startup parameters.
         m_presenter = new Presenter(e.Args);
      }
   }
}
