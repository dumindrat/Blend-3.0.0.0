using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blend.BuisnessEntities.TeaGrade;

namespace Blend.BuisnessService.Grades
{
    public interface IGradeService
    {
        IEnumerable<Grade> GetGradeList();
        bool InsertGrade(string grade);
    }
}
