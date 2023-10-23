using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VD.Blaze.Parser
{
    public abstract class Expression
    {
        public interface IVisitor
        {
            void VisitNumber(Number number);
        }

        public abstract void Accept(IVisitor visitor);



        public class Number : Expression
        {
            public override void Accept(IVisitor visitor)
            {
                visitor.VisitNumber(this);
            }
        }
    }
}
