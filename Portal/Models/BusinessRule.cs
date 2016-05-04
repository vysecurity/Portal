using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.Models
{
    public class BusinessRuleContainer
    {
        public List<BusinessRuleList> BusinessRules { get; set; }
    }

    public class BusinessRuleList
    {   
        public string BusinessRuleName { get; set; }    
        public List<BRBranch> BRList { get; set; }
    }

    public class BRBranch
    {
        public string BranchID { get; set; }
        public List<BRCondition> ConditionList { get; set; }
        public string LogicalOperator { get; set; }
        public List<BRAction> ActionList { get; set; }
    }

    public class BRCondition
    {
        public string AttributeName { get; set; }   
        public string EntityName { get; set; }
        public string Operator { get; set; }
        public string ValueType { get; set; }
        public string Value { get; set; }
    }

    public class BRAction
    {
        public string AttributeName { get; set; }   
        public string EntityName { get; set; }
        public string Operation { get; set; }
        public string ValueType { get; set; }
        public string Value { get; set; }
    }
}