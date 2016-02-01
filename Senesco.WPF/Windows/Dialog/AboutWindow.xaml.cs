using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Senesco.Client.Utility;

namespace Senesco.WPF.Windows.Dialog
{
   /// <summary>
   /// Interaction logic for AboutWindow.xaml
   /// </summary>
   public partial class AboutWindow : Window, ISenescoWindow
   {
      public AboutWindow(Window owner)
      {
         WindowUtils.ConfigureChildWindow(owner, this);
         InitializeComponent();
         WindowUtils.RestoreWindowPosition(this, owner, WindowUtils.DefaultPosition.CenterOnParent);

         this.Title = String.Format("About {0}", AssemblyTitle);
         this.m_product.Content = AssemblyProduct;
         this.m_version.Content = String.Format("Version {0}", AssemblyVersion);
         this.m_copyright.Content = AssemblyCopyright;
         this.m_company.Content = AssemblyCompany;
         this.m_description.Content = AssemblyDescription;
      }

      #region Assembly Attribute Accessors

      public string AssemblyTitle
      {
         get
         {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            if (attributes.Length > 0)
            {
               AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
               if (titleAttribute.Title != "")
               {
                  return titleAttribute.Title;
               }
            }
            return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
         }
      }

      public string AssemblyVersion
      {
         get
         {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
         }
      }

      public string AssemblyDescription
      {
         get
         {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
            if (attributes.Length == 0)
            {
               return "";
            }
            return ((AssemblyDescriptionAttribute)attributes[0]).Description;
         }
      }

      public string AssemblyProduct
      {
         get
         {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            if (attributes.Length == 0)
            {
               return "";
            }
            return ((AssemblyProductAttribute)attributes[0]).Product;
         }
      }

      public string AssemblyCopyright
      {
         get
         {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            if (attributes.Length == 0)
            {
               return "";
            }
            return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
         }
      }

      public string AssemblyCompany
      {
         get
         {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
            if (attributes.Length == 0)
            {
               return "";
            }
            return ((AssemblyCompanyAttribute)attributes[0]).Company;
         }
      }

      #endregion

      public void Window_SizeChanged(object sender, SizeChangedEventArgs e)
      {
         WindowUtils.SaveWindowPosition(this);
      }

      public void Window_LocationChanged(object sender, EventArgs e)
      {
         WindowUtils.SaveWindowPosition(this);
      }

      public void SaveWindowPosition()
      {
         ConfigSettings.UserSettings.AboutWindowLeft = this.Left;
         ConfigSettings.UserSettings.AboutWindowTop = this.Top;
      }

      public void RestoreWindowPosition()
      {
         this.Left = ConfigSettings.UserSettings.AboutWindowLeft;
         this.Top = ConfigSettings.UserSettings.AboutWindowTop;
      }

      private void Window_KeyDown(object sender, KeyEventArgs e)
      {
         // Close the window immediately if Escape is pressed.
         if (Keyboard.IsKeyDown(Key.Escape))
            this.Close();
      }

      private void Button_Click(object sender, RoutedEventArgs e)
      {
         this.Close();
      }
   }
}
