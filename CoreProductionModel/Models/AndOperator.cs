using CoreProductionModel.Abstract;
using System.Collections.Generic;
using System.Linq;
using CoreProductionModel.Extensions;

namespace CoreProductionModel.Models
{
    public class AndOperator : MultipleOperator
    {
        public AndOperator(params NodeOperator[] operators) : base(operators) { }

        public override bool Check(IEnumerable<Sign> signs, IList<Sign> signsChecked, LoggerCondition logger = null) => logger.LoggerCurrentLevel(this, () => _operators.All(x => x.Check(signs, signsChecked, logger)));
        public override bool CheckConflict(NodeOperator nodeOperator) => nodeOperator is AndOperator andOperator && andOperator.Operators.SequenceEqual(Operators, ConflictComparer);
        public override int GetMaxMandatorySigns() => Operators.Sum(x => x.GetMaxMandatorySigns());
        public override string ToString() => ToStringHelper("и");
    }
}
