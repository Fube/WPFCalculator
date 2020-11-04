using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace WPFCalculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Set as nullable because it defaults to 0 if it ISN'T nullable.
        private static double?
                _lastNumber,
                _result;
        // Set as nullable because it defaults to the first value in the enum if it ISN'T nullable
        private static SelectedOperator? _selectedOperator;

        private enum SelectedOperator
        {
            Addition,
            Subtraction,
            Multiplication,
            Division
        }

        public MainWindow()
        {
            InitializeComponent();

            // You know what would make this easier? 
            // CSS selectors.
            // WPF sucks. Change my mind
            foreach (var elem in MyGrid.Children) // We loop over all of the grid's children
            {

                // You might be wondering: Why not just name every button and assign a Click one by one?
                // If you have a thousand buttons, will you go through them and name them one by one?
                // Hopefully not. My laziness level caps at: if I have to do thing more than twice, I'd rather loop.
                // There are more than 2 buttons, so here we are.

                if(elem is Button button && !Regex.IsMatch(button.Content.ToString(), @"\d")) // If the child is a Button, we enter this if and refer to the child CAST TO A BUTTON as button
                {
                    button.Click += AssignClick( button.Content.ToString() ); // We attach the appropriate event handler
                }
            }
        }

        private RoutedEventHandler AssignClick(string content)
        {
            // Simple-ish RegEx to check if the content is a number, operation, special operation.
            // If it is none, we simply log (with Trace.WriteLine) that the button is unknown
            //const string isNumber = @"^\d+$";
            const string isOperation = @"^(\+|\-|\*|\/)$";
            const string isSpecialOperation = @"^(\+\/\-|\%|AC|=|\.)$";

            // Default event handler
            RoutedEventHandler reh = (e, s) => Trace.WriteLine("Unknown");

            // Removed to match the requirements
            // if (Regex.IsMatch(content, isNumber))
            // {
            //     reh = (e, s) => HandleNumber(content);
            // }
            if(Regex.IsMatch(content, isOperation))
            {
                reh = (e, s) => HandleOperation(content);
            }
            else if(Regex.IsMatch(content, isSpecialOperation))
            {
                reh = AssignSpecialOperationHandler(content);
            }

            return reh;
        }

        private void HandleSecondNumber()
        {

            if (_result == null) return;
            _result = null;
            Result.Content = "0";
        }

        private void HandleOperation(string operation)
        {
            _result = null;

            // Based on the operation string, call the appropriate mathematical operator
            switch (operation)
            {
                case "+": _selectedOperator = SelectedOperator.Addition;         break;
                case "-": _selectedOperator = SelectedOperator.Subtraction;      break;
                case "*": _selectedOperator = SelectedOperator.Multiplication;   break;
                case "/": _selectedOperator = SelectedOperator.Division;         break;
            }
        }

        private void HandleNumber(object sender, RoutedEventArgs eventArgs)
        {

            string number = ((Button) eventArgs.Source).Content.ToString();

            HandleSecondNumber();

            // When there is a _selectedOperator AND there is NO last number, it means we have just entered the first number and selected the operation
            // As such, we need to save the last number in _lastNumber and set Result.Content to the new number
            if (_selectedOperator != null && _lastNumber == null)
            {
                _lastNumber = double.Parse(Result.Content.ToString());
                Result.Content = number;
            }
            else // Normal flow
            {
                Result.Content = Result.Content.ToString().Equals("0") ? number : Result.Content + number; // Checks to see if it is the first ever entry, if yes: override otherwise: append
            }
        }

        private void HandleEqualsClick(object sender, RoutedEventArgs eventArgs)
        {
            try
            {

                _result = DoOperation(_selectedOperator, double.Parse(_lastNumber.ToString()), double.Parse(Result.Content.ToString()));
                Result.Content = _result;
            }
            catch (Exception ex) when (ex is ArithmeticException || ex is DivideByZeroException) // Division by zero is legal apparently. It just returns Infinity...
            {
                MessageBox.Show("That is mathematically impossible...", "Logic error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.StackTrace);
            }

            Reset();
        }

        private void HandleACClick(object sender, RoutedEventArgs eventArgs)
        {
            Reset();
            Result.Content = "0";
        }

        private void HandlePercentageClick(object sender, RoutedEventArgs eventArgs)
        {
            _result = null;
            // Prior to the change, this was needed to pass test 4
            // Test changed and so does the program...
            //_selectedOperator = SelectedOperator.Addition;
            Result.Content = double.Parse(Result.Content.ToString()) * ((_lastNumber ?? 1) / 100);
        }

        private void HandlePeriodClick(object sender, RoutedEventArgs eventArgs)
        {
            HandleSecondNumber();
            if (!Result.Content.ToString().Contains("."))// This is to prevent having 1.23.45
                Result.Content += ".";
        }

        private void HandlePlusMinusClick(object sender, RoutedEventArgs eventArgs)
        {
            Result.Content = double.Parse(Result.Content.ToString()) * -1;
        }

        private RoutedEventHandler AssignSpecialOperationHandler(string operation)
        {

            switch (operation)
            {
                case "=":   return HandleEqualsClick;
                case "AC":  return HandleACClick;
                case "+/-": return HandlePlusMinusClick;
                case "%":   return HandlePercentageClick;
                case ".":   return HandlePeriodClick;
                default:    return (e, s) => Trace.WriteLine("Unknown");
            }
        }

        private double? DoOperation(SelectedOperator? so, double o, double o1)
        {
            switch (so)
            {
                case SelectedOperator.Addition:         return MathService.Add(o, o1);
                case SelectedOperator.Subtraction:      return MathService.Subtract(o, o1);
                case SelectedOperator.Multiplication:   return MathService.Multiply(o, o1);
                case SelectedOperator.Division:         return MathService.Divide(o, o1);
                default:                                return null;
            }
        }

        // This method is used in both "=" and "AC"
        // I extracted the code into a method to avoid repeating myself
        private void Reset() // Note that reset and "AC" are not the same
        {
            _lastNumber = null;
            _selectedOperator = null;
        }
    }
}
