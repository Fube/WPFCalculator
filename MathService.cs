using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFCalculator
{
    static class MathService
    {

        public static double Add(double o, double o1) => o + o1;
        public static double Subtract(double o, double o1) => o - o1;
        public static double Multiply(double o, double o1) => o * o1;
        public static double Divide(double o, double o1){

            if (o1 == 0) // Have to force the exception cause it refuses to throw it by itself
                throw new DivideByZeroException("Can't divide by zero");

            return o / o1;
        }
    }
}
