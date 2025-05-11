using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazingQuiz.Shared
{
    public interface IPlatform
    {
        bool IsMobileApp { get; }

        bool IsWeb { get; }
    }
}
