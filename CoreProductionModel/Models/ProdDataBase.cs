using System.Collections.Generic;
using System.Linq;

namespace CoreProductionModel.Models
{
    public class ProdDataBase
    {
        private readonly List<Rule> _rules = new List<Rule>();
        public IReadOnlyCollection<Rule> Rules => _rules;

        public bool TryAdd(Rule rule, out Rule ruleConflict)
        {
            ruleConflict = _rules.FirstOrDefault(x => x.Operator.CheckConflict(rule.Operator));
            if(ruleConflict != null) return false;
            _rules.Add(rule);
            return true;
        }
        public bool TryRemove(Rule rule) => _rules.Remove(rule);
    }
}
