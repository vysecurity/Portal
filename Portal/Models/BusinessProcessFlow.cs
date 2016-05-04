using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.Models
{
    public class BusinessProcessFlow
    {
        public string ID { get; set; }
        public string ProcessID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string PrimaryEntityName { get; set; }
        public string NextStepIndex { get; set; }
        public bool IsCrmUIWorkflow { get; set; }
        public string Mode { get; set; }
        public List<StageItem> StageList { get; set; }
        public string SelectedProcessID { get; set; }
        public string SelectedStageID { get; set; }
        public string SelectedTraversedPath { get; set; }
    }

    public class StageItem
    {
        public string ID { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string StageID { get; set; }
        public string NextStageID { get; set; }
        public string StageCategory { get; set; }
        public string LabelID { get; set; }
        public string LanguageCode { get; set; }
        public string LabelDescription { get; set; }
        public string IsMainStage { get; set; }
        public List<StepItem> StepList { get; set; }
        public List<ConditionItem> ConditionList { get; set; }

    }

    public class StepItem
    {
        public string ID { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string LabelID { get; set; }
        public string LanguageCode { get; set; }
        public string LabelDescription { get; set; }
        public string StepID { get; set; }
        public bool IsProcessRequired { get; set; }
        public ControlItem ControlElement { get; set; }
    }
 
    public class ControlItem
    {
        public string ID { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string ControlID { get; set; }
        public string DataFieldName { get; set; }
        public string SystemStepType { get; set; }
        public bool IsSystemControl { get; set; }
        public string Parameters { get; set; }
        public string ControlDisplayName { get; set; }
        public ControlPropertyItem ControlProperty { get; set; }
    }

    public class ConditionItem
    {
        public string ID { get; set; }
        public string NextStageID { get; set; }
        public string ParentStageID { get; set; }
        public string EntityName { get; set; }
        public string AttributeName { get; set; }
        public string ConditionOperator { get; set; }
        public string Value { get; set; }
        public string ConditionType { get; set; }
        public string AndOr { get; set; }
        public string ContainsElsebranch { get; set; }
    }

    public class ControlPropertyItem 
    {
        public String ElementType { get; set; }
        public List<Picklist> PicklistValues { get; set; }
        public String MaxValue { get; set; }
        public String MinValue { get; set; }
        public String LogicalName { get; set; }
        public String DatePart { get; set; }
        public String BeforeTimeFormat { get; set; } 
        public String BeforeDateFormat { get; set; }
        public String TimeFormat { get; set; }
        public String DateFormat { get; set; }
        public String Precision { get; set; }
    }

}