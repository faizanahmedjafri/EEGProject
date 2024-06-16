using System;
using System.Windows.Input;
using biopot.Resources.Strings;
using biopot.Services;
using Prism.Events;
using Xamarin.Forms;
using ZXing;

namespace biopot.ViewModels
{
	public class PatientDetailsViewModel : BaseViewModel
	{
        private IPermissionsRequester iPermissionsRequester;
        private IDisposable iSubscriptionToken;

        public event EventHandler<bool> IsValidationPassed;

	    public PatientDetailsViewModel()
	    {
	        InitViews();
        }

        /// <summary>
        /// Initializes dependencies after being instantiated.
        /// </summary>
        public void InitDependencies(IPermissionsRequester aPermissionsRequester,
            IEventAggregator aEventAggregator)
        {
            iPermissionsRequester = aPermissionsRequester;
            iSubscriptionToken = aEventAggregator.GetEvent<SystemPermissionsChangedEvent>()
                .Subscribe(OnSystemPermissionsCouldChange, ThreadOption.UIThread);
        }

        #region -- Public properties --

        private string _id;
		public string Id
		{
		    get => _id;
		    set
		    {
		        SetProperty(ref _id, value);
		        Validate();
		    }
		}

	    private string _message;
	    public string Message
	    {
	        get => _message;
	        set => SetProperty(ref _message, value);
	    }

        private string _infoMessage;
		public string InfoMessage
		{
			get => _infoMessage;
		    set => SetProperty(ref _infoMessage, value);
		}

	    private bool _isBarcodeScanned;
	    public bool IsBarcodeScanned
	    {
	        get => _isBarcodeScanned;
	        set => SetProperty(ref _isBarcodeScanned, value);
	    }

        private bool _isAnalyzing;
	    public bool IsAnalyzing
	    {
	        get => _isAnalyzing;
	        set => SetProperty(ref _isAnalyzing, value);
	    }

        private bool _isBarcodeScanningAllowed;
        public bool IsBarcodeScanningAllowed
        {
            get => _isBarcodeScanningAllowed;
            set => SetProperty(ref _isBarcodeScanningAllowed, value);
        }

        private Color _infoMessageColor;
	    public Color InfoMessageColor
	    {
	        get => _infoMessageColor;
	        set => SetProperty(ref _infoMessageColor, value);
	    }

        private ICommand _barcodeScannedCommand;
		public ICommand BarcodeScannedCommand => _barcodeScannedCommand ?? (_barcodeScannedCommand = new Command<Result>(OnBarcodeScanned));

	    private ICommand _barcodeScannedFailureCommand;
	    public ICommand BarcodeScannedFailureCommand => _barcodeScannedFailureCommand ?? (_barcodeScannedFailureCommand = new Command(OnBarcodeScannedFailure));

	    private ICommand _scanAgainCommand;
	    public ICommand ScanAgainCommand => _scanAgainCommand ?? (_scanAgainCommand = new Command(OnScanAgainPressed));

        #endregion

	    /// <inheritdoc/>
	    public override void OnAppearing()
	    {
	        base.OnAppearing();

            IsAnalyzing = true;

            OnSystemPermissionsCouldChange();
        }

        /// <inheritdoc/>
        public override void OnDisappearing()
	    {
	        IsAnalyzing = false;

	        base.OnDisappearing();
	    }

        /// <inheritdoc />
        public override void Destroy()
        {
            base.Destroy();

            iSubscriptionToken?.Dispose();
        }

        /// <summary>
        /// Handles a case, when system permissions could have changed.
        /// </summary>
        private void OnSystemPermissionsCouldChange()
        {
            IsBarcodeScanningAllowed =
                iPermissionsRequester.IsPermissionGranted(PermissionType.Camera);
        }

        /// <summary>
        /// Handles 'scan again' event.
        /// </summary>
        private void OnScanAgainPressed()
        {
            Id = "";
            IsBarcodeScanned = false;
            InitViews();
	    }

        /// <summary>
        /// Handles barcode scanned event.
        /// </summary>
        /// <param name="aResult"></param>
        private void OnBarcodeScanned(Result aResult)
        {
            Message = Strings.PatientDetailsPatientCodeScanned;
            InfoMessageColor = Color.DarkGray;
            Id = aResult.Text;
            IsBarcodeScanned = Validate();
        }

        /// <summary>
        /// Handles barcode scanned failure event.
        /// </summary>
        // FIXME show the barcode scanning error if needed
        private void OnBarcodeScannedFailure()
	    {
            InfoMessage = Strings.PatientDetailsScanFailed;
	        InfoMessageColor = Color.Red;
	    }

	    /// <summary>
	    /// Handles barcode scanned failure event.
	    /// </summary>
	    /// FIXME show that barcode scanning in process if needed
	    private void OnBarcodeScanning()
	    {
	        InfoMessage = Strings.PatientDetailsScanningBarcode;
	        InfoMessageColor = Color.DarkGray;
	    }

        /// <summary>
        /// Initializes views.
        /// </summary>
	    private void InitViews()
        {
            InfoMessageColor = Color.DarkGray;
	        Message = Strings.PatientDetailsPatientCodeScanOrEnter;
	        InfoMessage = Strings.PatientDetailsScanHelpMessage;
        }

        /// <summary>
        /// Validates patient details.
        /// </summary>
	    private bool Validate()
        {
            var isValid = !string.IsNullOrEmpty(Id?.Trim());
            IsValidationPassed?.Invoke(this, isValid);

            return isValid;
        }
	}
}
