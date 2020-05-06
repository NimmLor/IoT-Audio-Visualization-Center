using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;


namespace Analyzer
{
    public class NewDeviceViewModel
    {
        private IDialogCoordinator dialogCoordinator;

        // Constructor
        public NewDeviceViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;
        }

        // Methods
        private async void FooMessageAsync()
        {
            await dialogCoordinator.ShowMessageAsync(this, "HEADER", "MESSAGE");
            //await dialogCoordinator.
        }

        private void FooProgress()
        {
            // Show...
            //ProgressDialogController controller = await dialogCoordinator.ShowProgressAsync(this, "HEADER", "MESSAGE");
            //controller.SetIndeterminate();

            // Do your work... 

            // Close...
            //await controller.CloseAsync();
        }
    }
}
