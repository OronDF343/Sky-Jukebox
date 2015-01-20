using System;
using System.Windows;

namespace SkyJukebox
{
    /// <summary>
    /// Interaction logic for ErrorMessage.xaml
    /// </summary>
    public partial class ErrorMessage : Window
    {
        public ErrorMessage()
        {
            InitializeComponent();
        }

        public ErrorMessage(Exception ex)
            : this()
        {
            ErrorTextBox.Text = string.Format(
@"Exception type: {0}
Message: {1}
Source: {2}
Stacktrace:
{3}

Inner exception: 
{4}

Inner.Inner exception:
{5}", ex.GetType().FullName, ex.Message, ex.Source, ex.StackTrace, ex.InnerException, ex.InnerException.InnerException);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(ErrorTextBox.Text);
        }
    }
}
