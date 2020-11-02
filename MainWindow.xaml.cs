using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFCalculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Class level var = static var
        private static double?
                lastNumber,
                result;

        private static SelectedOperator? selectedOperator;

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

            foreach (var elem in myGrid.Children)
            {

                if(elem is Button button)
                {


                    button.Click += HandleClick( button.Content.ToString() );
                }
            }
        }

        private RoutedEventHandler HandleClick(string content)
        {
            const string IS_NUMBER = @"^\d+$";
            const string IS_OPERATION = @"^(\+|\-|\*|\/)$";
            const string IS_SPECIAL_OPERATION = @"^(\+\/\-|\%|AC|=|\.)$";

            RoutedEventHandler REH = (e, s) => Trace.WriteLine("Unknown");

            if (Regex.IsMatch(content, IS_NUMBER))
            {
                REH = (e, s) => HandleNumber(content);
            }
            else if(Regex.IsMatch(content, IS_OPERATION))
            {
                REH = (e, s) => HandleOperation(content);
            }
            else if(Regex.IsMatch(content, IS_SPECIAL_OPERATION))
            {
                REH = (e, s) => HandleSpecialOperation(content);
            }

            return REH;
        }

        private void HandleOperation(string operation)
        {

            result = null;

            //if (selectedOperator != null) // Throw error if user changes the operation half way through
            //{
            //    throw new Exception("Wtf dude");
            //}

            switch (operation)
            {
                case "+": selectedOperator = SelectedOperator.Addition;         break;
                case "-": selectedOperator = SelectedOperator.Subtraction;      break;
                case "*": selectedOperator = SelectedOperator.Multiplication;   break;
                case "/": selectedOperator = SelectedOperator.Division;         break;
            }
        }

        private void HandleNumber(string number)
        {

            if (result != null)
            {
                result = null;
                Result.Content = "0";
            }

            if (selectedOperator != null && lastNumber == null) // Second number flow
            {
                lastNumber = double.Parse(Result.Content + "");
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

                            result = DoOperation(selectedOperator, double.Parse(lastNumber.ToString()),double.Parse(Result.Content.ToString()));
                            Result.Content = result;
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
                case "%":   result = null; selectedOperator = SelectedOperator.Addition; Result.Content = double.Parse(Result.Content + "") * 0.01; break;
                case ".":   if (!Result.Content.ToString().Contains(".")) Result.Content += "."; break;
            }
        }

        private double? DoOperation(SelectedOperator? SO, double o, double o1)
        {
            switch (SO)
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
            lastNumber = null;
            selectedOperator = null;
        }
    }
}
