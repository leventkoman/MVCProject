using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MyEvernote.DataAccessLayer;
using MyEvernote.Entities;
using MyEvernote.Common;
using MyEvernote.Core.DataAccess;

namespace MyEvernote.DataAccessLayer.EntityFramework
{
    public class Repository<T> : RepositoryBase , IDataAccess<T> where T: class
    {
        private DbSet<T> _objSet;

        public Repository()
        {
            _objSet = context.Set<T>();
        }

        public List<T> List()
        {
            return _objSet.ToList();
        }

        public IQueryable<T> ListQueryable()
        {
            return _objSet.AsQueryable<T>();
        }

        public List<T> List(Expression<Func<T,bool>> where)
        {
            return _objSet.Where(where).ToList();
        }

        public int Insert(T obj)
        {
            _objSet.Add(obj);

            if (obj is MyEntityBase)
            {
                MyEntityBase o =obj as MyEntityBase;
                DateTime now =DateTime.Now;
                o.CreatedOn = now;
                o.ModifiedOn = now;
                o.ModifiedUsername = App.Common.GetCurrentUsername();
            }
            return Save();
        }

        public int Update(T obj)//update için sadece yapılan değişiklikleri kaydeder.
        {
            if (obj is MyEntityBase)
            {
                MyEntityBase o = obj as MyEntityBase;

                o.ModifiedOn = DateTime.Now;
                o.ModifiedUsername = App.Common.GetCurrentUsername();
            }
            return Save();
        }

        public int Delete(T obj)
        {
            _objSet.Remove(obj);
            return Save();
        }

        public int Save()
        {
            return context.SaveChanges();
        }

        public T Find(Expression<Func<T, bool>> where)
        {
            return _objSet.FirstOrDefault(where);
        }
    }
}
