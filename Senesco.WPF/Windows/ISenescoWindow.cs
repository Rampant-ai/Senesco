using System;
using System.Windows;

namespace Senesco.WPF.Windows
{
   interface ISenescoWindow
   {
      void RestoreWindowPosition();
      void SaveWindowPosition();

      void Window_SizeChanged(object sender, SizeChangedEventArgs e);
      void Window_LocationChanged(object sender, EventArgs e);
   }
}
