using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
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

    public class ControlPropertyItem
    {
        
        public String ElementType { get; set; }
        
        public List<Picklist> PicklistValues { get; set; }
        
        public String DatePart { get; set; }
        
        public String BeforeTimeFormat { get; set; }
        
        public String BeforeDateFormat { get; set; }
        
        public String MaxValue { get; set; }
        
        public String MinValue { get; set; }
        
        public String LogicalName { get; set; }
        
        public String TimeFormat { get; set; }
        
        public String DateFormat { get; set; }
        
        public String Precision { get; set; }
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


    public interface ISteps { }
    public interface IExpression { }

    public class Steps
    {
        public List<ISteps> list { get; set; }
    }

    public class StepLabels
    {
        public List<StepLabelItem> list { get; set; }
    }

    public class StepLabelItem
    {
        public Guid? labelId { get; set; }
        public int languageCode { get; set; }
        public string description { get; set; }
    }

    public class WorkflowStep
    {
        public string __class { get; set; }
        public string id { get; set; }
        public string description { get; set; }
        public string name { get; set; }
        public StepLabels stepLabels { get; set; }
        public Steps steps { get; set; }
        public string primaryEntityName { get; set; }
        public string nextStepIndex { get; set; }
        public bool isCrmUIWorkflow { get; set; }
        public string category { get; set; }
        public string mode { get; set; }
        public string title { get; set; }
    }

    public class EntityStep : ISteps
    {
        public string __class { get; set; }
        public string id { get; set; }
        public string description { get; set; }
        public string name { get; set; }
        public StepLabels stepLabels { get; set; }
        public Steps steps { get; set; }
        public object relationshipName { get; set; }
        public bool isClosedLoop { get; set; }
    }

    public class StageStep : ISteps
    {

        public string __class { get; set; }
        public string id { get; set; }
        public string description { get; set; }
        public string name { get; set; }
        public StepLabels stepLabels { get; set; }
        public Steps steps { get; set; }
        public Guid? stageId { get; set; }
        public Guid? nextStageId { get; set; }
        public int? stageCategory { get; set; }
    }

    public class StepStep : ISteps
    {
        public string __class { get; set; }
        public string id { get; set; }
        public string description { get; set; }
        public string name { get; set; }
        public StepLabels stepLabels { get; set; }
        public Steps steps { get; set; }
        public Guid? stepStepId { get; set; }
        public bool isProcessRequired { get; set; }
    }

    public class ConditionStep : ISteps
    {
        public string __class { get; set; }
        public string id { get; set; }
        public string description { get; set; }
        public string name { get; set; }
        public StepLabels stepLabels { get; set; }
        public Steps steps { get; set; }
        public bool containsElsebranch { get; set; }
    }

    public class ConditionBranchStep : ISteps
    {
        public string __class { get; set; }
        public string id { get; set; }
        public string description { get; set; }
        public string name { get; set; }
        public StepLabels stepLabels { get; set; }
        public Steps steps { get; set; }
        public BinaryExpression conditionExpression { get; set; }
    }

    public class BinaryExpression : IExpression
    {
        public string __class { get; set; }
        public string type { get; set; }
        public bool typeSet { get; set; }
        public string conditionOperatoroperator { get; set; }
        public IExpression left { get; set; }
        public List<IExpression> right { get; set; }
    }

    public class EntityAttributeExpression : IExpression
    {
        public string __class { get; set; }
        public string type { get; set; }
        public string typeSet { get; set; }
        public PrimaryEntity entity { get; set; }
        public string attributeName { get; set; }
    }

    public class UnaryExpression : IExpression
    {
        public string __class { get; set; }
        public string type { get; set; }
        public string typeSet { get; set; }
        public string conditionOperatoroperator { get; set; }
        public EntityAttributeExpression operand { get; set; }
    }

    public class PrimitiveExpression : IExpression
    {
        public string __class { get; set; }
        public string type { get; set; }
        public bool typeSet { get; set; }
        public string primitiveValue { get; set; }
    }

    public class PrimaryEntity
    {
        public string __class { get; set; }
        public string parameterName { get; set; }
        public string entityName { get; set; }
    }

    public class SetNextStageStep : ISteps
    {
        public string __class { get; set; }
        public string id { get; set; }
        public string description { get; set; }
        public string name { get; set; }
        public StepLabels stepLabels { get; set; }
        public Guid? stageId { get; set; }
        public Guid? parentStageId { get; set; }
    }

    public class ControlStep : ISteps
    {
        public string __class { get; set; }
        public string id { get; set; }
        public string description { get; set; }
        public string name { get; set; }
        public StepLabels stepLabels { get; set; }
        public string controlId { get; set; }
        public Guid? classId { get; set; }
        public string dataFieldName { get; set; }
        public string SystemStepType { get; set; }
        public bool isSystemControl { get; set; }
        public string parameters { get; set; }
        public string controlDisplayName { get; set; }
    }

    public class CustomJavaScriptStep
    {
        public string javascript { get; set; }
    }
}
