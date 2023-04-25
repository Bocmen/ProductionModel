using System.Collections.Generic;
using System.Linq;
using CoreProductionModel.Extensions;
using CoreProductionModel.Models;

namespace CoreProductionModel.Abstract
{
    public abstract class Sign: NodeOperator
    {
        public abstract int Token { get; }
        public abstract string GetValue();

        public override bool Check(IEnumerable<Sign> signs, IList<Sign> signsChecked, LoggerCondition logger = null)
        {
            if(logger.LoggerCurrentLevel(this, () => signs.Any(x => x.Token == Token)))
            {
                signsChecked?.Add(this);
                return true;
            }
            return false;
        }
        public override int GetMaxMandatorySigns() => 1;
        public override int GetCountSigns() => 1;
        public override bool Equals(object obj) => obj is Sign sign && Token == sign.Token;
        public override int GetHashCode() => Token.GetHashCode();
        public static bool operator ==(Sign left, Sign right) => EqualityComparer<Sign>.Default.Equals(left, right);
        public static bool operator !=(Sign left, Sign right) => !(left==right);
        public override IEnumerable<Sign> GetSigns() => Enumerable.Empty<Sign>().Append(this);
        public override bool CheckConflict(NodeOperator nodeOperator) => (nodeOperator is Sign sign && sign.Token == Token) || (nodeOperator is OrOperator orOperator && orOperator.Operators.Contains(this));
    }
}
