using Windows.UI.Xaml.Controls;
using Bermuda.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Bermuda.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Checkouts : Page
    {

        public CheckoutViewModel CheckoutView { get; } = (Application.Current as App).Container.GetService<CheckoutViewModel>();

        public Checkouts()
        {
            this.InitializeComponent();
        }

    }
}
