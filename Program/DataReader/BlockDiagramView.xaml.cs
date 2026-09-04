using System.Windows;
using System.Windows.Controls;

namespace DataHandler
{
    public partial class BlockDiagramView : UserControl
    {
        public BlockDiagramView()
        {
            InitializeComponent();
        }

        private void Block_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string componentName && DataContext is MainViewModel vm)
            {
                foreach (var block in vm.HardwareBlocks)
                {
                    if (block.Name.Equals(componentName, System.StringComparison.OrdinalIgnoreCase) || block.Name.StartsWith(componentName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        vm.SelectedBlock = block;
                        break;
                    }
                }
            }
        }
    }
}
