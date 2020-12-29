using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelParaOptExcelAddIn.GA
{
    //https://blog.csdn.net/u011528448/article/details/25248241
    class MyException : ApplicationException
    {
        //public MyException(){}
        public MyException(string message) : base(message) { }

        public override string Message
        {
            get
            {
                return base.Message;
            }
        }
    }
}
