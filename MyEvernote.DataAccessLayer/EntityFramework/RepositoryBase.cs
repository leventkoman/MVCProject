using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyEvernote.DataAccessLayer;

namespace MyEvernote.DataAccessLayer.EntityFramework
{
   public class RepositoryBase
   {
       protected static DatabaseContext context;
       private static object lockobj=new object();

       protected RepositoryBase()
       {
           CreateContext();
       }

       private static void CreateContext()
       {
           if (context==null)
           {
               lock (lockobj)
               {
                   if (context==null)
                   {
                       context = new DatabaseContext();
                    }
               }

           }
       }
   }
}
