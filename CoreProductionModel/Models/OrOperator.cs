using CoreProductionModel.Abstract;
using System.Collections.Generic;
using System.Linq;
using CoreProductionModel.Extensions;

namespace CoreProductionModel.Models
{
    public class OrOperator : MultipleOperator
    {
        public OrOperator(params NodeOperator[] operators) : base(operators) { }

        public override bool Check(IEnumerable<Sign> signs, IList<Sign> signsChecked, LoggerCondition logger = null) => logger.LoggerCurrentLevel(this, () =>
        {
            bool result = false;
            foreach (var @operator in Operators)
            {
                if (@operator.Check(signs, signsChecked, logger))
                    result = true;
            }
            return result;
        });
        public override string ToString() => ToStringHelper("или");
        public override bool CheckConflict(NodeOperator nodeOperator) => nodeOperator is OrOperator orOperator && orOperator.Operators.Intersect(Operators, ConflictComparer).Count() > 0;
        public override int GetMaxMandatorySigns() => Operators.Min(x => x.GetMaxMandatorySigns());
    }
}
