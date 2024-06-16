using System;
using System.Collections.Generic;
using SharedCore.Services.Charts.Filters;

namespace SharedCore.Services.Charts
{
	/// <summary>
	/// Creates specific filters for signal processing.
	/// </summary>
	public class SignalFilterService : ISignalFilterService
	{
		private readonly Dictionary<int, Dictionary<int, double[]>> iFilterCoefficientsA;
		private readonly Dictionary<int, Dictionary<int, double[]>> iFilterCoefficientsB;

		/// <summary>
		/// Static constructor.
		/// </summary>
		public SignalFilterService()
		{
			iFilterCoefficientsA = new Dictionary<int, Dictionary<int, double[]>>
			{
				{
					50, new Dictionary<int, double[]>
					{
						{250, new double[5]{   1.073670060215053d,  -1.328807521393868d,   2.558483518747286d,  -1.328807521393868d,   1.073670060215053d}},
						{500, new double[5]{   1.036182056040525d,  -3.354214867413880d,   4.786838202611984d,  -3.354214867413880d,   1.036182056040525d}},
						{1000, new double[5]{   1.017930372174805d,  -3.872743029875737d,   5.719349071603179d,  -3.872743029875737d,   1.017930372174805d}},
						{2000, new double[5]{   1.008925360695109d,  -3.986093943239008d,   5.954946948553112d,  -3.986093943239008d,   1.008925360695109d}},
					}
				},
				{
					60, new Dictionary<int, double[]>
					{
						{250, new double[5]{   1.073670060215052d,  -0.270006233124972d,   2.164315392589536d,  -0.270006233124972d,   1.073670060215052d}},
						{500, new double[5]{   1.036182056040523d,  -3.022331329225414d,   4.276244841912036d,  -3.022331329225414d,   1.036182056040523d}},
						{1000, new double[5]{   1.017930372174842d,  -3.786089831015608d,   5.556356006194980d,  -3.786089831015608d,   1.017930372174842d}},
						{2000, new double[5]{   1.008925360695258d,  -3.964296326806692d,   5.912005328601317d,  -3.964296326806692d,   1.008925360695258d}},
					}
				},
				{
					100, new Dictionary<int, double[]>
					{
						{250, new double[5]{   1.073670060215052d,   3.478863255515649d,   4.965358895246471d,   3.478863255515650d,   1.073670060215052d}},
						{500, new double[5]{   1.036182056040521d,  -1.281196073781875d,   2.468400544555680d,  -1.281196073781876d,   1.036182056040521d}},
						{1000, new double[5]{   1.017930372174851d,  -3.294351988903674d,   4.701257937383206d,  -3.294351988903674d,   1.017930372174851d}},
						{2000, new double[5]{   1.008925360694332d,  -3.838255918860897d,   5.668321081389367d,  -3.838255918860897d,   1.008925360694332d}},
					}
				},
				{
					120, new Dictionary<int, double[]>
					{
						{250, new double[5]{   1.073670060215053d,   4.266203804935427d,   6.385256306152934d,   4.266203804935427d,   1.073670060215053d}},
						{500, new double[5]{   1.036182056040522d,  -0.260331854092367d,   2.088715648439951d,  -0.260331854092367d,   1.036182056040522d}},
						{1000, new double[5]{   1.017930372174856d,  -2.968391596581379d,   4.199895955281941d,  -2.968391596581379d,   1.017930372174856d}},
						{2000, new double[5]{   1.008925360694218d,  -3.752374374216825d,   5.506789049440467d,  -3.752374374216825d,   1.008925360694218d}},
					}
				},
			};
			iFilterCoefficientsB = new Dictionary<int, Dictionary<int, double[]>>
			{
				{
					50, new Dictionary<int, double[]>
					{
						{250, new double[5]{   1.000000000000000d,  -1.281537813724422d,  2.553048857088362d, -1.376077229063318d,   1.152774782089033d}},
						{500, new double[5]{   1.000000000000000d,  -3.294592777339695d,   4.785528632689303d,  -3.413836957488042d,   1.073673682003708}},
						{1000, new double[5]{   1.000000000000000d,  -3.838328930508655d,   5.719027547512687d,  -3.907157129243187d,   1.036182268440471}},
						{2000, new double[5]{   1.000000000000000d,  -3.968383961410780d,   5.954867284898089d,  -4.003803925060641d,   1.017930385038646}},
					}
				},
				{
					60, new Dictionary<int, double[]>
					{
						{250, new double[5]{   1.000000000000000d,  -0.260401293731373d,   2.158880730930608d,  -0.279611172518570d,   1.152774782089030d}},
						{500, new double[5]{   1.000000000000000d,  -2.968608560151881d,   4.274935271989369d,  -3.076054098298943d,   1.073673682003711d}},
						{1000, new double[5]{   1.000000000000000d,  -3.752445752218581d,   5.556034482104289d,  -3.819733909812737d,   1.036182268440474d}},
						{2000, new double[5]{   1.000000000000000d,  -3.946683190510477d,   5.911925664945453d,  -3.981909463095174d,   1.017930385038645d}},
					}
				},
				{
					100, new Dictionary<int, double[]>
					{
						{250, new double[5]{   1.000000000000000d,   3.355109554198765d,   4.959924233587545d,   3.602616956832533d,   1.152774782089031d}},
						{500, new double[5]{   1.000000000000000d,  -1.258422461853850d,   2.467090974633017d,  -1.303969685709903d,   1.073673682003709d}},
						{1000, new double[5]{   1.000000000000000d,  -3.265077607458207d,   4.700936413292448d,  -3.323626370349149d,   1.036182268440470d}},
						{2000, new double[5]{   1.000000000000000d,  -3.821202772715481d,   5.668241417738932d,  -3.855309065005854d,   1.017930385038641d}},
					}
				},
				{
					120, new Dictionary<int, double[]>
					{
						{250, new double[5]{   1.000000000000000d,    4.114442015909693d,    6.379821644494007d,    4.417965593961158d,   1.152774782089033d}},
						{500, new double[5]{   1.000000000000000d,   -0.255704383919044d,    2.087406078517285d,   -0.264959324265688d,   1.073673682003709d}},
						{1000, new double[5]{   1.000000000000000d,   -2.942013775337451d,    4.199574431191168d,   -2.994769417825294d,   1.036182268440472d}},
						{2000, new double[5]{   1.000000000000000d,   -3.735702794742473d,    5.506709385790676d,   -3.769045953691593d,   1.017930385038645d}},
					}
				},
			};
		}


		/// <inheritdoc />
        IReadOnlyList<ISignalFilter> ISignalFilterService.GetFilters(SignalFilterType aFilterType, int aSamplingRate)
        {
            if (aFilterType == SignalFilterType.NoFilter)
            {
                return new[]
                {
                    new CopyFilter()
                };
            }

            return new[]
            {
                CreateNotchFilter(aFilterType, aSamplingRate, 1),
                CreateNotchFilter(aFilterType, aSamplingRate, 2)
            };
        }

		/// <summary>
		/// Creates a notch filter for given frequency and sample rate.
		/// </summary>
		/// <param name="aFilterType">The filter frequency.</param>
		/// <param name="aSamplingRate">The sampling rate.</param>
		/// <param name="aFrequencyMultiplier">The multiplier of base frequency for the filter, usually 1 or 2.</param>
		/// <returns>created filter.</returns>
		private NotchFilter CreateNotchFilter(SignalFilterType aFilterType, 
			int aSamplingRate, int aFrequencyMultiplier)
		{
			int frequencyKey;
			switch (aFilterType)
			{
				case SignalFilterType.NotchFilter50Hz:
					frequencyKey = 50 * aFrequencyMultiplier;
					break;
				case SignalFilterType.NotchFilter60Hz:
					frequencyKey = 60 * aFrequencyMultiplier;
					break;
				default:
					throw new ArgumentOutOfRangeException(
						nameof(aFilterType), aFilterType, null);
			}

			var filterParams = new NotchFilterParams(2,
				iFilterCoefficientsA[frequencyKey][aSamplingRate],
				iFilterCoefficientsB[frequencyKey][aSamplingRate]);

			return new NotchFilter(filterParams);
		}
	}
}