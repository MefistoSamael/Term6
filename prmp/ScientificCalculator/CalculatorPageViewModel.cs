using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExtendedNumerics;
using ScientificCalculator.Calculator.ScientificCalculator;
using System.ComponentModel;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

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

internal partial class CalculatorPageViewModel : ObservableObject
{
    //public event PropertyChangedEventHandler? PropertyChanged;
    private ICalculator calculator;

    Random rnd = new Random();

    string first = "0";
    string second = "0";
    string trailing = "0";

    BinaryOperations firstOp = BinaryOperations.Plus;
    BinaryOperations secondOp = BinaryOperations.Plus;

    [ObservableProperty]
    string display;
    ToDisplay toDisplay = ToDisplay.FIRST;

    CalculatorState state = CalculatorState.INITIAL;

    bool constantNumberPressed = false;

    const int placesNumber = 45;

    [ObservableProperty]
    string expressionString = "";


    Stack<CalculatorInternalState> states = new Stack<CalculatorInternalState>();

    public CalculatorPageViewModel()
    {
        calculator = new JavaMathCalculator();
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
    private async Task ReceiveInput(string input)
    {
        ExpressionString += input;
        Display = "zhdi";

        if (input == "cos")
        {
            var decimalOperand = GetBigDecimal(first);

            var result = await Task.Run(() => BigDecimal.Round(BigDecimal.Cos(decimalOperand, placesNumber), placesNumber).ToString());
            first = result;
            DisplayNumber();
            return;
        }

        if (isNumberInput(input) && !constantNumberPressed)
            HandleNumberInput(input);
        else if (isEqual(input))
            try
            {
                await HandleEqual(input);
            }
            catch (InvalidDataException)
            {
                SetToErrorState();
            }
            catch (DivideByZeroException)
            {
                SetToErrorState();
            }
            catch (OverflowException)
            {
                SetToErrorState();
            }
        else if (isReset(input))
            HandleReset(input);
        else if (isBinaryOperation(input) && state != CalculatorState.ERROR)
            try
            {
                await HandleBinaryOperation(input);
            }
            catch (InvalidDataException)
            {
                SetToErrorState();
            }
            catch (DivideByZeroException)
            {
                SetToErrorState();
            }
            catch (OverflowException)
            {
                SetToErrorState();
            }
        else if (isUnaryOperation(input))
            try
            {
                await HandleUnaryOperation(input);
            }
            catch (InvalidDataException)
            {
                SetToErrorState();
            }
            catch (DivideByZeroException)
            {
                SetToErrorState();
            }
            catch (OverflowException)
            {
                SetToErrorState();
            }
        else if (isBrace(input) && state != CalculatorState.ERROR)
            await HandleBrace(input);
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
            resulting_display = (display == "0" || display == "-0" ? "" : display) + input;

        return resulting_display;
    }

    private bool isEqual(string input)
    {
        return input == "=";
    }

    private async Task HandleEqual(string input)
    {
        var resultFOp1S = await GetOperationResult(first, firstOp, second); // 1 + 2
        var resultSOp2T = await GetOperationResult(second, secondOp, trailing); // 2 * 3
        var resultFOp1SOp2T = await GetOperationResult(first, firstOp, resultSOp2T); // 1 + 2 * 3

        switch (state)
        {
            case CalculatorState.TRAILING:
                first = resultFOp1SOp2T;
                firstOp = secondOp;
                second = trailing;
                break;
            case CalculatorState.TRANSITION_FROM_TRAILING:
                first = resultFOp1SOp2T;
                firstOp = secondOp;
                second = trailing;
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

    private async Task<string> GetOperationResult(string first, BinaryOperations op, string second)
    {
        switch (op)
        {
            case BinaryOperations.Plus:
                return await calculator.Plus(first, second);
            case BinaryOperations.Minus:
                return await calculator.Minus(first, second);
            case BinaryOperations.Mult:
                return await calculator.Mult(first, second);
            case BinaryOperations.Div:
                return await calculator.Div(first, second);
            case BinaryOperations.YsPower:
                return await calculator.YsPower(first, second);
            case BinaryOperations.NthRoot:
                return await calculator.NthRoot(first, second);
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

    private async Task HandleBinaryOperation(string input)
    {
        constantNumberPressed = false;
        BinaryOperations input_operation = GetBinaryOperation(input);

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
                    var resultFOp1S = await GetOperationResult(first, firstOp, second);
                    var resultSOp2T = await GetOperationResult(second, secondOp, trailing);
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
                    var resultSOp2T = await GetOperationResult(second, secondOp, trailing);
                    var resultFOp1SOp2T = await GetOperationResult(first, firstOp, resultSOp2T);
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
                    var resultSOp2T = await GetOperationResult(second, secondOp, trailing);
                    var resultFOp1SOp2T = await GetOperationResult(first, firstOp, resultSOp2T);
                    first = resultFOp1SOp2T;
                    firstOp = input_operation;
                    second = resultFOp1SOp2T;
                    toDisplay = ToDisplay.FIRST;
                    state = CalculatorState.TRANSITION;
                }
                else
                {
                    // complex operation case: move back to TRAILING
                    var resultSOp2T = await GetOperationResult(second, secondOp, trailing);
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

    private async Task HandleUnaryOperation(string input)
    {
        switch (state)
        {
            case CalculatorState.INITIAL:
                first = await PerfomUnaryOperation(first, input);
                break;
            case CalculatorState.TRANSITION_FROM_INITIAL:
                first = await PerfomUnaryOperation(first, input);
                break;
            case CalculatorState.TRANSITION:
                second = await PerfomUnaryOperation(second, input);
                break;
            case CalculatorState.TRANSITION_FROM_TRANSITION:
                second = await PerfomUnaryOperation(second, input);
                break;
            case CalculatorState.TRAILING:
                trailing = await PerfomUnaryOperation(trailing, input);
                break;
            case CalculatorState.TRANSITION_FROM_TRAILING:
                trailing = await PerfomUnaryOperation(trailing, input);
                break;
            case CalculatorState.EQUAL:
                first = await PerfomUnaryOperation(first, input);
                break;
            case CalculatorState.ERROR:
                if (input == "π" || input == "e" || input == "rand" || input == "+/-")
                {
                    SetToBaseState(true);
                    first = await PerfomUnaryOperation(first, input);
                }
                break;
            default:
                throw new Exception("Invalid state! BAD.");
        }
    }

    private async Task<string> PerfomUnaryOperation(string operand, string input)
    {

        var operation = unaryOperators[input];

        switch (operation)
        {
            case UnaryOperations.Sign:
                return calculator.Sign(operand);

            case UnaryOperations.Percent:
                return await calculator.Percent(operand);
            case UnaryOperations.Comma:
                return calculator.Comma(operand);

            case UnaryOperations.TenPower:
                return await calculator.TenPower(operand);

            case UnaryOperations.Cube:
                return await calculator.Cube(operand);

            case UnaryOperations.Square:
                return await calculator.Square(operand);

            case UnaryOperations.Invert:
                return await calculator.Invert(operand);

            case UnaryOperations.SquareRoot:
                return await calculator.SquareRoot(operand);


            case UnaryOperations.CubeRoot:
                return await calculator.CubeRoot(operand);


            case UnaryOperations.NaturalLogarithm:
                return await calculator.NaturalLogarithm(operand);

            case UnaryOperations.DecimalLogarithm:
                return await calculator.DecimalLogarithm(operand);

            case UnaryOperations.Sin:
                return await calculator.Sin(operand);


            case UnaryOperations.Cos:
                return await calculator.Cos(operand);


            case UnaryOperations.Tan:
                return await calculator.Tan(operand);

            case UnaryOperations.EulersNumber:
                constantNumberPressed = true;
                return calculator.EulersNumber();

            //case UnaryOperations.Sinh:
            //    return BigDecimal.Round(BigDecimal.Sinh(decimalOperand), placesNumber).ToString();

            //case UnaryOperations.Cosh:
            //    return BigDecimal.Round(BigDecimal.Cosh(decimalOperand), placesNumber).ToString();

            //case UnaryOperations.Tanh:
            //    return BigDecimal.Round(BigDecimal.Tanh(decimalOperand), placesNumber).ToString();

            case UnaryOperations.Pi:
                constantNumberPressed = true;
                return calculator.Pi();

            case UnaryOperations.Rand:
                constantNumberPressed = true;
                return calculator.Rand();


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

    private async Task HandleBrace(string input)
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

            var answer = await EvaluateAnswer();

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


    private async Task<string> EvaluateAnswer()
    {
        var resultFOp1S = await GetOperationResult(first, firstOp, second);
        var resultSOp2T = await GetOperationResult(second, secondOp, trailing);
        var resultFOp1SOp2T = await GetOperationResult(first, firstOp, resultSOp2T);
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