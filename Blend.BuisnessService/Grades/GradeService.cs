using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blend.BuisnessEntities.TeaGrade;

namespace Blend.BuisnessService.Grades
{
    public class GradeService : BaseManagement<Grade>, IGradeService
    {
        public IEnumerable<BuisnessEntities.TeaGrade.Grade> GetGradeList()
        {
            try
            {
                var mapper = new DataMapper<Grade>("GetGrades", new Hashtable() { });
                var result = mapper.ObjectList();
                return result;
            }
            catch (Exception ex)
            {
                logger.LogWrite("GetGradeList fails. Error " + ex.Message);
                throw ex;
            }
        }

        public bool InsertGrade(string grade)
        {
            dataOperation.CommandText("trn_GradeInsert");
            dataOperation.AddParam("@GradeName", grade);
            dataOperation.DoExecute();
            return true;
        }
    }
}
