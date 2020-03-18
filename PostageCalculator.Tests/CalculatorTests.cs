using System.Threading.Tasks;
using Xunit;

namespace PostageCalculator.Tests
{
    public class CalculatorTests
    {
        private const int BASE_COST = 5;
        private static readonly Calculator calculator = new Calculator();

        [Theory]
        [InlineData(3000, 1)]
        [InlineData(5000, 999)]
        public async Task BoxesUnder1KgAreAlwaysFiveBucks(int postcode, int weight)
        {
            var box = new Box { WeightInGrams = weight };

            Assert.Equal(BASE_COST, await calculator.CalculatePostageAsync(postcode, box));
        }

        [Theory]
        [InlineData(3000, 2000)]
        [InlineData(3121, 50000)]
        public async Task BoxesOver1KgAreFiveBucksPlus20cPerKilo(int postcode, int weight)
        {
            var box = new Box { WeightInGrams = weight };

            var expectedPostage = BASE_COST + WeightCost(weight);

            Assert.Equal(expectedPostage, await calculator.CalculatePostageAsync(postcode, box));
        }

        [Theory]
        [InlineData(5000, 2000)]
        public async Task PostcodesOutsideVictoriacostNineFiddyMore(int postcode, int weight)
        {
            var box = new Box { WeightInGrams = weight };

            var expectedPostage = BASE_COST + WeightCost(weight) + 9.5;

            Assert.Equal(expectedPostage, await calculator.CalculatePostageAsync(postcode, box));
        }

        private static double WeightCost(int weight)
        {
            return ((weight / 1000) * 0.2);
        }

        [Theory]
        [InlineData(3000, 70000)]
        public async Task BoxesHeavierThanFiddyLargeIncurA17PercentSurcharge(int postcode, int weight)
        {
            var box = new Box { WeightInGrams = weight };

            var expectedPostage = (BASE_COST + WeightCost(weight)) * 1.17;

            Assert.Equal(expectedPostage, await calculator.CalculatePostageAsync(postcode, box));
        }

        [Fact]
        public void BoxesWithAVolumeGreaterThan3CubicMetresCannotBeShipped()
        {
            int postcode = 0;
            var box = new Box
            {
                DepthInMM = 3000,
                WidthInMM = 2000,
                HeightInMM = 510
            };

            Assert.ThrowsAsync<ExcessiveVolumeException>(() => calculator.CalculatePostageAsync(postcode, box));
        }

        [Fact]
        public async Task BoxesWithAVolumeLessThanOrEqualTo3CubicMetresCanBeShipped()
        {
            int postcode = 3121;
            var box = new Box
            {
                DepthInMM = 3000,
                WidthInMM = 2000,
                HeightInMM = 500,
                WeightInGrams = 500
            };

            Assert.Equal(BASE_COST, await calculator.CalculatePostageAsync(postcode, box));
        }
    }
}