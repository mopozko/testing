using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;

namespace HomeExercises
{
    public class NumberValidatorTests
    {
        [TestCase(3, 2, true, " ", ExpectedResult = false, TestName = "FalseWhenWhitespace")]
        [TestCase(3, 2, true, null, ExpectedResult = false, TestName = "FalseWhenNull")]
        [TestCase(3, 2, true, "a.sd", ExpectedResult = false, TestName = "FalseWhenNan")]
        [TestCase(3, 2, true, "-1,23", ExpectedResult = false, TestName = "NegativeWhenOnlyPositivIsTrue")]
        [TestCase(5, 2, false, "-1,23", ExpectedResult = true, TestName = "NegativeWhenOnlyPositivIsFalse")]
        [TestCase(3, 2, false, "11.23", ExpectedResult = false, TestName = "FalseWhenValueMorePrecision")]
        [TestCase(4, 2, false, "0.123", ExpectedResult = false, TestName = "FalseWhenFracPartMoreScale")]
        [TestCase(5, 2, false, "123,11", ExpectedResult = true, TestName = "CorrectSeparation1")]
        [TestCase(5, 2, false, "123.11", ExpectedResult = true, TestName = "CorrectSeparation2")]
        public static bool ValidateNumber(int precision, int scale, bool onlyPositiv, string value)
        {
            return new NumberValidator(precision, scale, onlyPositiv).IsValidNumber(value);
        }

        [TestCase(-1, 0, false, null, TestName = "ArgumentExceptionWhenPrecisionIsNegative")]
        [TestCase(0, 0, false, null, TestName = "ArgumentExceptionWhenPrecisionIsZero")]
        [TestCase(1, -1, false, null, TestName = "ArgumentExceptionWhenScaleIsNegative")]
        [TestCase(1, 2, false, null, TestName = "ArgumentExceptionWhenScaleGreaterThenPrecision")]
        public void CheckThrowing_AfterInitialization(int precision, int scale, bool onlyPositive, string value)
        {
            Assert.Throws<ArgumentException>(() => new NumberValidator(precision, scale, onlyPositive));
        }
    }

    public class NumberValidator
    {
        private readonly Regex numberRegex;
        private readonly bool onlyPositive;
        private readonly int precision;
        private readonly int scale;

        public NumberValidator(int precision, int scale = 0, bool onlyPositive = false)
        {
            this.precision = precision;
            this.scale = scale;
            this.onlyPositive = onlyPositive;
            if (precision <= 0)
                throw new ArgumentException("precision must be a positive number");
            if (scale < 0 || scale >= precision)
                throw new ArgumentException("precision must be a non-negative number less or equal than precision");
            numberRegex = new Regex(@"^([+-]?)(\d+)([.,](\d+))?$", RegexOptions.IgnoreCase);
        }

        public bool IsValidNumber(string value)
        {
            // Проверяем соответствие входного значения формату N(m,k), в соответствии с правилом, 
            // описанным в Формате описи документов, направляемых в налоговый орган в электронном виде по телекоммуникационным каналам связи:
            // Формат числового значения указывается в виде N(m.к), где m – максимальное количество знаков в числе, включая знак (для отрицательного числа), 
            // целую и дробную часть числа без разделяющей десятичной точки, k – максимальное число знаков дробной части числа. 
            // Если число знаков дробной части числа равно 0 (т.е. число целое), то формат числового значения имеет вид N(m).

            if (string.IsNullOrEmpty(value))
                return false;

            var match = numberRegex.Match(value);
            if (!match.Success)
                return false;

            // Знак и целая часть
            var intPart = match.Groups[1].Value.Length + match.Groups[2].Value.Length;
            // Дробная часть
            var fracPart = match.Groups[4].Value.Length;

            if (intPart + fracPart > precision || fracPart > scale)
                return false;

            if (onlyPositive && match.Groups[1].Value == "-")
                return false;
            return true;
        }
    }
}