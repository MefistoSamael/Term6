using Java.Lang;
using Java.Math;

namespace ScientificCalculator.Calculator.ScientificCalculator
{
    public class JavaMathCalculator : ICalculator
    {
        BigDecimal doublePi = new BigDecimal(ExtendedNumerics.BigDecimal.Pi.ToString()).Add(new BigDecimal(ExtendedNumerics.BigDecimal.Pi.ToString()));
        BigInteger ComparePower = new BigInteger("99999");
        private readonly Random random = new Random();
        int SCALE = 45;

        RoundOptions ROUNDING_MODE = Java.Math.RoundOptions.Down;
        public JavaMathCalculator() { }


        public JavaMathCalculator(int scale)
        {
            SCALE = scale;
        }
        public async Task<string> Plus(string first, string second)
        {
            var ft = new BigDecimal(first);
            var sd = new BigDecimal(second);

            return await Task.Run(() => ft.Add(sd)!.ToEngineeringString()!);
        }
        public async Task<string> Minus(string first, string second)
        {
            var ft = new BigDecimal(first);
            var sd = new BigDecimal(second);

            return await Task.Run(() => ft.Subtract(sd)!.ToEngineeringString()!);
        }
        public async Task<string> Mult(string first, string second)
        {
            var ft = new BigDecimal(first);
            var sd = new BigDecimal(second);

            return await Task.Run(() => ft.Multiply(sd)!.ToEngineeringString()!);
        }
        public async Task<string> Div(string first, string second)
        {
            var ft = new BigDecimal(first);
            var sd = new BigDecimal(second);

            return await Task.Run(() => ft.Divide(sd)!.ToEngineeringString()!);
        }
        public async Task<string> YsPower(string first, string second)
        {
            var ft = new BigDecimal(first);
            var sec = new BigInteger(second);
            BigDecimal ans = new BigDecimal(1);

            while (sec.CompareTo(ComparePower) >= 0)
            {
                ans = await Task.Run( () => ans.Multiply(ft.Pow(ComparePower.IntValue())));
                //ans = ans.Multiply(ft.Pow(ComparePower.IntValue()));
                sec = sec.Subtract(ComparePower);
            }

            if (sec.CompareTo(ComparePower) < 0 && sec.CompareTo(BigInteger.Zero) > 0)
            {
                //ans = await Task.Run(() => ans.Multiply(ft.Pow(sec.IntValueExact())));
                ans = ans.Multiply(ft.Pow(sec.IntValueExact()));
            }

            return ans.ToEngineeringString();
        }
        public async Task<string> NthRoot(string first, string second)
        {
            var a = new BigDecimal(first);
            var n = int.Parse(second);
            var p = BigDecimal.ValueOf(.1)!.MovePointLeft(SCALE);

            var answ = await Task.Run(() =>
            {
                if (a.CompareTo(BigDecimal.Zero) < 0)
                {
                    throw new IllegalArgumentException("nth root can only be calculated for positive numbers");
                }
                if (a.Equals(BigDecimal.Zero))
                {
                    return "0";
                }
                BigDecimal xPrev = a;
                BigDecimal x = a.Divide(new BigDecimal(n), SCALE, BigDecimal.RoundDown);  // starting "guessed" value...
                while (x.Subtract(xPrev)!.Abs()!.CompareTo(p) > 0)
                {
                    xPrev = x;
                    x = BigDecimal.ValueOf(n - 1.0).Multiply(x).Add(a.Divide(x.Pow(n - 1), SCALE, ROUNDING_MODE)).Divide(new BigDecimal(n), SCALE, ROUNDING_MODE);
                }
                return x.ToEngineeringString();
            });

            return answ;
        }


        public string Sign(string first)
        {
            if (first[0] != '-')
                return first.Insert(0, "-");
            else
                return first.Remove(0, 1);
        }
        public async Task<string> Percent(string first)
        {
            var a = new BigDecimal(first);

            return await Task.Run(() => a.Divide(new BigDecimal(100))!.ToEngineeringString()!);
        }
        public string Comma(string first)
        {
            int ind = first.IndexOf(".");
            if (ind != -1)
                return first.Append('.').ToString()!;
            else
            {
                return first;
            }
        }
        public async Task<string> TenPower(string first)
        {
            return await Task.Run(() => new BigDecimal(10).Pow(int.Parse(first))!.ToEngineeringString()!);
        }
        public async Task<string> Cube(string first)
        {
            return await Task.Run(() => new BigDecimal(first).Pow(3)!.ToEngineeringString()!);
        }
        public async Task<string> Square(string first)
        {
            return await Task.Run(() => new BigDecimal(first).Pow(2)!.ToEngineeringString()!);
        }
        public async Task<string> Invert(string first)
        {
            return await Task.Run(() => new BigDecimal(1).Divide(new BigDecimal(first)).ToEngineeringString());
        }
        public async Task<string> SquareRoot(string first)
        {
            return await Task.Run(() => new BigDecimal(first).Sqrt(new MathContext(SCALE)).ToEngineeringString()!);
        }
        public async Task<string> CubeRoot(string first)
        {
            return await NthRoot(first, "3");
        }
        public async Task<string> NaturalLogarithm(string first)
        {
            int scale = SCALE;
            var x = new BigDecimal(first);
            // Check that x > 0.
            if (x.Signum() <= 0)
            {
                throw new IllegalArgumentException("x <= 0");
            }

            // The number of digits to the left of the decimal point.
            int magnitude = x.ToString().Length - x.Scale() - 1;

            if (magnitude < 3)
            {
                return await Task.Run(() => lnNewton(x, scale)!.ToEngineeringString());
            }

            // Compute magnitude*ln(x^(1/magnitude)).
            else
            {
                // x^(1/magnitude)
                BigDecimal root = await Task.Run(() => IntRoot(x, magnitude, SCALE));

                // ln(x^(1/magnitude))
                BigDecimal lnRoot = await Task.Run(() => lnNewton(root, scale));

                // magnitude*ln(x^(1/magnitude))
                return await Task.Run(() => BigDecimal.ValueOf(magnitude).Multiply(lnRoot)
                            .SetScale(scale, BigDecimal.RoundHalfEven).ToEngineeringString());
            }
        }

        private BigDecimal lnNewton(BigDecimal x, int scale)
        {
            int sp1 = scale + 1;
            BigDecimal n = x;
            BigDecimal term;

            // Convergence tolerance = 5*(10^-(scale+1))
            BigDecimal tolerance = BigDecimal.ValueOf(5)
                                                .MovePointLeft(sp1);

            // Loop until the approximations converge
            // (two successive approximations are within the tolerance).
            do
            {

                // e^x
                BigDecimal eToX = Exp(x, sp1);

                // (e^x - n)/e^x
                term = eToX.Subtract(n)
                            .Divide(eToX, sp1, BigDecimal.RoundDown);

                // x - (e^x - n)/e^x
                x = x.Subtract(term);

                Java.Lang.Thread.Yield();
            } while (term.CompareTo(tolerance) > 0);

            return x.SetScale(SCALE, BigDecimal.RoundHalfEven);
        }

        private BigDecimal IntRoot(BigDecimal x, long index,
                                 int scale)
        {
            // Check that x >= 0.
            if (x.Signum() < 0)
            {
                throw new IllegalArgumentException("x < 0");
            }

            int sp1 = scale + 1;
            BigDecimal n = x;
            BigDecimal i = BigDecimal.ValueOf(index);
            BigDecimal im1 = BigDecimal.ValueOf(index - 1);
            BigDecimal tolerance = BigDecimal.ValueOf(5)
                                                .MovePointLeft(sp1);
            BigDecimal xPrev;

            // The initial approximation is x/index.
            x = x.Divide(i, scale, BigDecimal.RoundHalfEven);

            // Loop until the approximations converge
            // (two successive approximations are equal after rounding).
            do
            {
                // x^(index-1)
                BigDecimal xToIm1 = IntPower(x, index - 1, sp1);

                // x^index
                BigDecimal xToI =
                        x.Multiply(xToIm1)
                            .SetScale(sp1, BigDecimal.RoundHalfEven);

                // n + (index-1)*(x^index)
                BigDecimal numerator =
                        n.Add(im1.Multiply(xToI))
                            .SetScale(sp1, BigDecimal.RoundHalfEven);

                // (index*(x^(index-1))
                BigDecimal denominator =
                        i.Multiply(xToIm1)
                            .SetScale(sp1, BigDecimal.RoundHalfEven);

                // x = (n + (index-1)*(x^index)) / (index*(x^(index-1)))
                xPrev = x;
                x = numerator
                        .Divide(denominator, sp1, BigDecimal.RoundDown);

                Java.Lang.Thread.Yield();
            } while (x.Subtract(xPrev).Abs().CompareTo(tolerance) > 0);

            return x;
        }

        private BigDecimal IntPower(BigDecimal x, long exponent, int scale)
        {
            // If the exponent is negative, compute 1/(x^-exponent).
            if (exponent < 0)
            {
                return BigDecimal.ValueOf(1)
                        .Divide(IntPower(x, -exponent, scale), scale, BigDecimal.RoundHalfEven);
            }

            BigDecimal power = BigDecimal.ValueOf(1);

            // Loop to compute value^exponent.
            while (exponent > 0)
            {

                // Is the rightmost bit a 1?
                if ((exponent & 1) == 1)
                {
                    power = power.Multiply(x).SetScale(scale, BigDecimal.RoundHalfEven);
                }

                // Square x and shift exponent 1 bit to the right.
                x = x.Multiply(x).SetScale(scale, BigDecimal.RoundHalfEven);
                exponent >>= 1;

                Java.Lang.Thread.Yield();
            }

            return power;
        }

        private BigDecimal Exp(BigDecimal x, int scale)
        {
            // e^0 = 1
            if (x.Signum() == 0)
            {
                return BigDecimal.ValueOf(1);
            }

            // If x is negative, return 1/(e^-x).
            else if (x.Signum() == -1)
            {
                return BigDecimal.ValueOf(1)
                            .Divide(Exp(x.Negate(), scale), scale,
                                    BigDecimal.RoundHalfEven);
            }

            // Compute the whole part of x.
            BigDecimal xWhole = x.SetScale(0, BigDecimal.RoundDown);

            // If there isn't a whole part, compute and return await Task.Run( () =>  e^x.
            if (xWhole.Signum() == 0) return expTaylor(x, scale);

            // Compute the fraction part of x.
            BigDecimal xFraction = x.Subtract(xWhole);

            // z = 1 + fraction/whole
            BigDecimal z = BigDecimal.ValueOf(1)
                                .Add(xFraction.Divide(
                                        xWhole, scale,
                                        BigDecimal.RoundHalfEven));

            // t = e^z
            BigDecimal t = expTaylor(z, scale);

            BigDecimal maxLong = BigDecimal.ValueOf(Long.MaxValue);
            BigDecimal result = BigDecimal.ValueOf(1);

            // Compute and return await Task.Run( () =>  t^whole using IntPower().
            // If whole > Long.MaxValue, then first compute products
            // of e^Long.MaxValue.
            while (xWhole.CompareTo(maxLong) >= 0)
            {
                result = result.Multiply(
                                    IntPower(t, Long.MaxValue, scale))
                            .SetScale(scale, BigDecimal.RoundHalfEven);
                xWhole = xWhole.Subtract(maxLong);

                Java.Lang.Thread.Yield();
            }
            return result.Multiply(IntPower(t, xWhole.LongValue(), scale))
                            .SetScale(scale, BigDecimal.RoundHalfEven);
        }

        private BigDecimal expTaylor(BigDecimal x, int scale)
        {
            BigDecimal factorial = BigDecimal.ValueOf(1);
            BigDecimal xPower = x;
            BigDecimal sumPrev;

            // 1 + x
            BigDecimal sum = x.Add(BigDecimal.ValueOf(1));

            // Loop until the sums converge
            // (two successive sums are equal after rounding).
            int i = 2;
            do
            {
                // x^i
                xPower = xPower.Multiply(x).SetScale(scale, BigDecimal.RoundHalfEven);

                // i!
                factorial = factorial.Multiply(BigDecimal.ValueOf(i));

                // x^i/i!
                BigDecimal term = xPower.Divide(factorial, scale, BigDecimal.RoundHalfEven);

                // sum = sum + x^i/i!
                sumPrev = sum;
                sum = sum.Add(term);

                ++i;
                Java.Lang.Thread.Yield();
            } while (sum.CompareTo(sumPrev) != 0);

            return sum;
        }

        public async Task<string> DecimalLogarithm(string first)
        {
            var b = new BigDecimal(first);
            int NUM_OF_DIGITS = SCALE + 2;
            // need to Add one to get the right number of dp
            //  and then Add one again to get the next number
            //  so I can round it correctly.

            MathContext mc = new MathContext(NUM_OF_DIGITS, RoundingMode.HalfEven);
            // special conditions:
            // log(-x) -> exception
            // log(1) == 0 exactly;
            // log of a number lessthan one = -log(1/x)
            if (b.Signum() <= 0)
            {
                throw new System.ArithmeticException("log of a negative number! (or zero)");
            }
            else if (b.CompareTo(BigDecimal.One) == 0)
            {
                return await Task.Run(() => BigDecimal.Zero.ToEngineeringString());
            }
            else if (b.CompareTo(BigDecimal.One) < 0)
            {
                return await Task.Run(() => Sign(DecimalLogarithm((BigDecimal.One).Divide(b, mc).ToEngineeringString()).Result));
            }

            StringBuilder sb = new StringBuilder();
            // number of digits on the left of the decimal point
            int leftDigits = b.Precision() - b.Scale();

            // so, the first digits of the log10 are:
            sb.Append(leftDigits - 1).Append(".");

            // this is the algorithm outlined in the webpage
            int n = 0;
            while (n < NUM_OF_DIGITS)
            {
                await Task.Run(() => b = (b.MovePointLeft(leftDigits - 1)).Pow(10, mc));
                leftDigits = b.Precision() - b.Scale();
                sb.Append(leftDigits - 1);
                n++;
            }

            BigDecimal ans = new BigDecimal(sb.ToString());

            // Round the number to the correct number of decimal places.
            ans =
                    ans.Round(
                            new MathContext(
                                    ans.Precision() - ans.Scale() + SCALE, RoundingMode.HalfEven));
            return ans.ToEngineeringString();
        }
        public async Task<string> Sin(string first)
        {
            var x = new BigDecimal(first);
            x = await ToDoublePiPromezhutok(x);
            
            BigDecimal lastVal = x.Add(BigDecimal.One);
            BigDecimal currentValue = x;
            BigDecimal xSquared = await Task.Run(() => x.Multiply(x));
            BigDecimal numerator = x;
            BigDecimal denominator = BigDecimal.One;
            int i = 0;

            while (lastVal.CompareTo(currentValue) != 0)
            {
                lastVal = currentValue;

                int z = 2 * i + 3;

                denominator = denominator.Multiply(BigDecimal.ValueOf(z));
                denominator = denominator.Multiply(BigDecimal.ValueOf(z - 1));
                numerator = numerator.Multiply(xSquared);

                BigDecimal term = numerator.Divide(denominator, SCALE + 5, ROUNDING_MODE);

                if (i % 2 == 0)
                {
                    currentValue = currentValue.Subtract(term);
                }
                else
                {
                    currentValue = currentValue.Add(term);
                }

                i++;
            }
            return currentValue.ToEngineeringString();
        }

        private async Task<BigDecimal> ToDoublePiPromezhutok(BigDecimal x)
        {
            var SkolkoOtnyant = await Task.Run(() => x.Divide(doublePi, new MathContext(SCALE, RoundingMode.Down)).SetScale(0, RoundingMode.Down));

            return x.Subtract(doublePi.Multiply(SkolkoOtnyant));
        }

        public async Task<string> Cos(string first)
        {
            var x = new BigDecimal(first);

            x = await ToDoublePiPromezhutok(x);
            BigDecimal currentValue = BigDecimal.One;
            BigDecimal lastVal = currentValue.Add(BigDecimal.One);
            BigDecimal xSquared = await Task.Run(() => x.Multiply(x));
            BigDecimal numerator = BigDecimal.One;
            BigDecimal denominator = BigDecimal.One;
            int i = 0;

            while (lastVal.CompareTo(currentValue) != 0)
            {
                lastVal = currentValue;

                int z = 2 * i + 2;

                denominator = denominator.Multiply(BigDecimal.ValueOf(z));
                denominator = denominator.Multiply(BigDecimal.ValueOf(z - 1));
                numerator = numerator.Multiply(xSquared);

                BigDecimal term = numerator.Divide(denominator, SCALE + 5, ROUNDING_MODE);

                if (i % 2 == 0)
                {
                    currentValue = currentValue.Subtract(term);
                }
                else
                {
                    currentValue = currentValue.Add(term);
                }
                i++;
            }

            return currentValue.ToEngineeringString()!;
        }
        public async Task<string> Tan(string first)
        {
            BigDecimal sin = new BigDecimal(await Sin(first));
            BigDecimal cos = new BigDecimal(await Cos(first));

            return await Task.Run(() => sin.Divide(cos, SCALE, BigDecimal.RoundHalfUp)!.ToEngineeringString()!);
        }
        public string EulersNumber()
        {
            return ExtendedNumerics.BigDecimal.E.ToString();
        }
        public string Pi()
        {
            return ExtendedNumerics.BigDecimal.Pi.ToString();
        }

        public string Rand()
        {
            return random.Next().ToString();
        }
    }
}

