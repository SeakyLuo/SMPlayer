using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Interfaces
{
    public interface ISearchEvaluator
    {
        // 计算该实体和关键词的匹配程度
        double Match(string keyword);
        // 评估该实体和其他不同类型的实体相比是否更匹配
        double Evaluate(string keyword);
    }
}
