using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelParaOptExcelAddIn
{
    public static class EnumClass
    {
        
        /// <summary>
        ///CSUH,0-Unified Harding model for Clays and Sands（砂土和粘土的统一硬化模型）  
        ///UH,1-Unified Harding model（适用于超固结土的统一硬化模型）
        ///MCC,2-Modified Cam-Clay model(修正剑桥模型) 
        ///CamClay,3-Cam-Clay model剑桥模型   
        ///DuncanEB,4-Duncan-Chang Elestic-Bulk modulus model
        /// </summary>
        public enum ConstitutiveModelType
        {
            CSUH, 
            UH,
            MCC,
            DuncanEB
        }
        /// <summary>
        ///IsoCom,0-Isotropic compression drainage test
        ///CD,1-Triaxial consolidation drainage shear test
        ///CU,2-Triaxial consolidated undrained shear test
        ///K0,3-Lateral compression drainage test
        ///ConstP4-Constant p drainage shear test
        /// </summary>
        public enum TestType
        {
            IsoCom,
            CD,
            CU,
            K0,
            ConstP
        }


        public enum PaticipateInCompute
        {
            Part,
            All
        }





    }
}
