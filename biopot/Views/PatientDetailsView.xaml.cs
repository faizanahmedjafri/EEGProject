using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using biopot.ViewModels;
using Xamarin.Forms;
using ZXing.Mobile;
using ZXing.Net.Mobile.Forms;

namespace biopot.Views
{
	public partial class PatientDetailsView : ContentView
	{
	    //private readonly int ScannerViewSize = 200;
     //   private ZXingScannerView iScannerView;

        /// <summary>
        /// The view model, nullable.
        /// </summary>
        private PatientDetailsViewModel ViewModel { get; set; }

        public PatientDetailsView()
        {
            InitializeComponent();
        }

        /// <inheritdoc />
        //protected override async void OnBindingContextChanged()
        //{
        //    base.OnBindingContextChanged();

        //    if (ViewModel != null)
        //    {
        //        ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        //    }

        //    ViewModel = (PatientDetailsViewModel) BindingContext;

        //    if (ViewModel != null)
        //    {
        //        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        //    }

        //    await SetupScannerViewIfNeeded();
        //}

        /// <summary>
        /// Handles properties change event of view model.
        /// </summary>
        //private async void OnViewModelPropertyChanged(object aSender, PropertyChangedEventArgs aArgs)
        //{
        //    if (aArgs.PropertyName == nameof(PatientDetailsViewModel.IsBarcodeScanningAllowed))
        //    {
        //        await SetupScannerViewIfNeeded();
        //    }
        //}

        /// <summary>
        /// Setups scanner view, if allowed and needed.
        /// </summary>
        //private async Task SetupScannerViewIfNeeded()
        //{
        //    if (ViewModel != null && ViewModel.IsBarcodeScanningAllowed)
        //    {
        //        // Wait until UI is fully initialized
        //        // issue of ZXing.Net.Mobile v.2.4.1: https://github.com/Redth/ZXing.Net.Mobile/issues/717
        //        await Task.Delay(1000);
        //        //SetupScannerView();
        //    }
        //}

        /// <summary>
        /// Creates and configures the scanner view.
        /// </summary>
        //private void SetupScannerView()
        //{
        //    if (iScannerView != null)
        //    {
        //        return;
        //    }

        //    iScannerView = new ZXingScannerView
        //    {
        //        HorizontalOptions = LayoutOptions.Center,
        //        VerticalOptions = LayoutOptions.Center,
        //        IsScanning = true,
        //    };

        //    iScannerView.SetBinding(ZXingScannerView.IsAnalyzingProperty,
        //        new Binding(nameof(PatientDetailsViewModel.IsAnalyzing)));
        //    iScannerView.SetBinding(ZXingScannerView.ScanResultCommandProperty,
        //        new Binding(nameof(PatientDetailsViewModel.BarcodeScannedCommand)));

        //    iScannerView.Options = new MobileBarcodeScanningOptions
        //    {
        //        CameraResolutionSelector = resolutions =>
        //        {
        //            CameraResolution cameraResolution;
        //            var squareResolution =
        //                resolutions.FirstOrDefault(resolution => resolution.Width == resolution.Height);
        //            if (squareResolution != null)
        //            {
        //                cameraResolution = squareResolution;
        //                return cameraResolution;
        //            }

        //            cameraResolution = resolutions.FirstOrDefault();

        //            if (cameraResolution != null && cameraResolution.Width != cameraResolution.Height)
        //            {
        //                // camera resolution is always in landscape mode, 
        //                // so width and height should be swapped for portrait mode.
        //                // FIXME fix if both modes(portrait and lanscape) will be supported by the app
        //                var cameraWidth = cameraResolution.Height;
        //                var cameraHeigh = cameraResolution.Width;

        //                double scannerHeight;
        //                double scannerWidth;
        //                if (cameraWidth > cameraHeigh)
        //                {
        //                    scannerWidth = ScannerViewSize;
        //                    scannerHeight = scannerWidth * cameraHeigh / cameraWidth;
        //                }
        //                else
        //                {
        //                    scannerHeight = ScannerViewSize;
        //                    scannerWidth = scannerHeight * cameraWidth / cameraHeigh;
        //                }

        //                iScannerView.HeightRequest = scannerHeight;
        //                iScannerView.WidthRequest = scannerWidth;
        //            }

        //            return cameraResolution;
        //        }
        //    };

        //    CameraViewfinder.Children.Add(iScannerView);
        //}
    }
}