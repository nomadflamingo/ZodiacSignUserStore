using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZodiacSignUserStore.Models;
using ZodiacSignUserStore.ViewModels;

namespace ZodiacSignUserStore.Views
{
    /// <summary>
    /// Interaction logic for PeopleListControl.xaml
    /// </summary>
    public partial class PeopleListControl : UserControl
    {
        private object? _originalValue;
        private PeopleListViewModel _viewModel;
        public PeopleListControl()
        {
            InitializeComponent();
            DataContext = _viewModel = new PeopleListViewModel();
        }

        private void PeopleGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            // Save original value before editing
            if (e.Row.Item is Person person && e.Column is DataGridBoundColumn col)
            {
                var bindingPath = (col.Binding as System.Windows.Data.Binding)?.Path?.Path;
                if (!string.IsNullOrEmpty(bindingPath))
                {
                    var prop = typeof(Person).GetProperty(bindingPath);
                    _originalValue = prop?.GetValue(person);
                }
            }
        }

        private void PeopleGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit)
                return;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    // Force update the binding to trigger validation
                    var binding = (e.Column as DataGridBoundColumn)?.Binding as System.Windows.Data.Binding;
                    if (binding != null)
                    {
                        var expression = e.EditingElement.GetBindingExpression(TextBox.TextProperty);
                        expression?.UpdateSource();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Invalid input", MessageBoxButton.OK, MessageBoxImage.Warning);

                    // Revert to original value
                    if (e.Row.Item is Person person && e.Column is DataGridBoundColumn col)
                    {
                        var bindingPath = (col.Binding as System.Windows.Data.Binding)?.Path?.Path;
                        if (!string.IsNullOrEmpty(bindingPath))
                        {
                            var prop = typeof(Person).GetProperty(bindingPath);
                            if (prop != null && _originalValue != null)
                            {
                                prop.SetValue(person, _originalValue);
                            }
                        }
                    }

                    // Refresh the DataGrid to show reverted value
                    PeopleGrid.Items.Refresh();
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }
    }
}
