using CoreProductionModel.Abstract;
using System.Collections.Generic;

namespace CoreProductionModel.Models
{
    public class Rule
    {
        public NodeOperator Operator { get; private set; }
        public Sign Sign { get; private set; }
        public string TextOperator { get; private set; }

        public Rule(NodeOperator @operator, Sign sign, string textOperator)
        {
            Operator=@operator;
            Sign=sign;
            TextOperator=textOperator;
        }

        public override bool Equals(object obj) =>obj is Rule rule && EqualityComparer<NodeOperator>.Default.Equals(Operator, rule.Operator) && EqualityComparer<Sign>.Default.Equals(Sign, rule.Sign); 
        public override int GetHashCode()
        {
            int hashCode = 369163881;
            hashCode=hashCode*-1521134295+EqualityComparer<NodeOperator>.Default.GetHashCode(Operator);
            hashCode=hashCode*-1521134295+EqualityComparer<Sign>.Default.GetHashCode(Sign);
            return hashCode;
        }
        public static bool operator ==(Rule left, Rule right) => EqualityComparer<Rule>.Default.Equals(left, right);
        public static bool operator !=(Rule left, Rule right) => !(left==right);
        public override string ToString() => $"ЕСЛИ [{Operator}] ТО [{Sign}]";
    }
}
