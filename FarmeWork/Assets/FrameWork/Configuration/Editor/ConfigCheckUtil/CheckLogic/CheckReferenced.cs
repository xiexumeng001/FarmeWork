using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ShipFarmeWork.Configuration.Editor
{
    /// <summary>
    /// 检查表关联
    /// </summary>
    public class CheckReferenced : CheckLogicBase
    {
        public override string Check(CheckTableData data)
        {
            string referenceName = data["ReferencedConfigName"];
            string referenceAttributeName = data["ReferencedConfigIdAttribuName"];

            Table config = ConfigCheckUtil.LoadConfig(data.ConfigName);
            Table referenceConfig = ConfigCheckUtil.LoadConfig(referenceName);

            string log = "";
            bool isHasIgon = (data.IgronNum != "");
            foreach (var item in config.AllTableData)
            {
                string attriNum = item[data.AttriName];
                if (isHasIgon && attriNum == data.IgronNum)
                {
                    continue;
                }
                if (!referenceConfig.IsHasConfigDataByAttribute(referenceAttributeName, attriNum))
                {
                    log = log + "  " + item[data.KeyName];
                }
            }
            if (!string.IsNullOrEmpty(log))
            {
                log = string.Format("{0}表的{1}属性未引导到{2}表的有:{3}", data.ConfigName, data.AttriName, referenceName, log);
            }
            return log;
        }
    }
}