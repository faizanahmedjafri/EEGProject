using System;
using System.Collections.Generic;
using System.Linq;
using biopot.Models;
using Xamarin.Forms;

namespace biopot.Controls
{
    public partial class ImpedanceControl : ContentView
    {
        private readonly IReadOnlyDictionary<int, VisualElement> iSensorViews;

        public ImpedanceControl()
        {
            InitializeComponent();

            // FIXME set the relaction with channel id
            iSensorViews = new Dictionary<int, VisualElement>
            {
                {1, T3Circle}, {2, F7Circle}, {3, Fp1Circle}, {4, Fp2Circle},
                {5, F8Circle}, {6, T4Circle}, {7, T6Circle}, {8, Circle02},
                {9, Circle01}, {10, T5Circle}, {11, C3Circle}, {12, F3Circle},
                {13, FzCircle}, {14, F4Circle}, {15, C4Circle}, {16, P4Circle},
                {17, PzCircle}, {18, P3Circle}, {19, CzCircle},
                {20, A1Circle}, {21, A2Circle}
            };

            SetDefaultBindingContext();
        }

        public static readonly BindableProperty SensorConnectionListProperty = BindableProperty.Create
        (nameof(SensorConnectionList), typeof(IList<SensorConnectionModel>), typeof(ImpedanceControl),
            default(IList<SensorConnectionModel>), propertyChanged: OnSensorConnectionChanged);

        public IList<SensorConnectionModel> SensorConnectionList
        {
            get { return (IList<SensorConnectionModel>) GetValue(SensorConnectionListProperty); }
            set { SetValue(SensorConnectionListProperty, value); }
        }

        private static void OnSensorConnectionChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue == null)
            {
                return;
            }

            var _this = (ImpedanceControl) bindable;
            _this.SetChannelsBindingContext((IEnumerable<SensorConnectionModel>) newValue);
        }

        private void SetChannelsBindingContext(IEnumerable<SensorConnectionModel> sensorConnectionList)
        {
            var sensorIdToModels = sensorConnectionList
                .ToDictionary(aModel => aModel.SensorId);

            foreach (var channelPair in iSensorViews)
            {
                if (sensorIdToModels.TryGetValue(channelPair.Key, out var sensorModel))
                {
                    channelPair.Value.BindingContext = sensorModel;
                }
                else
                {
                    // clear context, when no sensor model exists for this sensor
                    channelPair.Value.BindingContext = new SensorConnectionModel();
                }
            }
        }

        /// <summary>
        /// Sets the default binding context.
        /// </summary>
        private void SetDefaultBindingContext()
        {
            foreach (var pair in iSensorViews)
            {
                pair.Value.BindingContext = new SensorConnectionModel();
            }
        }
    }
}
