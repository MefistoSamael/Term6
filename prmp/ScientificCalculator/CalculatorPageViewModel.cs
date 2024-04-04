using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExtendedNumerics;
using System.Globalization;
using System.Numerics;

namespace MauiScientificCalculator.ViewModels;

class CalculatorInternalState
{
    public string first;
    public BinaryOperations firstOp;
    public string second;
    public BinaryOperations secondOp;
    public string trailing;
    public CalculatorState state;
    public ToDisplay toDisplay;
    public bool constantNumberPressed;
}

[INotifyPropertyChanged]
#pragma warning disable MVVMTK0032 // Inherit from ObservableObject instead of using [INotifyPropertyChanged]
internal partial class CalculatorPageViewModel
#pragma warning restore MVVMTK0032 // Inherit from ObservableObject instead of using [INotifyPropertyChanged]
{
    Random rnd = new Random();
    string first = "0";
    BinaryOperations firstOp = BinaryOperations.Plus;
    string second = "0";
    BinaryOperations secondOp = BinaryOperations.Plus;
    string trailing = "0";
    [ObservableProperty]
    string display;
    CalculatorState state = CalculatorState.INITIAL;
    ToDisplay toDisplay = ToDisplay.FIRST;

    const int placesNumber = 45;

    bool constantNumberPressed = false;

    Stack<CalculatorInternalState> states = new Stack<CalculatorInternalState>();

    public CalculatorPageViewModel()
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        Display = first;
    }

    private void DisplayNumber()
    {
        switch (toDisplay)
        {
            case ToDisplay.FIRST:
                Display = first;
                break;
            case ToDisplay.SECOND:
                Display = second;
                break;
            case ToDisplay.TRAILING:
                Display = trailing;
                break;
        }
    }

    [RelayCommand]
    private void ReceiveInput(string input)
    {
        Display = "zhdi";

        if (isNumberInput(input) && !constantNumberPressed)
            HandleNumberInput(input);
        else if (isEqual(input))
            try
            {
                HandleEqual(input);
            }
            catch (InvalidDataException)
            {
                SetToErrorState();
            }
            catch (DivideByZeroException)
            {
                SetToErrorState();
            }
        else if (isReset(input))
            HandleReset(input);
        else if (isBinaryOperation(input) && state != CalculatorState.ERROR)
            try
            {
                HandleBinaryOperation(input);
            }
            catch (InvalidDataException)
            {
                SetToErrorState();
            }
            catch (DivideByZeroException)
            {
                SetToErrorState();
            }
        else if (isUnaryOperation(input))
            try
            {
                HandleUnaryOperation(input);
            }
            catch (InvalidDataException)
            {
                SetToErrorState();
            }
            catch (DivideByZeroException)
            {
                SetToErrorState();
            }
        else if (isBrace(input) && state != CalculatorState.ERROR)
            HandleBrace(input);
        else if (constantNumberPressed)
        { }
        else
            throw new InvalidOperationException("invalid operation");

        DisplayNumber();
    }

    private void SetToErrorState()
    {
        state = CalculatorState.ERROR;
        toDisplay = ToDisplay.FIRST;
        first = "Error";
    }

    private bool isNumberInput(string input)
    {
        return int.TryParse(input, out int res) || input == ".";
    }

    private bool isDecimalPointInput(string input)
    {
        return input == ".";
    }

    private void HandleNumberInput(string input)
    {
        switch (state)
        {
            case (CalculatorState.INITIAL):
                first = isDecimalPointInput(input) ? "0" + input : input;
                state = CalculatorState.TRANSITION_FROM_INITIAL;
                break;
            case (CalculatorState.TRANSITION):
                second = isDecimalPointInput(input) ? "0" + input : input;
                toDisplay = ToDisplay.SECOND;
                state = CalculatorState.TRANSITION_FROM_TRANSITION;
                break;
            case (CalculatorState.TRAILING):
                trailing = isDecimalPointInput(input) ? "0" + input : input;
                toDisplay = ToDisplay.TRAILING;
                state = CalculatorState.TRANSITION_FROM_TRAILING;
                break;
            case CalculatorState.EQUAL:
                first = isDecimalPointInput(input) ? "0" + input : input;
                state = CalculatorState.TRANSITION_FROM_INITIAL;
                break;
            case CalculatorState.TRANSITION_FROM_INITIAL:
                first = GetResultingDisplay(first, input);
                break;
            case CalculatorState.TRANSITION_FROM_TRANSITION:
                second = GetResultingDisplay(second, input);
                break;
            case CalculatorState.TRANSITION_FROM_TRAILING:
                trailing = GetResultingDisplay(trailing, input);
                break;
            case CalculatorState.ERROR:
                SetToBaseState(true);
                first = isDecimalPointInput(input) ? "0" + input : input;
                state = CalculatorState.TRANSITION_FROM_INITIAL;
                break;
            default:
                throw new Exception("Invalid State! BAD.");

        }

    }

    string GetResultingDisplay(string display, string input)
    {
        var resulting_display = "";
        if (isDecimalPointInput(input))
        {
            if (!display.Contains("."))
                resulting_display = display + input;
            else
                resulting_display = display;
        }
        else
            resulting_display = (display == "0" ? "" : display) + input;

        return resulting_display;
    }

    private bool isEqual(string input)
    {
        return input == "=";
    }

    private void HandleEqual(string input)
    {
        var resultFOp1S = GetOperationResult(first, firstOp, second);
        var resultSOp2T = GetOperationResult(second, secondOp, trailing);
        var resultFOp1SOp2T = GetOperationResult(first, firstOp, resultSOp2T);
        switch (state)
        {
            case CalculatorState.TRAILING:
                first = resultFOp1SOp2T;
                second = resultSOp2T;
                break;
            case CalculatorState.TRANSITION_FROM_TRAILING:
                first = resultFOp1SOp2T;
                second = resultSOp2T;
                break;
            case CalculatorState.ERROR:
                return;
            default:
                first = resultFOp1S;
                break;
        }
        toDisplay = ToDisplay.FIRST;
        state = CalculatorState.EQUAL;
    }

    private string GetOperationResult(string first, BinaryOperations op, string second)
    {
        var a = GetBigDecimal(first);
        var b = GetBigDecimal(second);

        switch (op)
        {
            case BinaryOperations.Plus:
                return BigDecimal.Round((a + b), placesNumber).ToString();
            case BinaryOperations.Minus:
                return BigDecimal.Round((a - b), placesNumber).ToString();
            case BinaryOperations.Mult:
                return BigDecimal.Round((a * b), placesNumber).ToString();
            case BinaryOperations.Div:
                return BigDecimal.Round((a / b), placesNumber).ToString();
            case BinaryOperations.YsPower:
                return Pow(a, second, b);
            case BinaryOperations.NthRoot:
                return NthRoot(a, (int)b);
            default:
                throw new Exception("Invalid operation for results! BAD");
        }
    }

    private BigDecimal GetBigDecimal(string number)
    {
        return BigDecimal.Parse(number);
    }

    private bool isReset(string input)
    {
        return input == "C";
    }

    private void HandleReset(string input)
    {
        switch (state)
        {
            case CalculatorState.INITIAL:
                SetToBaseState(true);
                return;
            case CalculatorState.TRANSITION_FROM_INITIAL:
                if (first != "0")
                {
                    first = "0";
                }
                else
                {
                    SetToBaseState(true);
                }
                break;
            case CalculatorState.TRANSITION:
                first = "0";
                state = CalculatorState.TRANSITION_FROM_INITIAL;
                break;
            case CalculatorState.TRANSITION_FROM_TRANSITION:
                if (second != "0")
                {
                    second = "0";
                }
                else
                {
                    SetToBaseState(true);
                }
                break;
            case CalculatorState.TRAILING:
                trailing = "0";
                state = CalculatorState.TRANSITION_FROM_TRAILING;
                break;
            case CalculatorState.TRANSITION_FROM_TRAILING:
                if (trailing != "0")
                {
                    trailing = "0";
                }
                else
                {
                    SetToBaseState(true);
                }
                break;
            case CalculatorState.EQUAL:
                first = "0";
                state = CalculatorState.TRANSITION_FROM_INITIAL;
                break;
            case CalculatorState.ERROR:
                SetToBaseState(true);
                break;
            default:
                throw new Exception("Invalid state! BAD.");
        }
    }

    private void SetToBaseState(bool clearStates)
    {
        first = "0";
        firstOp = BinaryOperations.Plus;
        second = "0";
        secondOp = BinaryOperations.Plus;
        trailing = "0";
        toDisplay = ToDisplay.FIRST;
        state = CalculatorState.INITIAL;
        constantNumberPressed = false;
        if (clearStates)
            states = new();
    }

    private bool isBinaryOperation(string input)
    {
        return binaryOperators.ContainsKey(input);
    }

    private void HandleBinaryOperation(string input)
    {
        constantNumberPressed = false;
        BinaryOperations input_operation = GetBinaryOperation(input);
        var resultFOp1S = GetOperationResult(first, firstOp, second);
        var resultSOp2T = GetOperationResult(second, secondOp, trailing);
        var resultFOp1SOp2T = GetOperationResult(first, firstOp, resultSOp2T);

        switch (state)
        {
            case CalculatorState.INITIAL:
                second = first;
                firstOp = input_operation;
                state = CalculatorState.TRANSITION;
                toDisplay = ToDisplay.SECOND;
                break;
            case CalculatorState.TRANSITION_FROM_INITIAL:
                second = first;
                firstOp = input_operation;
                state = CalculatorState.TRANSITION;
                toDisplay = ToDisplay.SECOND;
                break;
            case CalculatorState.TRANSITION:
                firstOp = input_operation;
                break;
            case CalculatorState.TRANSITION_FROM_TRANSITION:
                if (isComplexOperation(input_operation) && isSimpleOperation(firstOp))
                {
                    // complex operation case: move to TRAILING
                    secondOp = input_operation;
                    trailing = second;
                    toDisplay = ToDisplay.TRAILING;
                    state = CalculatorState.TRAILING;
                }
                else
                {
                    // simple operation case: move to TRANSITION
                    first = resultFOp1S;
                    firstOp = input_operation;
                    second = resultFOp1S;
                    toDisplay = ToDisplay.FIRST;
                    state = CalculatorState.TRANSITION;
                }
                break;
            case CalculatorState.TRAILING:
                if (isSimpleOperation(input_operation))
                {
                    // simple operation case: move back to TRANSITION
                    first = resultFOp1SOp2T;
                    firstOp = input_operation;
                    second = resultFOp1SOp2T;
                    toDisplay = ToDisplay.FIRST;
                    state = CalculatorState.TRANSITION;
                }
                else
                {
                    // complex operation case: stay in TRAILING
                    secondOp = input_operation;
                }
                break;
            case CalculatorState.TRANSITION_FROM_TRAILING:
                if (isSimpleOperation(input_operation))
                {
                    // simple operation case: move back to TRANSITION
                    first = resultFOp1SOp2T;
                    firstOp = input_operation;
                    second = resultFOp1SOp2T;
                    toDisplay = ToDisplay.FIRST;
                    state = CalculatorState.TRANSITION;
                }
                else
                {
                    // complex operation case: move back to TRAILING
                    second = resultSOp2T;
                    secondOp = input_operation;
                    trailing = resultSOp2T;
                    toDisplay = ToDisplay.SECOND;
                    state = CalculatorState.TRAILING;
                }
                break;
            case CalculatorState.EQUAL:
                firstOp = input_operation;
                second = first;
                state = CalculatorState.TRANSITION;
                break;
            case CalculatorState.ERROR:
                break;
            default:
                throw new Exception("Invalid state! BAD.");
        }
    }

    private bool isComplexOperation(BinaryOperations input_operation)
    {
        return input_operation == BinaryOperations.Mult ||
               input_operation == BinaryOperations.Div ||
               input_operation == BinaryOperations.YsPower ||
               input_operation == BinaryOperations.NthRoot;
    }

    private bool isSimpleOperation(BinaryOperations input_operation)
    {
        return input_operation == BinaryOperations.Plus ||
               input_operation == BinaryOperations.Minus;
    }

    private BinaryOperations GetBinaryOperation(string input)
    {
        return binaryOperators[input];
    }

    private bool isUnaryOperation(string input)
    {
        return unaryOperators.ContainsKey(input);
    }

    private void HandleUnaryOperation(string input)
    {
        switch (state)
        {
            case CalculatorState.INITIAL:
                first = PerfomUnaryOperation(first, input);
                break;
            case CalculatorState.TRANSITION_FROM_INITIAL:
                first = PerfomUnaryOperation(first, input);
                break;
            case CalculatorState.TRANSITION:
                second = PerfomUnaryOperation(second, input);
                break;
            case CalculatorState.TRANSITION_FROM_TRANSITION:
                second = PerfomUnaryOperation(second, input);
                break;
            case CalculatorState.TRAILING:
                trailing = PerfomUnaryOperation(trailing, input);
                break;
            case CalculatorState.TRANSITION_FROM_TRAILING:
                trailing = PerfomUnaryOperation(trailing, input);
                break;
            case CalculatorState.EQUAL:
                first = PerfomUnaryOperation(first, input);
                break;
            case CalculatorState.ERROR:
                if (input == "π" || input == "e" || input == "rand" || input == "+/-")
                {
                    SetToBaseState(true);
                    first = PerfomUnaryOperation(first, input);
                }
                break;
            default:
                throw new Exception("Invalid state! BAD.");
        }
    }

    private string PerfomUnaryOperation(string operand, string input)
    {
        var decimalOperand = GetBigDecimal(operand);

        var operation = unaryOperators[input];

        switch (operation)
        {
            case UnaryOperations.Sign:
                if (operand[0] != '-')
                    return operand.Insert(0, "-");
                else 
                    return operand.Remove(0,1);

            case UnaryOperations.Percent:
                return BigDecimal.Round((decimalOperand / 100), placesNumber).ToString();
            case UnaryOperations.Comma:

                if (!operand.Contains(','))
                    return operand.Insert(operand.Length, ".");
                else 
                    return operand;

            case UnaryOperations.TenPower:
                return Pow(10, operand, decimalOperand);
                
            case UnaryOperations.Cube:
                return Pow(decimalOperand, "3", 3);
                
            case UnaryOperations.Square:
                return Pow(decimalOperand, "2", 2);

            case UnaryOperations.Invert:
                if (decimalOperand.Sign == 0)
                    throw new InvalidDataException("Error: division by zero");
                return BigDecimal.Round((1 / decimalOperand), placesNumber).ToString();
                
            case UnaryOperations.SquareRoot:
                return NthRoot(decimalOperand, 2);
                
            case UnaryOperations.CubeRoot:
                // No 3throot for negative numbers in BigDecimal
                return NthRoot(decimalOperand, 3);
                
            case UnaryOperations.NaturalLogarithm:
                if (decimalOperand.IsNegative())
                    throw new InvalidDataException("Invalid logatithm arument");
                return Math.Round(Math.Log((double)decimalOperand), 15).ToString();

            case UnaryOperations.DecimalLogarithm:
                if (decimalOperand.IsNegative())
                    throw new InvalidDataException("Invalid logatithm arument");
                
                return Math.Round(Math.Log10((double)decimalOperand), 15).ToString();

            case UnaryOperations.Sin:
                if (decimalOperand.IsNegative())
                    return BigDecimal.Round(BigDecimal.Negate(BigDecimal.Sin(BigDecimal.Negate(decimalOperand), placesNumber)), placesNumber).ToString();
                return BigDecimal.Round(BigDecimal.Sin(decimalOperand, placesNumber), placesNumber).ToString();
            
            case UnaryOperations.Cos:
                return BigDecimal.Round(BigDecimal.Cos(decimalOperand, placesNumber), placesNumber).ToString();
            
            case UnaryOperations.Tan:
                
                if (BigDecimal.Abs(decimalOperand - BigDecimal.GetPiDigits(placesNumber) / 2) <= Math.Pow(0.1, placesNumber) ||
                    BigDecimal.Abs(decimalOperand - 3 * BigDecimal.GetPiDigits(placesNumber) / 2) <= Math.Pow(0.1, placesNumber))
                    throw new InvalidDataException("Invalid tan arument");
                return BigDecimal.Round(BigDecimal.Tan(decimalOperand, 10), placesNumber).ToString();

            case UnaryOperations.EulersNumber:
                constantNumberPressed = true;
                return BigDecimal.Round(BigDecimal.E, placesNumber).ToString();

            case UnaryOperations.Sinh:
                return BigDecimal.Round(BigDecimal.Sinh(decimalOperand), placesNumber).ToString();
                
            case UnaryOperations.Cosh:
                return BigDecimal.Round(BigDecimal.Cosh(decimalOperand), placesNumber).ToString();
                
            case UnaryOperations.Tanh:
                return BigDecimal.Round(BigDecimal.Tanh(decimalOperand), placesNumber).ToString();
                
            case UnaryOperations.Pi:
                constantNumberPressed = true;
                return BigDecimal.GetPiDigits(placesNumber).ToString();
                
            case UnaryOperations.Rand:
                constantNumberPressed = true;
                return rnd.Next().ToString();


            default:
                throw new InvalidOperationException("Invalid unary operation");
        }

    }

    private string NthRoot(BigDecimal decimalOperand, int root)
    {
        if (root % 2 != 0)
        {
            if (decimalOperand.IsNegative())
                return BigDecimal.Round(BigDecimal.Negate(BigDecimal.NthRoot(BigDecimal.Negate(decimalOperand), root, placesNumber)), placesNumber).ToString();
            else
                return BigDecimal.Round(BigDecimal.NthRoot(decimalOperand, root, placesNumber), placesNumber).ToString();
        }
        else
        {
            if (decimalOperand.IsNegative())
                throw new InvalidDataException("Negative numver in odd root");

            return BigDecimal.Round(BigDecimal.NthRoot(decimalOperand, root, placesNumber), placesNumber).ToString();
        }
        
    }

    private string GetFractionalPart(string number)
    {
        return number.Substring(number.IndexOf(".") + 1);
    }

    private string Pow(BigDecimal powBase, string exponent, BigDecimal decimalExponent)
    {
        if (exponent.IndexOf('.') is int index and not -1)
        {
            // x^5.25 will be handled like 100thRoot(x^525)
            int powerOfTen = GetFractionalPart(exponent).Length;
            var a = exponent.Remove(index, 1);
            var exp = BigInteger.Parse(a);
            var root = (int)Math.Pow(10, powerOfTen);
            BigDecimal basa = BigDecimal.Pow(powBase, exp);
            var nthroot = BigDecimal.NthRoot(basa, root, placesNumber);
            return BigDecimal.Round(nthroot, placesNumber).ToString();
        }
        else
        {
            return BigDecimal.Round(BigDecimal.Pow(powBase, (BigInteger)decimalExponent), placesNumber).ToString();
        }
    }

    private bool isBrace(string input)
    {
        return input == "(" || input == ")";
    }

    private void HandleBrace(string input)
    {
        if (input == "(") 
        {
            states.Push(new CalculatorInternalState
            {
                first = first,
                firstOp = firstOp,
                second = second,
                secondOp = secondOp,
                trailing = trailing,
                constantNumberPressed = constantNumberPressed,
                toDisplay = toDisplay,
                state = state,
            });
            SetToBaseState(false);
        }
        else
        {
            if (states.Count == 0)
                return;

            var st = states.Pop();
            if (st is null)
                return;

            var answer = EvaluateAnswer();

            SetState(st);

            switch (state)
            {
                case CalculatorState.INITIAL:
                    first = answer;
                    break;
                case CalculatorState.TRANSITION_FROM_INITIAL:
                    first = answer;
                    break;
                case CalculatorState.TRANSITION:
                    second = answer;
                    break;
                case CalculatorState.TRANSITION_FROM_TRANSITION:
                    second = answer;
                    break;
                case CalculatorState.TRAILING:
                    trailing = answer;
                    break;
                case CalculatorState.TRANSITION_FROM_TRAILING:
                    trailing = answer;
                    break;
                case CalculatorState.EQUAL:
                    first = answer;
                    break;
                case CalculatorState.ERROR:
                    break;
                default:
                    throw new Exception("Invalid state! BAD.");
            }
        }
    }

    private void SetState(CalculatorInternalState st)
    {
        first = st.first; 
        second = st.second;
        firstOp = st.firstOp;
        secondOp = st.secondOp;
        trailing = st.trailing;
        constantNumberPressed = st.constantNumberPressed;
        toDisplay = st.toDisplay;
        state = st.state;
    }


    private string EvaluateAnswer()
    {
        var resultFOp1S = GetOperationResult(first, firstOp, second);
        var resultSOp2T = GetOperationResult(second, secondOp, trailing);
        var resultFOp1SOp2T = GetOperationResult(first, firstOp, resultSOp2T);
        switch (state)
        {
            case CalculatorState.TRAILING:
                return resultFOp1SOp2T;
            case CalculatorState.TRANSITION_FROM_TRAILING:
                return resultFOp1SOp2T;
            case CalculatorState.ERROR:
                throw new Exception("Invalid state");
            default:
                return resultFOp1S;
        }
    }

    private readonly Dictionary<string, BinaryOperations> binaryOperators = new Dictionary<string, BinaryOperations> 
    {
        { "+", BinaryOperations.Plus },
        { "-", BinaryOperations.Minus },
        { "*", BinaryOperations.Mult },
        { "/", BinaryOperations.Div },
        { "x^Y", BinaryOperations.YsPower },
        { "∛x", BinaryOperations.NthRoot },
    };

    private readonly Dictionary<string, UnaryOperations> unaryOperators = new Dictionary<string, UnaryOperations>
    {
        { "+/-", UnaryOperations.Sign },
        { "%", UnaryOperations.Percent },
        { ".", UnaryOperations.Comma },
        { "10^", UnaryOperations.TenPower },
        { "x^3", UnaryOperations.Cube },
        { "x^2", UnaryOperations.Square },
        { "1/x", UnaryOperations.Invert },
        { "√", UnaryOperations.SquareRoot },
        { "∛", UnaryOperations.CubeRoot },
        { "ln", UnaryOperations.NaturalLogarithm },
        { "log", UnaryOperations.DecimalLogarithm },
        //{ "!", UnaryOperations.Factorial },
        { "sin", UnaryOperations.Sin },
        { "cos", UnaryOperations.Cos },
        { "tan", UnaryOperations.Tan },
        { "e", UnaryOperations.EulersNumber },
        { "sinh", UnaryOperations.Sinh },
        { "cosh", UnaryOperations.Cosh },
        { "tanh", UnaryOperations.Tanh },
        { "π", UnaryOperations.Pi },
        { "rand", UnaryOperations.Rand }
    };
}

enum CalculatorState
{
    INITIAL,
    TRANSITION_FROM_INITIAL,
    TRANSITION,
    TRANSITION_FROM_TRANSITION,
    TRAILING,
    TRANSITION_FROM_TRAILING,
    EQUAL,
    ERROR
}

enum ToDisplay
{
    FIRST,
    SECOND,
    TRAILING
}

enum BinaryOperations
{
    Plus,
    Minus,
    Mult,
    Div,
    YsPower,
    NthRoot
}

enum UnaryOperations
{
    Sign,
    Percent,
    Comma,
    TenPower,
    Cube,
    Square,
    Invert, // 1/x
    SquareRoot,
    CubeRoot,
    NaturalLogarithm,
    DecimalLogarithm,
    //Factorial,
    Sin,
    Cos,
    Tan,
    EulersNumber,
    Sinh,
    Cosh,
    Tanh,
    Pi,
    Rand
}