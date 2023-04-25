using CoreProductionModel.Abstract;
using CoreProductionModel.Models;
using ProductionModel.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CoreProductionModel.Tools
{
    public static class DotTreeDrawOperator
    {
        public delegate void Write(string value);
        public static int DrawTree(Write writer, NodeOperator nodeOperator, int startIndexator = 0) 
        {
            Queue<(int? parent, NodeOperator node)> openNodes = new Queue<(int? parent, NodeOperator node)>();
            openNodes.Enqueue((startIndexator, nodeOperator));
            while (openNodes.Any())
            {
                var (parent, node) = openNodes.Dequeue();
                if (node is MultipleOperator multipleOperator)
                {
                    foreach(var elem in multipleOperator.Operators)
                        openNodes.Enqueue((startIndexator, elem));
                }
                DrawNode(writer, parent, startIndexator, node, Color.Black);
                startIndexator++;
            }
            return startIndexator;
        }
        public static string CreateDot(NodeOperator nodeOperator)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("digraph G {");
            DrawTree((line) => sb.Append(line), nodeOperator);
            sb.Append('}');
            return sb.ToString();
        }

        private static string GetValueDot(NodeOperator @operator, out bool isMultuplyOparator)
        {
            isMultuplyOparator = false;
            if (@operator is Sign sign) return sign.GetValue();
            if (@operator is AndOperator) { isMultuplyOparator = true; return "И"; }
            if (@operator is OrOperator) { isMultuplyOparator = true; return "ИЛИ"; }
            throw new ArgumentOutOfRangeException(nameof(@operator));
        }
        private static string DecorateValueDot(string value, Color color, bool isMultuplyOparator) => $"[label=\"{value}\" shape={(isMultuplyOparator ? "hexagon" : "box")} color=\"{color.ToHex()}\"]";
        public static void DrawNode(Write writer, int? parentNodeIndex, int nodeIndex, NodeOperator node, Color color)
        {
            writer($"{nodeIndex} {DecorateValueDot(GetValueDot(node, out bool isMultuplyOparator), color, isMultuplyOparator)};");
            if (parentNodeIndex != null && parentNodeIndex != nodeIndex) writer($"{parentNodeIndex} -> {nodeIndex};");
        } 
    }
}
