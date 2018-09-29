using Map.Map;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Map.MapTest
{
    [TestClass()]
    public class PointCalculatorTest
    {
        private static readonly double delta = 0.00000000001;

        [TestMethod()]
        public void calculate_defaultwh_001()
        {
            PointCalculator calculator = new PointCalculator(100, 0, 0, 100);
            double[] point = calculator.calculate(100, 0);
            areEqualWithin(new double[] { 0, 0 }, point, delta);
        }

        [TestMethod()]
        public void calculate_defaultwh_002()
        {
            PointCalculator calculator = new PointCalculator(100, 0, 0, 100);
            double[] point = calculator.calculate(0, 100);
            areEqualWithin(new double[] { 1, 1 }, point, delta);
        }

        [TestMethod()]
        public void calculate_defaultwh_003()
        {
            PointCalculator calculator = new PointCalculator(100, 0, 0, 100);
            double[] point = calculator.calculate(90, 0);
            Console.WriteLine(point);
            areEqualWithin(new double[] { 0, 0.1 }, point, delta);
        }

        [TestMethod()]
        public void calculate_defaultwh_004()
        {
            PointCalculator calculator = new PointCalculator(100, 0, 0, 100);
            double[] point = calculator.calculate(100, 10);
            areEqualWithin(new double[] { 0.1, 0 }, point, delta);
        }

        [TestMethod()]
        public void calculate_defaultwh_011()
        {
            PointCalculator calculator = new PointCalculator(100, 50, 120, 170);
            double[] point = calculator.calculate(100, 120);
            areEqualWithin(new double[] { 0, 0 }, point, delta);
        }

        [TestMethod()]
        public void calculate_defaultwh_012()
        {
            PointCalculator calculator = new PointCalculator(100, 50, 120, 170);
            double[] point = calculator.calculate(50, 170);
            areEqualWithin(new double[] { 1, 1 }, point, delta);
        }

        [TestMethod()]
        public void calculate_defaultwh_013()
        {
            PointCalculator calculator = new PointCalculator(100, 50, 120, 170);
            double[] point = calculator.calculate(60, 120);
            Console.WriteLine(point);
            areEqualWithin(new double[] { 0, 0.8 }, point, delta);
        }

        [TestMethod()]
        public void calculate_defaultwh_014()
        {
            PointCalculator calculator = new PointCalculator(100, 50, 120, 170);
            double[] point = calculator.calculate(100, 160);
            areEqualWithin(new double[] { 0.8, 0 }, point, delta);
        }


        [TestMethod()]
        public void calculate_customwh_001()
        {
            PointCalculator calculator = new PointCalculator(100, 0, 0, 100, 1280, 720);
            double[] point = calculator.calculate(100, 0);
            areEqualWithin(new double[] { 0, 0 }, point, delta);
        }

        [TestMethod()]
        public void calculate_customwh_002()
        {
            PointCalculator calculator = new PointCalculator(100, 0, 0, 100, 1280, 720);
            double[] point = calculator.calculate(0, 100);
            areEqualWithin(new double[] { 1280, 720 }, point, delta);
        }

        [TestMethod()]
        public void calculate_customwh_003()
        {
            PointCalculator calculator = new PointCalculator(100, 0, 0, 100, 1280, 720);
            double[] point = calculator.calculate(90, 0);
            Console.WriteLine(point);
            areEqualWithin(new double[] { 0, 0.1*720 }, point, delta);
        }

        [TestMethod()]
        public void calculate_customwh_004()
        {
            PointCalculator calculator = new PointCalculator(100, 0, 0, 100, 1280, 720);
            double[] point = calculator.calculate(100, 10);
            areEqualWithin(new double[] { 0.1*1280, 0 }, point, delta);
        }

        [TestMethod()]
        public void calculate_customwh_011()
        {
            PointCalculator calculator = new PointCalculator(100, 50, 120, 170, 1920, 1080);
            double[] point = calculator.calculate(100, 120);
            areEqualWithin(new double[] { 0, 0 }, point, delta);
        }

        [TestMethod()]
        public void calculate_customwh_012()
        {
            PointCalculator calculator = new PointCalculator(100, 50, 120, 170, 1920, 1080);
            double[] point = calculator.calculate(50, 170);
            areEqualWithin(new double[] { 1*1920, 1*1080 }, point, delta);
        }

        [TestMethod()]
        public void calculate_customwh_013()
        {
            PointCalculator calculator = new PointCalculator(100, 50, 120, 170, 1920, 1080);
            double[] point = calculator.calculate(60, 120);
            Console.WriteLine(point);
            areEqualWithin(new double[] { 0, 0.8*1080 }, point, delta);
        }

        [TestMethod()]
        public void calculate_customwh_014()
        {
            PointCalculator calculator = new PointCalculator(100, 50, 120, 170, 1920, 1080);
            double[] point = calculator.calculate(100, 160);
            areEqualWithin(new double[] { 0.8*1920, 0 }, point, delta);
        }

        [TestMethod()]
        public void calculateInt_defaultwh_001()
        {
            PointCalculator calculator = new PointCalculator(100, 50, 120, 170);
            int[] point = calculator.calculateInt(76, 120);
            CollectionAssert.AreEqual(new int[] { 0, 0 }, point);
        }

        [TestMethod()]
        public void calculateInt_defaultwh_002()
        {
            PointCalculator calculator = new PointCalculator(100, 50, 120, 170);
            int[] point = calculator.calculateInt(75, 120);
            CollectionAssert.AreEqual(new int[] { 0, 1 }, point);
        }

        [TestMethod()]
        public void calculateInt_defaultwh_003()
        {
            PointCalculator calculator = new PointCalculator(100, 50, 120, 170);
            int[] point = calculator.calculateInt(100, 144);
            CollectionAssert.AreEqual(new int[] { 0, 0 }, point);
        }

        [TestMethod()]
        public void calculateInt_defaultwh_004()
        {
            PointCalculator calculator = new PointCalculator(100, 50, 120, 170);
            int[] point = calculator.calculateInt(100, 145);
            CollectionAssert.AreEqual(new int[] { 1, 0 }, point);
        }


        private void areEqualWithin(double[] expected, double[] actual, double delta)
        {
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i], delta);
            }
        }
    }
}