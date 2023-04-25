using System.Collections.Generic;
using System.Linq;

namespace CoreProductionModel.Abstract
{
    public abstract class NodeOperator
    {
        public abstract bool Check(IEnumerable<Sign> signs, IList<Sign> signsChecked, LoggerCondition logger = null);
        public int GetCountTryChekcNodes(IEnumerable<Sign> signs) => signs.Intersect(GetSigns()).Count();
        public override abstract string ToString();
        public abstract IEnumerable<Sign> GetSigns();
        public abstract bool CheckConflict(NodeOperator nodeOperator);
        public abstract int GetMaxMandatorySigns();
        public abstract int GetCountSigns();
    }
}
