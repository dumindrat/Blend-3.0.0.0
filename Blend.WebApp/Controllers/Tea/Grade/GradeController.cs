using Blend.BuisnessService.GardenMarks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Blend.BuisnessService.Grades;
using Blend.BuisnessEntities.TeaGrade;

namespace Blend.WebApp.Controllers.Tea.GardenMark
{
    public class GradeController : BaseApi
    {
         private IGradeService _gradeService;

         public GradeController(IGradeService gradeService)
        {
            _gradeService = gradeService;
        }
        // GET api/document
         public IEnumerable<Grade> Get()
        {
            return _gradeService.GetGradeList();
        }

    }
}
