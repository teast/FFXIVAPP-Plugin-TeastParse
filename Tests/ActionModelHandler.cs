using System.Data;
using Dapper;
using FFXIVAPP.Plugin.TeastParse.Models;

namespace Tests
{
    public class ActionModelHandler : SqlMapper.TypeHandler<ActionModel>
    {
        public override ActionModel Parse(object value)
        {
            return new ActionModel(value.ToString(), ActionCategory.Item);
        }

        public override void SetValue(IDbDataParameter parameter, ActionModel value)
        {
            parameter.Value = value.Name;
        }
    }
}