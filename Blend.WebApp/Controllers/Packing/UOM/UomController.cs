using Blend.BuisnessEntities.Packing.UnitOfMeasure;
using Blend.BuisnessService.Packing.UOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Blend.WebApp.Controllers.Packing.UOM
{
    public class UomController : BaseApi
    {
        private IUomService _uomService;
        public UomController(IUomService uomService)
        {
            _uomService = uomService;
        }

        [HttpGet]
        [ActionName("GetUom")]
        public IEnumerable<Uom> Get()
        {
            IEnumerable<Uom> uoms = new List<Uom>();
            try
            {
                uoms = _uomService.GetUomList();
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");

            }
            return uoms;
        }

    }
}
