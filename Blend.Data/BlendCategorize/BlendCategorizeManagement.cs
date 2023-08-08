using System;

namespace Blend.Data.BlendCategorize
{
    public class BlendCategorizeManagement
    {
        private readonly BlendCategorize _categorizeInfo;

        public BlendCategorizeManagement(BlendCategorize categorizeInfo)
        {
            _categorizeInfo = categorizeInfo;
        }

        public BlendCategorize GetBlendCategorizeInfo()
        {
            try
            {
                if (_categorizeInfo.PhysicalQty < _categorizeInfo.ExportQty)
                {
                    throw new ArgumentException("Unsuccessful blending.Unable to proceed");
                }
                CalculateBB();
                CalculateBG();
                CalculatedSifted();
                if (_categorizeInfo.PhysicalQty < _categorizeInfo.BlendQty)
                    CalculateLoss();
                return _categorizeInfo;
            }
            catch (Exception ex)
            {
                //Log
                throw;
            }
        }

        private void CalculateBB()
        {
            _categorizeInfo.BB = _categorizeInfo.BlendQty - _categorizeInfo.ExportQty;

        }

        private void CalculateBG()
        {
            _categorizeInfo.BG = _categorizeInfo.PhysicalQty > _categorizeInfo.BlendQty ? _categorizeInfo.PhysicalQty - _categorizeInfo.BlendQty : 0;
        }

        private void CalculatedSifted()
        {
            if (_categorizeInfo.BG >= _categorizeInfo.SFT)
            {
                _categorizeInfo.SFTBG = _categorizeInfo.SFT;
                _categorizeInfo.STDG = _categorizeInfo.BG - _categorizeInfo.SFT;
            }
            if (_categorizeInfo.BG < _categorizeInfo.SFT)
            {
                _categorizeInfo.SFTBG = _categorizeInfo.BG;
                _categorizeInfo.SFTBB = _categorizeInfo.SFT - _categorizeInfo.BG;
            }
        }

        private void CalculateLoss()
        {
            _categorizeInfo.Loss = (_categorizeInfo.BlendQty - _categorizeInfo.PhysicalQty) + _categorizeInfo.SFT;
        }
    }
}
