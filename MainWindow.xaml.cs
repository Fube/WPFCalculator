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

            foreach (var elem in MyGrid.Children)
            {

                if(elem is Button button)
                {
                    button.Click += HandleClick( button.Content.ToString() );
                }
            }
        }

        private RoutedEventHandler HandleClick(string content)
        {
            const string isNumber = @"^\d+$";
            const string isOperation = @"^(\+|\-|\*|\/)$";
            const string isSpecialOperation = @"^(\+\/\-|\%|AC|=|\.)$";

            RoutedEventHandler reh = (e, s) => Trace.WriteLine("Unknown");

            if (Regex.IsMatch(content, isNumber))
            {
                reh = (e, s) => HandleNumber(content);
            }
            else if(Regex.IsMatch(content, isOperation))
            {
                reh = (e, s) => HandleOperation(content);
            }
            else if(Regex.IsMatch(content, isSpecialOperation))
            {
                reh = (e, s) => HandleSpecialOperation(content);
            }

            return reh;
        }

        private void HandleOperation(string operation)
        {

            _result = null;

            switch (operation)
            {
                case "+": _selectedOperator = SelectedOperator.Addition;         break;
                case "-": _selectedOperator = SelectedOperator.Subtraction;      break;
                case "*": _selectedOperator = SelectedOperator.Multiplication;   break;
                case "/": _selectedOperator = SelectedOperator.Division;         break;
            }
        }

        private void HandleNumber(string number)
        {

            if (_result != null)
            {
                _result = null;
                Result.Content = "0";
            }

            if (_selectedOperator != null && _lastNumber == null) // Second number flow
            {
                _lastNumber = double.Parse(Result.Content + "");
                Result.Content = number;
            }
            else // Normal flow
            {
                Result.Content = Result.Content.ToString().Equals("0") ? number : Result.Content + number; // Checks to see if it is the first ever entry, if yes: override otherwise: append
            }
        }

        private void HandleSpecialOperation(string operation)
        {

            switch (operation)
            {
                case "=":
                    {
                        try
                        {

                            _result = DoOperation(_selectedOperator, double.Parse(_lastNumber.ToString()),double.Parse(Result.Content.ToString()));
                            Result.Content = _result;
                        }
                        catch (Exception ex) when (ex is ArithmeticException || ex is DivideByZeroException) // Division by zero is legal apparently. It just returns Infinity...
                        {
                            Result.Content = "That is mathematically impossible";
                        }
                        catch (Exception e)
                        {
                            Trace.WriteLine(e.StackTrace);
                        }

                        Reset();
                        break;
                    }
                case "AC":  Reset(); Result.Content = 0; break;
                case "+/-": Result.Content = double.Parse(Result.Content + "") * -1; break;
                case "%":   _result = null; _selectedOperator = SelectedOperator.Addition; Result.Content = double.Parse(Result.Content + "") * ((_lastNumber ?? 1) / 100); break;
                case ".":   if (!Result.Content.ToString().Contains(".")) Result.Content += "."; break;
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

        private void Reset() // Note that reset and "AC" are not the same
        {
            _lastNumber = null;
            _selectedOperator = null;
        }
    }
}
