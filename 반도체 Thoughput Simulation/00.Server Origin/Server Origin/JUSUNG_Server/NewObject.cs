using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JUSUNG_Server
{
    public class NewObject
    {
        public struct EFEMOBJECT
        {
            //=======필요한 데이터 첨삭======//
            public int nLPM_Curcnt;

            public int nAspeedPick;
            public int nAspeedPlace;
            public int nAspeedRotate;
            public int nLLVentTime;
            public int nLLVentStabTime;
            public int nAlgTime;
            public int LLwafer;
            public int nLL;

        }
        public EFEMOBJECT EFEMobj;

        public struct TMOBJECT
        {
            //=======필요한 데이터 첨삭======//
            public int nLLPumpTime;
            public int nLLPumpStabTime;
            public int nLLVentTime;
            public int nLLVentStabTime;

            public int nVspeedPick;
            public int nVspeedPlace;
            public int nVspeedRotate;
            public int nVac;
            public int nPM;
        }
        public TMOBJECT TMobj;

        public struct PMOBJECT
        {
            public int nPM;
            public int nPMWafer;
            public int nProcessTime;
        }
        public PMOBJECT PMobj;

        public struct COMPOBJECT
        {
            public int LLpos;
            public int PMpos;
            public int nWafer;

        }
        public COMPOBJECT ComObj;
        public List<COMPOBJECT> list = new List<COMPOBJECT>();
        public int ckTime;
        public int speed;
        public int doorSpeed;
    }
}
