using System.Collections.Generic;
using System.Linq;

namespace CoreProductionModel.Abstract
{
    public abstract class MultipleOperator: NodeOperator
    {
        protected static readonly HelpConflictComparer ConflictComparer = new HelpConflictComparer();
        protected readonly NodeOperator[] _operators;
        public IReadOnlyCollection<NodeOperator> Operators => _operators;

        protected MultipleOperator(NodeOperator[] operators) => _operators=operators;
        protected string ToStringHelper(string typeName) => string.Join($" {typeName} ", _operators.Select(x => x is MultipleOperator ? $"({x})" : x.ToString()));
        public override IEnumerable<Sign> GetSigns() => _operators.SelectMany(x => x.GetSigns());
        public override int GetCountSigns() => Operators.Sum(x => x.GetCountSigns());

        public override bool Equals(object obj) => obj is MultipleOperator @operator && _operators.SequenceEqual(@operator.Operators);
        public override int GetHashCode() => -949511377 + EqualityComparer<NodeOperator[]>.Default.GetHashCode(_operators);
        public static bool operator ==(MultipleOperator left, MultipleOperator right) => EqualityComparer<MultipleOperator>.Default.Equals(left, right);
        public static bool operator !=(MultipleOperator left, MultipleOperator right) => !(left==right);

        protected class HelpConflictComparer: IEqualityComparer<NodeOperator>
        {
            public bool Equals(NodeOperator x, NodeOperator y) => x.CheckConflict(y);
            public int GetHashCode(NodeOperator obj) => obj.GetHashCode();
        }
    }
}
