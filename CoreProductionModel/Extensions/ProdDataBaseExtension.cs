using CoreProductionModel.Abstract;
using CoreProductionModel.Models;
using CoreProductionModel.Tools;
using System.Collections.Generic;
using System.Linq;

namespace CoreProductionModel.Extensions
{
    public static class ProdDataBaseExtension
    {
        public const TypeDetectConflict DefaultTypeDetectConflict = TypeDetectConflict.Sequence;
        public static IEnumerable<Sign> GetSigns(this ProdDataBase prodDataBase)
        {
            foreach (var rule in prodDataBase.Rules)
            {
                yield return rule.Sign;
                foreach (var sign in rule.Operator.GetSigns())
                    yield return sign;
            }
            yield break;
        }
        public static bool TryAddDefaultCreateSign(this ProdDataBase prodDataBase, string expresionOperator, string consequence, out Rule ruleCreate, out Rule ruleConflict)
        {
            NodeOperator @operator = ParseWhereRule.Parse(expresionOperator, DefaultSign.Create);
            ruleCreate = new Rule(@operator, DefaultSign.Create(consequence), expresionOperator.Trim());
            return prodDataBase.TryAdd(ruleCreate, out ruleConflict);
        }
        public delegate void LoggerIteration(Rule selectRule, IEnumerable<RuleSelectInfo> detectConflictRule);
        public static IEnumerable<Sign> Check(this ProdDataBase prodDataBase, IEnumerable<Sign> signs, LoggerIteration loggerIteration = null, TypeDetectConflict typeDetectConflict = TypeDetectConflict.Intersect)
        {
            List<Sign> signsDatabase = new List<Sign>();
            List<Rule> rules = new List<Rule>();
            signsDatabase.AddRange(signs);
            rules.AddRange(prodDataBase.Rules/*.OrderByDescending(x => x.Operator.GetCountSigns())*/);
            List<Rule> rulesSelect = new List<Rule>();
            List<Sign> signWorkedConflict = null;
            List<Sign> signWorked = new List<Sign>();

            while (rules.Any())
            {
                rulesSelect.Clear();
                signWorked.Clear();
                signWorkedConflict = null;
                foreach (var rule in rules)
                {
                    if (rule.Operator.Check(signWorkedConflict ?? signsDatabase, signWorked))
                    {
                        if (rulesSelect.Any() && (typeDetectConflict == TypeDetectConflict.Intersect ? signWorked.Intersect(signWorkedConflict).Count() > 0 : !signWorked.Except(signWorkedConflict).Any()))
                        {
                            rulesSelect.Add(rule);
                            signWorked.Clear();
                        }
                        else
                        {
                            signWorkedConflict = signWorked;
                            signWorked = new List<Sign>();
                            rulesSelect.Clear();
                            rulesSelect.Add(rule);
                        }
                    }
                    else
                    {
                        signWorked.Clear();
                    }
                }
                if (rulesSelect.Any())
                {
                    Rule ruleSelect;
                    IEnumerable<RuleSelectInfo> conflictRules = null;
                    if (rulesSelect.Count > 1)
                    {
                        // механизм разрешения конфликтов
                        conflictRules = rulesSelect.Select(x => new RuleSelectInfo(x, x.Operator.GetMaxMandatorySigns()));
                        ruleSelect = conflictRules.OrderBy(x => x.MaxMandatorySigns).Last().Rule;
                    }
                    else
                        ruleSelect = rulesSelect.First();
                    foreach (var rule in rulesSelect) rules.Remove(rule);
                    signsDatabase.Add(ruleSelect.Sign);
                    loggerIteration?.Invoke(ruleSelect, conflictRules);
                }
                else
                    break;
            }
            return signsDatabase;
        }
        public struct RuleSelectInfo
        {
            public Rule Rule { get; private set; }
            public int MaxMandatorySigns { get; private set; }

            public RuleSelectInfo(Rule rule, int maxMandatorySigns)
            {
                Rule=rule;
                MaxMandatorySigns=maxMandatorySigns;
            }
        }
        public enum TypeDetectConflict: byte
        {
            Intersect,
            Sequence
        }
    }
}
