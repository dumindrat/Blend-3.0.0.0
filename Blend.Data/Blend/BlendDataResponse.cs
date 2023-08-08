using System.Collections.Generic;
using Blend.Data.Response;

namespace Blend.Data.Blend
{
    public class BlendDataResponse
    {
        public BlendInfo BlendHeader { get; set; }
        public IList<BlendDetail> BlendDetails { get; set; }
        public ResponseData Message { get; set; }

    }
}
