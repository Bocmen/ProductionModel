using CoreProductionModel.Abstract;
using CoreProductionModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CoreProductionModel.Tools
{
    public static class ParseWhereRule
    {
        private readonly static Regex _splitRegex = new Regex(@"((?:\bи(?:ли){0,1}\b)|\(|\))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static NodeOperator Parse(string expression, Func<string, Sign> createSign)
        {
            Stack<object> nodes = new Stack<object>();
            ReversePolishNotation(expression, createSign, (node) =>
            {
                if (node is TypeNode typeNode)
                {
                    if (nodes.Count < 2) throw new Exception("Ошибка разбора не хватает операндов для применения оператора");
                    object rightValue = nodes.Pop();
                    object leftValue = nodes.Pop();
                    bool isCastOpLeftValue = TryCastOperatorData(leftValue, typeNode, out OperatorData operatorDataLeft);
                    bool isCastOpRightValue = TryCastOperatorData(rightValue, typeNode, out OperatorData operatorDataRight);
                    if (isCastOpLeftValue && isCastOpRightValue)
                    {
                        operatorDataLeft.Operators.AddRange(operatorDataRight.Operators);
                        nodes.Push(operatorDataLeft);
                    }
                    else if (isCastOpLeftValue)
                    {
                        operatorDataLeft.Operators.Add(CastNodeOperator(rightValue));
                        nodes.Push(operatorDataLeft);
                    }
                    else if (isCastOpRightValue)
                    {
                        operatorDataRight.Operators.Add(CastNodeOperator(leftValue));
                        nodes.Push(operatorDataRight);
                    }
                    else
                    {
                        nodes.Push(new OperatorData(typeNode, CastNodeOperator(rightValue), CastNodeOperator(leftValue)));
                    }
                }
                else
                {
                    nodes.Push(node);
                }
            });
            if (nodes.Count != 1 || !(nodes.Peek() is NodeOperator || nodes.Peek() is OperatorData)) throw new Exception("Ошибка разбора");
            return CastNodeOperator(nodes.Pop());
        }
        private static void ReversePolishNotation(string expression, Func<string, Sign> createSign, Action<object> addNode)
        {
            Stack<TypeNode> stack = new Stack<TypeNode>();
            foreach (var partExpression in _splitRegex.Split(expression).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()))
            {
                var typeNode = GetTypeNode(partExpression);
                if (typeNode != TypeNode.Value)
                {
                    if (stack.Any() && typeNode != TypeNode.Opening)
                    {
                        if (typeNode == TypeNode.Clousing)
                        {
                            TypeNode valuePop = stack.Pop();
                            while (valuePop != TypeNode.Opening)
                            {
                                addNode(valuePop);
                                valuePop = stack.Pop();
                            }
                        }
                        else
                        {
                            if (GetPriority(typeNode) > GetPriority(stack.Peek()))
                            {
                                stack.Push(typeNode);
                            }
                            else
                            {
                                while (stack.Any() && GetPriority(typeNode) <= GetPriority(stack.Peek()))
                                    addNode(stack.Pop());
                                stack.Push(typeNode);
                            }
                        }
                    }
                    else
                        stack.Push(typeNode);
                }
                else
                    addNode(createSign(partExpression));
            }
            if (stack.Any())
            {
                foreach (var elem in stack)
                    addNode(elem);
                stack.Clear();
            }
        }
        private static int GetPriority(TypeNode typeNode)
        {
            switch (typeNode)
            {
                case TypeNode.Opening:
                case TypeNode.Clousing:
                    return 0;
                case TypeNode.Or: return 1;
                case TypeNode.And: return 2;
                default: throw new ArgumentOutOfRangeException(nameof(typeNode));
            }
        }
        private static TypeNode GetTypeNode(string nodeValue)
        {
            nodeValue = nodeValue.ToLower();
            switch (nodeValue)
            {
                case "и": return TypeNode.And;
                case "или": return TypeNode.Or;
                case "(": return TypeNode.Opening;
                case ")": return TypeNode.Clousing;
                default: return TypeNode.Value;
            }
        }
        private static bool TryCastOperatorData(object value, TypeNode typeNode, out OperatorData data)
        {
            if (value is OperatorData @operator && @operator.Type == typeNode)
            {
                data = @operator;
                return true;
            }
            data = default;
            return false;
        }
        private static int GetCountDepth(object value)
        {
            if (value is OperatorData operatorData) return operatorData.Operators.Sum(x => GetCountDepth(x));
            if (value is MultipleOperator multipleOperator) return multipleOperator.Operators.Sum(x => GetCountDepth(x));
            return 1;
        }
        private static NodeOperator[] OrderCastOperators(IEnumerable<object> values) => values.OrderBy(x => GetCountDepth(x)).Select(x => CastNodeOperator(x)).ToArray();
        private static NodeOperator CastNodeOperator(object value)
        {
            if (value is OperatorData operatorData)
            {
                var distinct = operatorData.Operators.Distinct();
                if (distinct.Count() > 1)
                {
                    switch (operatorData.Type)
                    {
                        case TypeNode.Or: return new OrOperator(OrderCastOperators(distinct));
                        case TypeNode.And: return new AndOperator(OrderCastOperators(distinct));
                    }
                }
                else
                    return distinct.First();
            }
            else if (value is NodeOperator @operator)
                return @operator;
            throw new Exception("Ошибка разбора, не удалось преобразовать значение в оператор");
        }
        private enum TypeNode
        {
            Value,
            And,
            Or,
            Opening,
            Clousing
        }
        private struct OperatorData
        {
            public TypeNode Type { get; private set; }
            public List<NodeOperator> Operators { get; private set; }

            public OperatorData(TypeNode type, params NodeOperator[] operators)
            {
                Type=type;
                Operators = new List<NodeOperator>();
                if (operators != null && operators.Any())
                    Operators.AddRange(operators);
            }
        }
    }
}