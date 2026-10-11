using System;
using System.Collections.Generic;
using System.Globalization;

namespace Backend
{
    public static class ExpressionEvaluator
    {
        public static double Evalute(string infix) => EvalutePostfix(ToPostfix(infix));

        private static List<string> ToPostfix(string infix)
        {
            var postfix = new List<string>();
            var stack = new Stack<char>();
            var tokens = Tokenize(infix);

            foreach (var itemStr in tokens)
            {
                if (double.TryParse(itemStr, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                {
                    postfix.Add(itemStr);
                }
                else if (itemStr.Length == 1 && IsOperator(itemStr[0]))
                {
                    char item = itemStr[0];
                    if (item == ')')
                    {
                        if (stack.Count > 0)
                        {
                            var ope = stack.Pop();
                            while (ope != '(' && stack.Count >= 0)
                            {
                                postfix.Add(ope.ToString());
                                if (stack.Count > 0) ope = stack.Pop();
                                else break;
                            }
                        }
                    }
                    else
                    {
                        if (stack.Count == 0 || item == '(')
                        {
                            stack.Push(item);
                        }
                        else
                        {
                            if (PriorityInfix(item) > PriorityStack(stack.Peek()))
                            {
                                stack.Push(item);
                            }
                            else
                            {
                                while (stack.Count > 0 && PriorityInfix(item) <= PriorityStack(stack.Peek()))
                                {
                                    postfix.Add(stack.Pop().ToString());
                                }
                                stack.Push(item);
                            }
                        }
                    }
                }
            }

            while (stack.Count > 0)
            {
                var op = stack.Pop();
                if (op != '(')
                {
                    postfix.Add(op.ToString());
                }
            }
            return postfix;
        }

        private static List<string> Tokenize(string expression)
        {
            var tokens = new List<string>();
            string numberBuffer = "";

            for (int i = 0; i < expression.Length; i++)
            {
                char c = expression[i];

                if (char.IsDigit(c) || c == '.')
                {
                    numberBuffer += c;
                }
                else
                {
                    if (!string.IsNullOrEmpty(numberBuffer))
                    {
                        tokens.Add(numberBuffer);
                        numberBuffer = "";
                    }

                    if (c == '-')
                    {
                        bool isUnary = (i == 0) || (expression[i - 1] == '(' || expression[i - 1] == '+' || expression[i - 1] == '-' || expression[i - 1] == '*' || expression[i - 1] == '/' || expression[i - 1] == '^');
                        if (isUnary)
                        {
                            numberBuffer += c;
                            continue;
                        }
                    }

                    if (!char.IsWhiteSpace(c))
                    {
                        tokens.Add(c.ToString());
                    }
                }
            }

            if (!string.IsNullOrEmpty(numberBuffer))
            {
                tokens.Add(numberBuffer);
            }

            return tokens;
        }

        private static int PriorityStack(char op) => op switch
        {
            '^' => 3,
            '*' => 2,
            '/' => 2,
            '+' => 1,
            '-' => 1,
            '(' => 0,
            _ => 0,
        };

        private static int PriorityInfix(char op) => op switch
        {
            '^' => 4,
            '*' => 2,
            '/' => 2,
            '+' => 1,
            '-' => 1,
            '(' => 5,
            _ => 0,
        };

        private static bool IsOperator(char item) => item == '^' || item == '*' || item == '/' || item == '+' || item == '-' || item == '(' || item == ')';

        private static double EvalutePostfix(List<string> postfix)
        {
            var stack = new Stack<double>();
            foreach (var item in postfix)
            {
                if (item.Length == 1 && IsOperator(item[0]))
                {
                    if (stack.Count < 2) throw new InvalidOperationException("Expresión inválida.");
                    var ope2 = stack.Pop();
                    var ope1 = stack.Pop();
                    stack.Push(Calculate(ope1, ope2, item[0]));
                }
                else
                {
                    if (double.TryParse(item, NumberStyles.Any, CultureInfo.InvariantCulture, out double num))
                    {
                        stack.Push(num);
                    }
                }
            }
            return stack.Count > 0 ? stack.Pop() : 0;
        }

        private static double Calculate(double ope1, double ope2, char item)
        {
            return item switch
            {
                '*' => ope1 * ope2,
                '/' => ope2 != 0 ? ope1 / ope2 : throw new DivideByZeroException("Division by zero."),
                '+' => ope1 + ope2,
                '-' => ope1 - ope2,
                '^' => Math.Pow(ope1, ope2),
                _ => throw new Exception("Invalid operator."),
            };
        }
    }
}